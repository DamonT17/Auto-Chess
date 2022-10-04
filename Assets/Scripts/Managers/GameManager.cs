using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

// INHERITANCE
// Inherited manager class to handle all overhead game states, mechanics, object generations, etc. 
public class GameManager : Manager<GameManager> {
    // Assignables
    public AgentDatabaseSO AgentDatabase;
    public List<AgentDatabaseSO.AgentData> AgentPool;
    public UIShop AgentShop;

    public SpawnManager Spawn;

    public Slider RoundSlider;
    public TextMeshProUGUI RoundTimerText;
    public TextMeshProUGUI RoundNumberText;

    public TextMeshProUGUI AllowedAgentsText;

    // Flags
    public bool IsGameActive;
    public bool IsTimerActive;

    // Constants
    public const int GameStateCarousel = 0;
    public const int GameStatePrep     = 1;
    public const int GameStateFight    = 2;
    public const int GameStateBuffer   = 3;

    private const int _carouselLength    = 10;  // Carousel timer length (s)
    private const int _roundPrepLength   = 15;  // Round preparation timer length between rounds (s)
    private const int _roundLength       = 30;  // Round timer length (s)
    private const int _roundBufferLength = 2;   // Buffer timer length between states (s)

    private readonly int[] _agentPoolSize = { 24, 18, 15, 10, 9 };    // # of Agents/Agent Cost
    
    // Variables
    public int[] GameState;               // Linear buffer of game states
    public int GameStateIndex;
    public int LastGameState;

    public int[] TimerLength;

    private List<Agent> _myAgents = new List<Agent>();
    private List<Agent> _enemyAgents = new List<Agent>();

    public int TeamSize = 0;
    public int CurrentTeamSize = 0;

    public Action OnRoundStart;
    public Action OnRoundEnd;
    public Action<Agent> OnAgentDeath;

    public enum Team {
        Team1,
        Team2
    } // List of teams for game

    // Awake is called when the script instance is being loaded
    protected new void Awake() {
        base.Awake();

        GenerateAgentPool();
    }

    // Start is called before the first frame update
    private void Start() {
        GameState = new int[5];     // Initialize size of game state linear buffer
        TimerLength = new int[] {
            _carouselLength,
            _roundPrepLength,
            _roundLength,
            _roundBufferLength
        }; // Initialize size and values of main game timer values

        AllowedAgentsText.text = $"{CurrentTeamSize}/{TeamSize}";

        IsGameActive = true;        // Initialize game start

        if (IsGameActive) {
            SetGameState(GameStatePrep);   // Initialize game state

            StartCoroutine(Timer(TimerLength[GetGameState()]));
        }
    }

    // Called every fixed frame-rate frame
    private void FixedUpdate() {
        if (!IsTimerActive) {
            GameStateIndex = UpdateGameStateIndex(GameStateIndex);

            switch (LastGameState) {
                case GameStateCarousel:
                    SetGameState(GameStatePrep);
                    AllowedAgentsText.gameObject.SetActive(true);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;

                case GameStatePrep:
                    SetGameState(GameStateFight);
                    AllowedAgentsText.gameObject.SetActive(false);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", true);
                    break;

                case GameStateFight:
                    SetGameState(GameStateBuffer);
                    AllowedAgentsText.gameObject.SetActive(false);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;

                case GameStateBuffer:
                    SetGameState(GameStatePrep);
                    AllowedAgentsText.gameObject.SetActive(true);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;
            }

            StartCoroutine(Timer(TimerLength[GetGameState()]));
        }
    }

    // Game state round(s) timer
    private IEnumerator Timer(int roundLength) {
        float timer = roundLength;
        RoundSlider.maxValue = roundLength;

        IsTimerActive = true;

        while (IsGameActive && IsTimerActive) {
            yield return new WaitForEndOfFrame();

            if (timer <= 0) {
                RoundSlider.value = RoundSlider.maxValue;
                IsTimerActive = false;

                LastGameState = GetGameState();
                break;
            }

            timer -= Time.deltaTime;
            RoundTimerText.text = Mathf.RoundToInt(timer).ToString();
            RoundSlider.value = timer;
        }
    }

    // ABSTRACTION
    // Generation of Agent cards database 
    public void GenerateAgentPool() {
        AgentPool = new List<AgentDatabaseSO.AgentData>();

        for (var i = 0; i < AgentDatabase.AllAgents.Count; i++) {
            int cost = AgentDatabase.AllAgents[i].AgentCost;

            // Populate # of Agent cards based on Agent cost
            for (var j = 0; j < _agentPoolSize[cost - 1]; j++) {
                AgentPool.Add(AgentDatabase.AllAgents[i]);
            }
        }
    }

    // ABSTRACTION
    // Adds agent to player's team
    // NOTE: Need to input variable of which team to add agent to for multi player)
    public void AddAgent(AgentDatabaseSO.AgentData agent) {
        Agent newAgent = Instantiate(agent.AgentPrefab);        // Need bench position and rotation
        newAgent.gameObject.name = agent.AgentName;
        _myAgents.Add(newAgent);

        newAgent.Setup(Team.Team1, GridManager.Instance.GetFreeNode(Team.Team1, GridManager.Instance.BenchGraph));
    }

    // ABSTRACTION
    // Set the game's current game state
    private void SetGameState(int gameState) {
        GameState[GameStateIndex] = gameState;
    }

    // ABSTRACTION
    // Obtain current game state
    private int GetGameState() {
        return GameState[GameStateIndex];
    }

    // ABSTRACTION
    // Update the game state's index position
    private int UpdateGameStateIndex(int index) {
        if (index >= GameState.Length - 1) // Reset buffer index position
            index = 0;
        else index++;

        return index;
    }

    // ABSTRACTION
    // Returns all agents for corresponding team
    public List<Agent> GetAgents(Team team) {
        return team != Team.Team1 ? _enemyAgents : _myAgents;
    }

    // ABSTRACTION
    // REMOVE AGENT FROM FIELD ON DEATH | WORK ON MORE
    public void AgentDead(Agent agent) {
        _myAgents.Remove(agent);    //???
        _enemyAgents.Remove(agent); //???

        OnAgentDeath?.Invoke(agent);

        agent.gameObject.SetActive(false);
    }

    // ABSTRACTION
    // Debug method for generating enemies to fight
    public void DebugBuild() {
        foreach (Tile tile in GridManager.Instance.EnemyTiles)
            tile.gameObject.SetActive(true);

        for (var i = 0; i < PlayerManager.Instance.PlayerLevel; i++) {
            int randomIndex = UnityEngine.Random.Range(0, AgentDatabase.AllAgents.Count);
            Agent agent = Instantiate(AgentDatabase.AllAgents[randomIndex].AgentPrefab);
            agent.gameObject.name = AgentDatabase.AllAgents[randomIndex].AgentName;
            _enemyAgents.Add(agent);

            // Create generic code to place enemy agents on battlefield
            agent.Setup(Team.Team2, GridManager.Instance.GetFreeNode(Team.Team2, GridManager.Instance.Graph));
        }
    }

    // ABSTRACTION
    // Debug method to fight enemies
    public void DebugFight() {
        // Add code for fighting here...
    }
}

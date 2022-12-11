using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UmbraProjects.AutoChess.Agents;
using UmbraProjects.AutoChess.UI;
using UnityEngine;
using UnityEngine.UI;
using UmbraProjects.Managers;
using Random = System.Random;

namespace UmbraProjects.AutoChess {
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
        public bool IsFightActive;

        // Constants
        public const int GAME_STATE_CAROUSEL = 0;
        public const int GAME_STATE_PREP = 1;
        public const int GAME_STATE_FIGHT = 2;
        public const int GAME_STATE_BUFFER = 3;

        private const int _CAROUSEL_LENGTH = 10; // Carousel timer length (s)
        private const int _ROUND_PREP_LENGTH = 15; // Round preparation timer length between rounds (s)
        private const int _ROUND_LENGTH = 30; // Round timer length (s)
        private const int _ROUND_BUFFER_LENGTH = 3; // Buffer timer length between states (s)

        private readonly int[] _agentPoolSize = {24, 18, 15, 10, 9}; // # of Agents/Agent Cost

        // Variables
        public int[] GameState; // Linear buffer of game states
        public int GameStateIndex;
        public int LastGameState;

        public int[] GameStateTimerLength;

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
            GameState = new int[5]; // Initialize size of game state linear buffer
            GameStateTimerLength = new int[]
            {
                _CAROUSEL_LENGTH,
                _ROUND_PREP_LENGTH,
                _ROUND_LENGTH,
                _ROUND_BUFFER_LENGTH
            }; // Initialize size and values of main game timer values

            AllowedAgentsText.text = $"{CurrentTeamSize}/{TeamSize}";

            IsGameActive = true; // Initialize game start

            if (IsGameActive) {
                SetGameState(GAME_STATE_PREP); // Initialize game state

                StartCoroutine(GameStateTimer(GameStateTimerLength[GetGameState()]));
            }
        }

        // Called every fixed frame-rate frame
        private void FixedUpdate() {
            if (!IsTimerActive) {
                GameStateIndex = SetGameStateIndex(GameStateIndex);

                switch (LastGameState) {
                    case GAME_STATE_CAROUSEL:
                        SetGameState(GAME_STATE_PREP);
                        AllowedAgentsText.gameObject.SetActive(true);
                        PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                        break;

                    case GAME_STATE_PREP:
                        SetGameState(GAME_STATE_FIGHT);
                        AllowedAgentsText.gameObject.SetActive(false);
                        PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", true);

                        break;

                    case GAME_STATE_FIGHT:
                        SetGameState(GAME_STATE_BUFFER);
                        AllowedAgentsText.gameObject.SetActive(false);
                        PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                        break;

                    case GAME_STATE_BUFFER:
                        SetGameState(GAME_STATE_PREP);
                        AllowedAgentsText.gameObject.SetActive(true);
                        PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                        break;
                }

                StartCoroutine(GameStateTimer(GameStateTimerLength[GetGameState()]));
            }
        }

        // Game state round(s) timer
        private IEnumerator GameStateTimer(int roundLength) {
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
            Agent newAgent = Instantiate(agent.AgentPrefab); // Need bench position and rotation
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
        private int SetGameStateIndex(int index) {
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
            _myAgents.Remove(agent); //???
            _enemyAgents.Remove(agent); //???

            OnAgentDeath?.Invoke(agent);

            agent.gameObject.SetActive(false);
        }

        // ABSTRACTION
        // Debug method for generating enemies to fight
        public void DebugBuild() {
            for (var i = 0; i < PlayerManager.Instance.PlayerLevel; i++) {
                // int randomIndex = UnityEngine.Random.Range(0, AgentDatabase.AllAgents.Count);
                // Agent agent = Instantiate(AgentDatabase.AllAgents[randomIndex].AgentPrefab);

                Agent agent = Instantiate(AgentDatabase.AllAgents[0].AgentPrefab);
                agent.gameObject.name = AgentDatabase.AllAgents[0].AgentName;
                _enemyAgents.Add(agent);

                // Create generic code to place enemy agents on battlefield
                agent.Setup(Team.Team2, GridManager.Instance.GetFreeNode(Team.Team2, GridManager.Instance.Graph));
            }
        }

        // ABSTRACTION
        // Debug method to fight enemies
        public void DebugFight() {
            // Add code for fighting here...

            IsFightActive = !IsFightActive;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UmbraProjects.AutoChess.Agents;
using UmbraProjects.AutoChess.UI;
using UnityEngine;
using UnityEngine.UI;
using UmbraProjects.Managers;
using UnityEngine.Serialization;
using Random = System.Random;

namespace UmbraProjects.AutoChess {
    // Inherited manager class to handle all overhead game states, mechanics, object generations, etc. 
    public class GameManager : Manager<GameManager> {
        // Variables
        private List<Agent> _myAgents = new List<Agent>();
        private List<Agent> _enemyAgents = new List<Agent>();
        
        // Constants
        private readonly int[] _agentPoolSize = {24, 18, 15, 10, 9};        // # of Agents/Agent Cost
        private readonly int[] _gameStateTimerLengths = {10, 15, 30, 3};    // Game state timer lengths (corresponds to GameState enum)
        
        // Flags
        public bool IsGameActive;
        public bool IsTimerActive;
        public bool IsFightActive;

        // Assignables
        public Camera GameCamera;
        public AgentDatabaseSO AgentDatabase;
        public List<AgentDatabaseSO.AgentData> AgentPool;
        public UIShop AgentShop;

        public SpawnManager Spawn;

        [Header("UI References")]
        public Slider RoundSlider;
        public TextMeshProUGUI RoundTimerText;
        public TextMeshProUGUI RoundNumberText;
        public TextMeshProUGUI AllowedAgentsText;

        // Variables
        public int[] GameStateBuffer; // Linear buffer of game states
        public int GameStateIndex;
        public int LastGameState;

        // Awake is called when the script instance is being loaded
        protected new void Awake() {
            base.Awake();

            GameCamera = Camera.main;
            GenerateAgentPool();
        }

        // Start is called before the first frame update
        private void Start() {
            GameStateBuffer = new int[5]; // Initialize size of game state linear buffer
            
            IsGameActive = true; // Initialize game start

            if (IsGameActive) {
                // Initialize game state
                SetGameState((int) GameState.Prep); // Switch to GameState.Carousel on game finish
                StartCoroutine(GameStateTimer(_gameStateTimerLengths[GetGameState()]));
            }
        }

        // Called every fixed frame-rate frame
        private void FixedUpdate() {
            if (IsTimerActive) {
                return;
            }
            
            GameStateIndex = SetGameStateIndex(GameStateIndex);

            switch (LastGameState) {
                case (int) GameState.Carousel:
                    SetGameState((int) GameState.Prep);
                    AllowedAgentsText.gameObject.SetActive(true);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;

                case (int) GameState.Prep:
                    SetGameState((int) GameState.Fight);
                    AllowedAgentsText.gameObject.SetActive(false);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", true);

                    break;

                case (int) GameState.Fight:
                    SetGameState((int) GameState.Buffer);
                    AllowedAgentsText.gameObject.SetActive(false);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;

                case (int) GameState.Buffer:
                    SetGameState((int) GameState.Prep);
                    AllowedAgentsText.gameObject.SetActive(true);
                    PlayerManager.Instance.Player.GetComponent<Animator>().SetBool("IsGameStateFight", false);
                    break;
            }

            StartCoroutine(GameStateTimer(_gameStateTimerLengths[GetGameState()]));
        }

        private void OnEnable() {
            EventManager.OnAgentDeath += AgentDead;
        }

        private void OnDisable() {
            EventManager.OnAgentDeath -= AgentDead;
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

        // Generation of Agent cards database 
        private void GenerateAgentPool() {
            AgentPool = new List<AgentDatabaseSO.AgentData>();

            for (var i = 0; i < AgentDatabase.AllAgents.Count; i++) {
                var cost = AgentDatabase.AllAgents[i].AgentCost;

                // Populate # of Agent cards based on Agent cost
                for (var j = 0; j < _agentPoolSize[cost - 1]; j++) {
                    AgentPool.Add(AgentDatabase.AllAgents[i]);
                }
            }
        }
        
        // Adds agent to player's team
        // NOTE: Need to input variable of which team to add agent to for multiplayer
        public void AddAgent(AgentDatabaseSO.AgentData agent) {
            var newAgent = Instantiate(agent.AgentPrefab); // Need bench position and rotation
            newAgent.gameObject.name = agent.AgentName;
            _myAgents.Add(newAgent);

            newAgent.Setup(Team.Team1, GridManager.Instance.GetFreeNode(Team.Team1, GridManager.Instance.BenchGraph));
        }
        
        // Set the game's current game state
        private void SetGameState(int gameState) {
            GameStateBuffer[GameStateIndex] = gameState;
        }
        
        // Obtain current game state
        private int GetGameState() {
            return GameStateBuffer[GameStateIndex];
        }
        
        // Update the game state's index position
        private int SetGameStateIndex(int index) {
            if (index >= GameStateBuffer.Length - 1) // Reset buffer index position
                index = 0;
            else index++;

            return index;
        }
        
        // Returns all agents for corresponding team
        public List<Agent> GetAgents(Team team) {
            return team != Team.Team1 ? _enemyAgents : _myAgents;
        }
        
        // REMOVE AGENT FROM FIELD ON DEATH | WORK ON MORE
        public void AgentDead(Agent agent) {
            _myAgents.Remove(agent); //???
            _enemyAgents.Remove(agent); //???

            agent.gameObject.SetActive(false);
        }
        
        // Debug method for generating enemies to fight
        public void DebugBuild() {
            for (var i = 0; i < PlayerManager.Instance.Player.GetComponent<Player>().Level.Value; i++) {
                // int randomIndex = UnityEngine.Random.Range(0, AgentDatabase.AllAgents.Count);
                // Agent agent = Instantiate(AgentDatabase.AllAgents[randomIndex].AgentPrefab);

                var agent = Instantiate(AgentDatabase.AllAgents[0].AgentPrefab);
                agent.gameObject.name = AgentDatabase.AllAgents[0].AgentName;
                _enemyAgents.Add(agent);

                // Create generic code to place enemy agents on battlefield
                agent.Setup(Team.Team2, GridManager.Instance.GetFreeNode(Team.Team2, GridManager.Instance.Graph));
            }
        }
        
        // Debug method to fight enemies
        public void DebugFight() {
            // Add code for fighting here...

            IsFightActive = !IsFightActive;
        }
    }
}
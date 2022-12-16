using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UmbraProjects.AutoChess;
using UmbraProjects.AutoChess.Agents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UmbraProjects.AutoChess.UI {
    // UI class for all Agent Shop interactions
    public class UIShop : MonoBehaviour {
        // Assignables
        public List<UICard> CardsInShop;

        public TextMeshProUGUI PlayerLevel;
        public TextMeshProUGUI PlayerXpAmount;
        public TextMeshProUGUI PlayerCoins;
        public TextMeshProUGUI PlayerWinStreak;
        public TextMeshProUGUI RefreshCost;
        public TextMeshProUGUI ExperienceCost;

        public TextMeshProUGUI SellAgentText;

        public Sprite LockSprite;
        public Sprite UnlockSprite;

        public TextMeshProUGUI[] AgentRollChance = new TextMeshProUGUI[5];

        // Variables
        private List<AgentDatabaseSO.AgentData> _cachedAgentPool;
        private List<AgentDatabaseSO.AgentData> _cachedAgentShop;

        private GameObject _lockPanel;
        private GameObject _xpPanel;

        private bool _shopLocked;

        private readonly int _shopCostRefresh = 2;
        private readonly int _shopCostXp = 4;

        private readonly int[,] _shopLevelPercentages = {
            {100, 0, 0, 0, 0}, // Level 1
            {100, 0, 0, 0, 0}, // Level 2
            {75, 25, 0, 0, 0}, // Level 3
            {55, 30, 15, 0, 0}, // Level 4
            {45, 33, 20, 2, 0}, // Level 5
            {25, 40, 30, 5, 0}, // Level 6
            {19, 30, 35, 15, 1}, // Level 7
            {16, 20, 35, 25, 4}, // Level 8
            {9, 15, 30, 30, 16}, // Level 9 
            {5, 10, 20, 40, 25}, // Level 10
            {1, 2, 12, 50, 35}, // Level 11
        };

        private Player _player;

        // Start is called before the first frame update
        private void Start() {
            _lockPanel = GameObject.Find("Lock/Unlock Icon");
            _xpPanel = GameObject.Find("Buy Experience Panel");
            _player = PlayerManager.Instance.Player.GetComponent<Player>();

            PlayerLevel.text = $"Level {_player.Level.Value}";
            PlayerXpAmount.text = null;
            PlayerCoins.text = _player.Coins.Value.ToString();
            RefreshCost.text = _shopCostRefresh.ToString();
            ExperienceCost.text = _shopCostXp.ToString();

            GenerateShop();
        }

        private void OnEnable() {
            EventManager.OnLevelUp += UpdatePlayerLevel;
            EventManager.OnLevelUp += UpdateRollChance;
            
            
        }

        private void OnDisable() {
            EventManager.OnLevelUp -= UpdatePlayerLevel;
            EventManager.OnLevelUp -= UpdateRollChance;
            
            
        }
        
        // UI interaction for OnEnter event of cards in shop
        public void OnEnter() {
            if (PlayerManager.Instance.SelectedAgent != null) {
                GameObject selectedAgent = PlayerManager.Instance.SelectedAgent;

                foreach (var card in CardsInShop)
                    card.transform.parent.gameObject.SetActive(false);

                SellAgentText.gameObject.SetActive(true);
                SellAgentText.text = $"Sell {selectedAgent.name} for {selectedAgent.GetComponent<Agent>().Cost} coins";
            }
        }

        // UI interaction for OnExit event of cards in shop
        public void OnExit() {
            if (PlayerManager.Instance.SelectedAgent != null) {
                SellAgentText.gameObject.SetActive(false);

                foreach (var card in CardsInShop)
                    card.transform.parent.gameObject.SetActive(true);
            }
        }

        // WRITE CODE FOR SELLING AGENTS [CONTINUE HERE]
        // UI interaction for OnRelease event of cards in shop
        public void OnRelease() {
            SellAgentText.gameObject.SetActive(false);

            foreach (var card in CardsInShop)
                card.transform.parent.gameObject.SetActive(true);

            Debug.Log($"Agent sold!");

            /*if (PlayerManager.Instance.SelectedAgent != null) {
                Debug.Log($"Agent sold!");
            }*/
        }

        // Generation of cards to appear in shop for user based off user level and tier percentages
        private List<UICard> GenerateShop() {
            _cachedAgentPool = GameManager.Instance.AgentPool;
            _cachedAgentShop = new List<AgentDatabaseSO.AgentData>();

            foreach (var card in CardsInShop) {
                if (!card.gameObject.activeSelf) {
                    card.gameObject.SetActive(true);
                }

                var cost = ShopCostTier((int) _player.Level.Value, Random.value);
                var tempAgents = _cachedAgentPool.Where(agent => agent.AgentCost == cost).ToList();

                var cachedAgent = tempAgents[Random.Range(0, tempAgents.Count)];
                _cachedAgentShop.Add(cachedAgent); // Add to cache of shop for pool removal

                card.Setup(cachedAgent, this); // Setup UI card
            }

            // Remove current agents in shop from pool
            RemoveAgentsFromPool(_cachedAgentShop, GameManager.Instance.AgentPool);

            return CardsInShop;
        }
        
        // Returns the cost-tier to use for random selection of Agent for the shop
        // Level = Player's level for percent chance of each cost tier
        // Value = Random value between 0 and 1
        private int ShopCostTier(int level, float value) {
            var agentCost = 0;

            // Refer to _shopLevelPercentages for cost-tier roll chances
            switch (level) {
                case 1:
                    agentCost = 1;
                    break;

                case 2:
                    agentCost = 1;
                    break;

                case 3:
                    agentCost = value <= 0.75f ? 1 : 2;
                    break;

                case 4:
                    if (value <= 0.55f)
                        agentCost = 1;
                    else if (value > 0.55f && value <= 0.85f)
                        agentCost = 2;
                    else
                        agentCost = 3;

                    break;

                case 5:
                    if (value <= 0.45f)
                        agentCost = 1;
                    else if (value > 0.45f && value <= 0.78f)
                        agentCost = 2;
                    else if (value > 0.78f && value <= 0.98f)
                        agentCost = 3;
                    else
                        agentCost = 4;

                    break;

                case 6:
                    if (value <= 0.25f)
                        agentCost = 1;
                    else if (value > 0.25f && value <= 0.65f)
                        agentCost = 2;
                    else if (value > 0.65f && value <= 0.95f)
                        agentCost = 3;
                    else
                        agentCost = 4;

                    break;

                case 7:
                    if (value <= 0.19f)
                        agentCost = 1;
                    else if (value > 0.19f && value <= 0.49f)
                        agentCost = 2;
                    else if (value > 0.49f && value <= 0.84f)
                        agentCost = 3;
                    else if (value > 0.84f && value <= 0.99f)
                        agentCost = 4;
                    else
                        agentCost = 5;

                    break;

                case 8:
                    if (value <= 0.16f)
                        agentCost = 1;
                    else if (value > 0.16f && value <= 0.36f)
                        agentCost = 2;
                    else if (value > 0.36f && value <= 0.71f)
                        agentCost = 3;
                    else if (value > 0.71f && value <= 0.96f)
                        agentCost = 4;
                    else
                        agentCost = 5;

                    break;
                case 9:
                    if (value <= 0.09f)
                        agentCost = 1;
                    else if (value > 0.09f && value <= 0.24f)
                        agentCost = 2;
                    else if (value > 0.24f && value <= 0.54f)
                        agentCost = 3;
                    else if (value > 0.54f && value <= 0.84f)
                        agentCost = 4;
                    else
                        agentCost = 5;

                    break;

                case 10:
                    if (value <= 0.05f)
                        agentCost = 1;
                    else if (value > 0.05f && value <= 0.15f)
                        agentCost = 2;
                    else if (value > 0.15f && value <= 0.35f)
                        agentCost = 3;
                    else if (value > 0.35f && value <= 0.75f)
                        agentCost = 4;
                    else
                        agentCost = 5;

                    break;

                case 11:
                    if (value <= 0.01f)
                        agentCost = 1;
                    else if (value > 0.01f && value <= 0.03f)
                        agentCost = 2;
                    else if (value > 0.03f && value <= 0.15f)
                        agentCost = 3;
                    else if (value > 0.15f && value <= 0.65f)
                        agentCost = 4;
                    else
                        agentCost = 5;

                    break;
            }

            return agentCost;
        }

        // Removes randomly selected Agents for shop from the collective pool
        private void RemoveAgentsFromPool(List<AgentDatabaseSO.AgentData> cachedAgentShop, List<AgentDatabaseSO.AgentData> agentPool) {
            for (var i = 0; i < cachedAgentShop.Count; i++)
                agentPool.Remove((agentPool.Where(agent => agent.AgentName == cachedAgentShop[i].AgentName)).First());
        }

        // On shop refresh or round ending, return active Agents in shop to collective pool
        private void ReturnAgentsToPool(List<AgentDatabaseSO.AgentData> cachedAgentShop, List<AgentDatabaseSO.AgentData> agentPool) {
            if (cachedAgentShop != null) {
                agentPool.AddRange(cachedAgentShop);
                agentPool = agentPool.OrderBy(agent => agent.AgentNumber).ToList();
            }
        }

        // Refresh shop
        private void RefreshShop() {
            if (_shopLocked) {
                UnlockShop();
            }
            
            CacheSelectedAgents(CardsInShop);
            ReturnAgentsToPool(_cachedAgentShop, GameManager.Instance.AgentPool);
            GenerateShop(); // Generate new shop
        }
        
        // Temporarily store Agents pulled from the Agent Pool to offer to Player
        private void CacheSelectedAgents(List<UICard> cards) {
            var tempAgents = new List<AgentDatabaseSO.AgentData>();

            for (var i = 0; i < cards.Count; ++i) {
                if (cards[i].isActiveAndEnabled) {
                    tempAgents.Add(_cachedAgentShop[i]);
                }
            }

            _cachedAgentShop = tempAgents;
        }
        
        // On card click, add Agent to team if Player can afford
        public void OnCardClick(UICard card, AgentDatabaseSO.AgentData agent) {
            if (!_player.CanPlayerAfford(agent.AgentCost)) {
                return;
            }

            if (!GridManager.Instance.IsNodeFree(GridManager.Instance.BenchGraph)) {
                return;
            }
            
            EventManager.OnCardClick?.Invoke(agent.AgentCost);
            card.gameObject.SetActive(false);
            GameManager.Instance.AddAgent(agent);
        }
        
        // On refresh click, return Agents to pool and generate a new Shop
        public void OnRefreshClick() {
            if (!_player.CanPlayerAfford(_shopCostRefresh)) {
                return;
            }
            
            EventManager.OnRefreshClick?.Invoke(_shopCostRefresh);
            RefreshShop();
        }

        // On XP click, invoke the relative action
        public void OnExperienceClick() {
            if (!_player.CanPlayerAfford(_shopCostXp)) {
                return;
            }
            
            EventManager.OnExperienceClick?.Invoke(_shopCostXp);
        }
        
        // On lock click, change the state of the shop 
        public void OnLockClick() {
            if (!_shopLocked) {
                LockShop();
            }
            else {
                UnlockShop();
            }
        }

        // Unlock shop to allow refresh
        private void UnlockShop() {
            _shopLocked = false;
            _lockPanel.GetComponent<Image>().sprite = UnlockSprite;
        }

        // Lock shop to disallow refresh
        private void LockShop() {
            _shopLocked = true;
            _lockPanel.GetComponent<Image>().sprite = LockSprite;
        }

        // Update percentages of Agent roll odds for shop UI
        public void UpdateRollChance(int level) {
            for (var i = 0; i < AgentRollChance.Length; ++i)
                AgentRollChance[i].text = $"{_shopLevelPercentages[level - 1, i]}%";
        }

        // Update player level for UI
        public void UpdatePlayerLevel(int level) {
            PlayerLevel.text = $"Level {level}";

            if (level == (int) _player.Level.MaxValue) {
                UpdatePlayerStatsAtMaxLevel();
            }
        }

        // Update player stats at max level for UI
        public void UpdatePlayerStatsAtMaxLevel() {
            Button xpButton = _xpPanel.GetComponent<Button>();

            PlayerXpAmount.text = $"Max level";
            xpButton.interactable = false;
        }
    }
}
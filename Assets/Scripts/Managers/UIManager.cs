using System.Collections.Generic;
using TMPro;
using UmbraProjects.AutoChess.UI;
using UmbraProjects.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UmbraProjects.AutoChess
{
    public class UIManager : Manager<UIManager> {
        // Assignables
        public GameObject Player;

        [Header("UI References")]
        public Slider RoundSlider;
        public TextMeshProUGUI RoundTimerText;
        public TextMeshProUGUI RoundNumberText;
        public TextMeshProUGUI PlayerTeamSizeText;
        public TextMeshProUGUI PlayerCoinsText;
        public TextMeshProUGUI PlayerLevelText;
        public TextMeshProUGUI[] PlayerRollChance = new TextMeshProUGUI[5];
        public Button PlayerXpButton;
        public TextMeshProUGUI PlayerXpText;
        
        
        // Variables
        private Player _player;
        
        // Constants
        private readonly int[,] _shopRollPercentages = {
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

        // Awake is called when the script instance is being loaded
        protected new void Awake() {
            base.Awake();

            _player = Player.GetComponent<Player>();
        }

        // Start is called before the first frame update
        private void Start() {
            PlayerTeamSizeText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";

            PlayerCoinsText.text = $"{_player.Coins.Value}";
            PlayerLevelText.text = $"Level {_player.Level.Value}";
            PlayerXpText.text = $"{_player.Xp.Value}/{PlayerManager.Instance.XpPerLevel[(int) _player.Level.Value]}";

            for (var i = 0; i < PlayerRollChance.Length; ++i) {
                PlayerRollChance[i].text = $"{_shopRollPercentages[(int) (_player.Level.Value - 1), i]}%";
            }
        }

        private void OnEnable() {
            EventManager.OnGainXp += UpdatePlayerXpText;
            
            EventManager.OnLevelUp += UpdateAllowedAgentsText;
            EventManager.OnLevelUp += UpdatePlayerLevelText;
            EventManager.OnLevelUp += UpdatePlayerRollChance;
            EventManager.OnLevelUp += UpdatePlayerXpText;
            
            EventManager.OnSpendCoins += UpdatePlayerCoinsText;
        }

        private void OnDisable() {
            EventManager.OnGainXp -= UpdatePlayerXpText;
            
            EventManager.OnLevelUp -= UpdateAllowedAgentsText;
            EventManager.OnLevelUp -= UpdatePlayerLevelText;
            EventManager.OnLevelUp -= UpdatePlayerRollChance;
            EventManager.OnLevelUp -= UpdatePlayerXpText;
            
            EventManager.OnSpendCoins -= UpdatePlayerCoinsText;
            
        }

        private void UpdateAllowedAgentsText(int value) {
            PlayerTeamSizeText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
        }

        private void UpdatePlayerCoinsText(int value) {
            PlayerCoinsText.text = $"{_player.Coins.Value}";
        }

        private void UpdatePlayerLevelText(int value) {
            PlayerLevelText.text = $"Level {_player.Level.Value}";

            if ((int) _player.Level.Value == (int) _player.Level.MaxValue) {
                UpdatePlayerStatsAtMaxLevel();
            }
        }

        private void UpdatePlayerRollChance(int value) {
            for (var i = 0; i < PlayerRollChance.Length; ++i) {
                PlayerRollChance[i].text = $"{_shopRollPercentages[(int) (_player.Level.Value - 1), i]}%";
            }
        }

        private void UpdatePlayerStatsAtMaxLevel() {
            PlayerXpText.text = "Max level";
            PlayerXpButton.interactable = false;
        }

        private void UpdatePlayerXpText(int value) {
            if (_player.Level.Value >= _player.Level.MaxValue) {
                return;
            }
            
            PlayerXpText.text = $"{_player.Xp.Value}/{PlayerManager.Instance.XpPerLevel[(int) _player.Level.Value]}";
        }
    }
}
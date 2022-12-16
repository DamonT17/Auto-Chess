using TMPro;
using UmbraProjects.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UmbraProjects.AutoChess
{
    public class UIManager : Manager<UIManager> {
        // Variables
        private Player _player;
        
        // Assignables
        public GameObject Player;
        
        [Header("UI References")]
        public Slider RoundSlider;
        public TextMeshProUGUI RoundTimerText;
        public TextMeshProUGUI RoundNumberText;
        public TextMeshProUGUI AllowedAgentsText;
        public TextMeshProUGUI CoinsText;
        public TextMeshProUGUI XpAmountText;

        // Awake is called when the script instance is being loaded
        protected new void Awake() {
            base.Awake();

            _player = Player.GetComponent<Player>();
        }

        // Start is called before the first frame update
        private void Start() {
            AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
            XpAmountText.text = $"{_player.Xp.Value}/{PlayerManager.Instance.XpPerLevel[(int) _player.Level.Value]}";
        }

        private void OnEnable() {
            EventManager.OnGainXp += UpdateXpText;
            
            EventManager.OnLevelUp += UpdateAllowedAgentsText;
            EventManager.OnLevelUp += UpdateXpText;
            
            EventManager.OnSpendCoins += UpdateCoinsText;
        }

        private void OnDisable() {
            EventManager.OnGainXp -= UpdateXpText;
            
            EventManager.OnLevelUp -= UpdateAllowedAgentsText;
            EventManager.OnLevelUp -= UpdateXpText;
            
            EventManager.OnSpendCoins -= UpdateCoinsText;
            
        }

        private void UpdateAllowedAgentsText(int value) {
            AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
        }

        private void UpdateCoinsText(int value) {
            CoinsText.text = $"{_player.Coins.Value}";
        }

        private void UpdateXpText(int value) {
            XpAmountText.text = $"{_player.Xp.Value}/{PlayerManager.Instance.XpPerLevel[(int) _player.Level.Value]}";
        }
    }
}
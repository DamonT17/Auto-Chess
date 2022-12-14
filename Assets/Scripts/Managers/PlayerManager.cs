using TMPro;
using UnityEngine;
using UmbraProjects.Managers;

namespace UmbraProjects.AutoChess {
// Inherited manager class to handle all overhead player stats, mechanics, object generations, etc.
    public class PlayerManager : Manager<PlayerManager> {
        // Variables
        private Player _player;
        
        // Constants
        private readonly int[] _xpPerLevel = new int[11] {
            0, 2, 2, 6, 10, 20, 36, 56, 80, 100, 120
        }; // XP needed for player to level up
        
        // Assignables
        public GameObject Player;
        public GameObject SelectedAgent;
        
        // Events
        public System.Action OnUpdate;
        
        // Awake is called when the script instance is being loaded
        protected new void Awake() {
            base.Awake();

            _player = Player.GetComponent<Player>();
        }

        // Start is called before the first frame update
        private void Start() {
            GameManager.Instance.AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
        }

        // Check if Player can afford cost of Agent or XP
        public bool CanAfford(int amount) {
            return amount <= _player.Coins.Value;
        }

        // Check if Player is at max level or can level up
        public bool CanPlayerLevelUp() {
            return _player.Level.Value < _player.Level.MaxValue;
        }

        // Spend an amount of coins for Agent or XP
        public void SpendCoins(int amount) {
            _player.Coins.Value -= amount;
            OnUpdate?.Invoke();
        }

        // Increment Player's XP amount
        public void GainXP(int amount) {
            _player.Xp.Value += amount;

            if (CanPlayerLevelUp()) {
                if (_player.Xp.Value >= _xpPerLevel[(int) _player.Level.Value]) {
                    LevelUp();
                    return;
                }

                GameManager.Instance.AgentShop.UpdatePlayerXP((int) _player.Xp.Value, 
                    _xpPerLevel[(int) _player.Level.Value]);
            }
        }

        // Level up the Player (Move to Player class???)
        public void LevelUp() {
            if (!CanPlayerLevelUp()) {
                return;
            }

            Player.GetComponent<Animator>().SetTrigger("CanPlayerLevelUp");

            _player.Xp.Value -= _xpPerLevel[(int) _player.Level.Value]; // Reset XP to modulus for next level
            _player.Level.Value++; // Increase level
            _player.TeamSize.MaxValue = _player.Level.Value;

            // Check if player is at max level
            if ((int) _player.Level.Value == (int) _player.Level.MaxValue) {
                Player.GetComponentInChildren<TextMeshProUGUI>().text = $"{_player.Level.Value}";
                GameManager.Instance.AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";

                GameManager.Instance.AgentShop.UpdatePlayerLevel((int) _player.Level.Value);
                GameManager.Instance.AgentShop.UpdatePlayerStatsAtMaxLevel();
                return;
            }

            GainXP(0); // Recursively call LevelUp() through GainXP()

            Player.GetComponentInChildren<TextMeshProUGUI>().text = $"{_player.Level.Value}";

            GameManager.Instance.AgentShop.UpdatePlayerLevel((int) _player.Level.Value);
            GameManager.Instance.AgentShop.UpdateRollChance((int) _player.Level.Value); // Update Agent roll percentages for shop

            GameManager.Instance.AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
        }
    }
}
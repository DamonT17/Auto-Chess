using UnityEngine;
using UmbraProjects.Managers;

namespace UmbraProjects.AutoChess {
    // Inherited manager class to handle all overhead player stats, mechanics, object generations, etc.
    public class PlayerManager : Manager<PlayerManager> {
        // Variables
        private Player _player;
        
        // Constants
        public readonly int[] XpPerLevel = new int[11] {
            0, 2, 2, 6, 10, 20, 36, 56, 80, 100, 120
        }; // XP needed for player to level up
        
        // Assignables
        public GameObject Player;
        public GameObject SelectedAgent;
        
        // Awake is called when the script instance is being loaded
        protected new void Awake() {
            base.Awake();

            _player = Player.GetComponent<Player>();
        }

        // Start is called before the first frame update
        private void Start() {
            //GameManager.Instance.AllowedAgentsText.text = $"{_player.TeamSize.Value}/{_player.TeamSize.MaxValue}";
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// INHERITANCE
// Inherited manager class to handle all overhead player stats, mechanics, object generations, etc.
public class PlayerManager : Manager<PlayerManager> {
    // Assignables
    public GameObject Player;
    public Camera Camera;

    // ENCAPSULATION
    // Properties
    public int PlayerHealth { get; private set; }
    public int PlayerLevel { get; private set; }
    public int PlayerCoins { get; private set; }
    public int PlayerXP { get; private set; }

    public System.Action OnUpdate;

    // Variables
    public GameObject SelectedAgent;

    private Slider _playerSlider;
    private int _xpPerRound = 2;                            // Experience gained per round
    private readonly int[] _xpPerLevel = new int[11] {
        0, 2, 2, 6, 10, 20, 36, 56, 80, 100, 120
    };    // Experience points needed to level up for each level

    private int _maxLevel = 11;                     // Max player level
    private int _maxHealth = 100;                   // Max player health

    // Awake is called when the script instance is being loaded
    protected new void Awake() {
        base.Awake();
        _playerSlider = Player.GetComponentInChildren<Slider>();
        
        PlayerHealth = 100;
        PlayerLevel = 1;
        PlayerCoins = 500;
        PlayerXP = 0;

        _playerSlider.maxValue = _maxHealth;
        _playerSlider.value = PlayerHealth;
        _playerSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerLevel}";
    }

    // Start is called before the first frame update
    private void Start() {
        GameManager.Instance.TeamSize = PlayerLevel;
    }

    // Called every fixed frame-rate frame
    private void FixedUpdate() {

    }

    // Obtains world coordinates of mouse position
    public Vector3 GetMousePosition() {
        // Get world coordinates of mouse position
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        var worldPos = new Vector3(0, 0, 0);

        if (Physics.Raycast(ray, out hit))
            worldPos = hit.point;

        return worldPos;
    }

    // Check if Player can afford cost of Agent or XP
    public bool CanAfford(int amount) {
        return amount <= PlayerCoins;
    }
    
    // Check if Player is at max level or can level up
    public bool CanPlayerLevelUp() {
        return PlayerLevel < _maxLevel;
    }

    // Spend an amount of coins for Agent or XP
    public void SpendCoins(int amount) {
        PlayerCoins -= amount;
        OnUpdate?.Invoke();
    }

    // Increment Player's XP amount
    public void GainXP(int amount) {
        PlayerXP += amount;

        if (CanPlayerLevelUp()) {
            if (PlayerXP >= _xpPerLevel[PlayerLevel]) {
                LevelUp();
                return;
            }

            GameManager.Instance.AgentShop.UpdatePlayerXP(PlayerXP, _xpPerLevel[PlayerLevel]);
        }
    }

    // Level up the Player
    public void LevelUp() {
        if (CanPlayerLevelUp()) {
            Player.GetComponent<Animator>().SetTrigger("CanPlayerLevelUp");

            PlayerXP -= _xpPerLevel[PlayerLevel];   // Reset XP to modulus for next level
            PlayerLevel++;                          // Increase level
            GameManager.Instance.TeamSize = PlayerLevel;

            // Check if player is at max level
            if (PlayerLevel == _maxLevel) {
                _playerSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerLevel}";
                GameManager.Instance.AllowedAgentsText.text =
                    $"{GameManager.Instance.CurrentTeamSize}/{GameManager.Instance.TeamSize}";

                GameManager.Instance.AgentShop.UpdatePlayerLevel(PlayerLevel);
                GameManager.Instance.AgentShop.UpdatePlayerStatsAtMaxLevel();
                return;
            }

            GainXP(0);  // Recursively call LevelUp() through GainXP()

            _playerSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerLevel}";

            GameManager.Instance.AgentShop.UpdatePlayerLevel(PlayerLevel);
            GameManager.Instance.AgentShop.UpdateRollChance(PlayerLevel); // Update Agent roll percentages for shop

            GameManager.Instance.AllowedAgentsText.text =
                $"{GameManager.Instance.CurrentTeamSize}/{GameManager.Instance.TeamSize}";
        }
    }
}

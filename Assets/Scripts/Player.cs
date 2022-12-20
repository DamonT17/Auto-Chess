using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UmbraProjects.Utilities;
using UmbraProjects.Utilities.Interfaces;
using UnityEngine.Serialization;
using Attribute = UmbraProjects.Utilities.Attribute;

namespace UmbraProjects.AutoChess
{
    // Base class for Player control
    public class Player : MonoBehaviour, IDamageable<int>, IKillable, ILeveler {
        private Animator _playerAnimator;
        private NavMeshAgent _navMeshAgent;
        private Rigidbody _playerRb;
        private Slider _playerSlider;
        private TextMeshProUGUI _playerText;
        
        private Vector3 _playerPosition;
        private Vector3 _targetPosition;
        
        // Flags
        private bool _movePlayer = false;

        // Constants
        private const int _LEFT_CLICK = 0;
        private const int _RIGHT_CLICK = 1;

        [Header("Player Attributes")]
        public Attribute Health;
        public Attribute Level;
        public Attribute Coins;
        public Attribute ShopCost;
        public Attribute Xp;
        public Attribute XpCost;
        public Attribute XpPerRound;
        public Attribute TeamNumber;
        public Attribute TeamSize;
        public Attribute MoveSpeed;

        // Awake is called when the script instance is being loaded
        private void Awake() {
            _playerAnimator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _playerRb = GetComponent<Rigidbody>();
            _playerSlider = GetComponentInChildren<Slider>();
            _playerText = GetComponentInChildren<TextMeshProUGUI>();
            
            SetAttributes();
        }
        
        // Start is called before the first frame update
        private void Start() {
            _playerSlider.maxValue = Health.MaxValue;
            _playerSlider.value = Health.BaseValue;
            _playerSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"{Level.BaseValue}";
        }

        // Update is called once per frame
        private void Update() {
            _playerPosition = transform.position;
            
            if (Input.GetMouseButtonDown(_LEFT_CLICK)) {
                // (left-click input)
                // Instantiate(GameManager.Instance.Spawn.orbPrefab[0], GameManager.Instance.Spawn.GenerateCircularSpawnPosition(),
                // GameManager.Instance.Spawn.orbPrefab[0].transform.rotation);
            }
            else if (Input.GetMouseButtonDown(_RIGHT_CLICK)) {
                // Get target position player is to move to (right-click input)
                _targetPosition = Utility.GetMousePositionInWorldCoordinates(GameManager.Instance.GameCamera);
                _movePlayer = true;
            }

            if (_movePlayer) {
                MovePlayer(_targetPosition);
            }
        }

        private void OnEnable() {
            EventManager.OnCardClick += SpendCoins;
            EventManager.OnExperienceClick += GainXp;
            EventManager.OnRefreshClick += SpendCoins;
        }

        private void OnDisable() {
            EventManager.OnCardClick -= SpendCoins;
            EventManager.OnExperienceClick -= GainXp;
            EventManager.OnRefreshClick -= SpendCoins;
        }
        
        // Set Player's attributes on instantiation
        private void SetAttributes() {
            Health.BaseValue = 100;
            Health.MaxValue = 100;
            
            Level.BaseValue = 1;
            Level.MaxValue = 11;

            Coins.BaseValue = 500;  // Change to 0 on finalization of project!

            ShopCost.BaseValue = 2;
            
            Xp.BaseValue = 0;

            XpCost.BaseValue = 4;

            XpPerRound.BaseValue = 2;

            TeamNumber.BaseValue = (float) Team.Team1;
            
            TeamSize.BaseValue = 0;
            TeamSize.MaxValue = 1;
            
            MoveSpeed.BaseValue = 2.0f;
        }

        // Check if Player can afford cost of Agent or XP
        public bool CanPlayerAfford(int amount) {
            return amount <= Coins.Value;
        }

        // Check if Player can level up
        private bool CanPlayerLevelUp() {
            return Level.Value < Level.MaxValue && Xp.Value >= PlayerManager.Instance.XpPerLevel[(int) Level.Value];
        }

        // Spend an amount of coins for Agent or XP
        public void SpendCoins(int amount) {
            Coins.Value -= amount;
            EventManager.OnSpendCoins?.Invoke(amount);
        }

        // Increment the Player's XP attribute
        private void GainXp(int amount) {
            SpendCoins(amount);
            
            Xp.Value += XpCost.Value;
            EventManager.OnGainXp?.Invoke(amount);
            
            if (!CanPlayerLevelUp()) {
                return;
            }
            
            LevelUp();
        }

        // Increase the Player's Level attribute
        public void LevelUp() {
            while ((int) Level.Value != (int) Level.MaxValue && Xp.Value >= PlayerManager.Instance.XpPerLevel[(int) Level.Value]) {
                Xp.Value -= PlayerManager.Instance.XpPerLevel[(int) Level.Value];
                Level.Value++;
                TeamSize.MaxValue++;
            }
            
            _playerAnimator.SetTrigger("CanPlayerLevelUp");
            _playerText.text = $"{Level.Value}";
            
            EventManager.OnLevelUp?.Invoke((int) Level.Value);
        }

        public void ApplyDamage(int damageAmount) {
            // Add code for damage of player
        }

        public void ApplyDamage(int damageAmount, int damageType, bool isCriticalDamage) {
            throw new NotImplementedException();
        }

        public void Death() {
            // Add code for killing of player
        }

        // Moves player to target destination
        private void MovePlayer(Vector3 target) {
            const float tolerance = 0.0001f;

            _navMeshAgent.destination = target;
            _playerAnimator.SetFloat("Speed", _navMeshAgent.velocity.magnitude);

            if (Math.Abs(_playerPosition.x - target.x) < tolerance &&
                Math.Abs(_playerPosition.z - target.z) < tolerance)
                _movePlayer = false;
        }

        // Player collider events
        private void OnTriggerEnter(Collider other) {
            var colliderTag = Utility.GetColliderTag(other);

            switch (colliderTag) {
                case "Coin": // Coins increase player's money count

                    Destroy(other.gameObject);
                    break;

                case "Item": // Items buff player's Agents

                    Destroy(other.gameObject);
                    break;

                case "Orb": // Orbs drop either coin(s) or item(s) based off random spawn generator

                    Destroy(other.gameObject);
                    break;
            }

            if (other.CompareTag("Coin") || other.CompareTag("Item") || other.CompareTag("Orb")) {
                Destroy(other.gameObject);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace UmbraProjects.AutoChess.Agents {
    public class AgentStatusBar : MonoBehaviour {
        // Assignables
        [SerializeField] private Image[] _statusImages;
        private Camera _camera;
        private RectTransform _statusRectTransform;
        private Agent _thisAgent;

        // Constants
        private const float _DAMAGE_EFFECT_SPEED = 0.5f;
        private const float _DAMAGE_EFFECT_TIMER_MAX = 0.25f;
        
        // Variables
        private float _damageEffectTimer = 0f;

        // Awake is called when the script instance is being loaded
        private void Awake() {
            _camera = Camera.main;
            
            _statusRectTransform = GetComponent<RectTransform>();
            _thisAgent = GetComponentInParent<Agent>();
        }

        // Start is called before the first frame update
        private void Start() {
            SetImage((int) AgentStatusType.Shield, 0f);
            SetImage((int) AgentStatusType.Damage, 1f);
            SetImage((int) AgentStatusType.Health, 1f);
            SetImage((int) AgentStatusType.Mana, (float) _thisAgent.Mana.Value / _thisAgent.Mana.MaxValue);

            _statusRectTransform.rotation = _camera.transform.rotation;
        }

        // Update is called once per frame
        private void Update() {
            LockRotation();

            _damageEffectTimer -= Time.deltaTime;

            if (_damageEffectTimer < 0) {
                if (_statusImages[(int) AgentStatusType.Health].fillAmount <
                    _statusImages[(int) AgentStatusType.Damage].fillAmount) {
                    _statusImages[(int) AgentStatusType.Damage].fillAmount -= _DAMAGE_EFFECT_SPEED * Time.deltaTime;
                }
            }
        }

        public void SetImage(int index, float value) {
            _statusImages[index].fillAmount = value;
        }

        public void StartDamageEffect() {
            _damageEffectTimer = _DAMAGE_EFFECT_TIMER_MAX;
        }

        private void LockRotation() {
            if (_statusRectTransform.rotation == _camera.transform.rotation) {
                return;
            }

            _statusRectTransform.rotation = _camera.transform.rotation;
        }
    }
}
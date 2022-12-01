using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgentStatusBar : MonoBehaviour {
    // Assignables
    private Camera _camera;
    private RectTransform _statusRectTransform;
    private Vector3 _cameraRotation;

    private Image[] _statusImages;
    private Agent _thisAgent;

    private enum Status {
        HealthBackground,
        Shield,
        Damage,
        Health,
        ManaBackground,
        Mana
    };

    // Variables
    private const float _damageEffectSpeed = 0.5f;
    private const float _damageEffectTimerMax = 0.25f;
    private float _damageEffectTimer = 0f;

    private void Awake() {
        _camera = Camera.main;
        _cameraRotation = _camera.transform.eulerAngles;

        _statusRectTransform = GetComponent<RectTransform>();
        _statusImages = GetComponentsInChildren<Image>();
        _thisAgent = GetComponentInParent<Agent>();
    }

    // Start is called before the first frame update
    private void Start() {
        SetImage((int) Status.Shield, 0f);
        SetImage((int) Status.Damage, 1f);
        SetImage((int) Status.Health, 1f);
        SetImage((int) Status.Mana, (float) _thisAgent.StartingMana / _thisAgent.Mana);
    }

    // Update is called once per frame
    void Update() {
        _statusRectTransform.rotation = Quaternion.Euler(_cameraRotation.x, 0.0f, 0.0f);

        _damageEffectTimer -= Time.deltaTime;

        if (_damageEffectTimer < 0) {
            if (_statusImages[(int) Status.Health].fillAmount < _statusImages[(int) Status.Damage].fillAmount) {
                _statusImages[(int) Status.Damage].fillAmount -= _damageEffectSpeed * Time.deltaTime;
            }
        }
    }

    public void SetImage(int index, float value) {
        _statusImages[index].fillAmount = value;
    }

    public void StartDamageEffect() {
        _damageEffectTimer = _damageEffectTimerMax;
    }
}

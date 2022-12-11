using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UmbraProjects.AutoChess {
    public enum PopupType {
        Physical,
        Magic,
        True,
        Health
    }

    public class StatusPopup : MonoBehaviour {
        private TextMeshPro _textMesh;
        private Color _textColor;
        private float _disappearTimer;

        private static int _sortingOrder;

        private Vector3 _moveVector;

        private const float _DISAPPEAR_TIMER_MAX = 0.5f;



        private void Awake() {
            _textMesh = transform.GetComponent<TextMeshPro>();
        }

        // Update is called once per frame
        void Update() {
            if (_disappearTimer > _DISAPPEAR_TIMER_MAX * 0.85f) {
                // First tenth of popup lifetime
                transform.position += _moveVector * Time.deltaTime;
            }

            _disappearTimer -= Time.deltaTime;

            if (_disappearTimer < 0) {
                // Start disappearing
                const float decreaseScale = 3f;
                const float disappearSpeed = 9f;

                transform.localScale -= Vector3.one * decreaseScale * Time.deltaTime;
                _textColor.a -= disappearSpeed * Time.deltaTime;
                //_textMesh.color = _textColor;

                if (_textColor.a < 0) {
                    Destroy(gameObject);
                }
            }
        }

        // Create popup object
        public static StatusPopup CreatePopup(Vector3 position, int amount, int type, bool isCritical) {
            var popupObject = Instantiate(SpawnManager.Instance.StatusPopupPrefab, position,
                Quaternion.Euler(50, 0, 0));
            var statusPopup = popupObject.GetComponent<StatusPopup>();
            statusPopup.Setup(amount, type, isCritical);

            return statusPopup;
        }

        public void Setup(int value, int type, bool isCritical) {
            switch (type) {
                // Physical damage
                case (int) PopupType.Physical:
                    _textColor = new Color(1, 0.75f, 0, 1);
                    _textMesh.color = _textColor;
                    break;

                // Magic damage
                case (int) PopupType.Magic:
                    _textColor = new Color(0.13f, 0.56f, 0.89f, 1);
                    _textMesh.color = _textColor;
                    break;

                // True damage
                case (int) PopupType.True:
                    _textColor = new Color(1, 1, 1, 1);
                    _textMesh.color = _textColor;
                    break;

                // Healing amount
                case (int) PopupType.Health:
                    _textColor = new Color(0, 0.84f, 0.07f, 1);
                    _textMesh.color = _textColor;
                    break;
            }

            if (!isCritical) {
                // Normal hit
                _textMesh.SetText(value.ToString());
                _textMesh.fontSize = 2;

                _moveVector = new Vector3(Random.Range(-0.25f, 0.25f), -0.25f, Random.Range(-0.25f, 0.25f)) * 8f;
            }
            else {
                // Critical hit
                _textMesh.SetText($"<sprite index=0 color=#{ColorUtility.ToHtmlStringRGB(_textColor)}>"
                                  + value.ToString());
                _textMesh.fontSize = 3;

                _moveVector = new Vector3(Random.Range(-0.25f, 0.25f), -0.5f, Random.Range(-0.25f, 0.25f)) * 10f;
            }

            _disappearTimer = _DISAPPEAR_TIMER_MAX;

            _sortingOrder++;
            _textMesh.sortingOrder = _sortingOrder;
        }
    }
}
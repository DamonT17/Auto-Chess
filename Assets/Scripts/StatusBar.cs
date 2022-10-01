using UnityEngine;

// Class for handling player's status bar movement in game
internal class StatusBar : MonoBehaviour {
    // Assignables
    public PlayerControl Player;

    private Camera _camera;
    private RectTransform _statusRt;
    private Vector3 _cameraRotation;

    // Start is called before the first frame update
    void Start() {
        _camera = Camera.main;
        _cameraRotation = _camera.transform.eulerAngles;

        _statusRt = GetComponent<RectTransform>();
        }

    // Update is called once per frame
    void Update() {
        _statusRt.rotation = Quaternion.Euler(_cameraRotation.x, 0.0f, 0.0f);
    }
}
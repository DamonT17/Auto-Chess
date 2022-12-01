using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusBar : MonoBehaviour {
    // Assignables
    private Camera _camera;
    private RectTransform _statusRectTransform;
    private Vector3 _cameraRotation;

    // Start is called before the first frame update
    void Start() {
        _camera = Camera.main;
        _cameraRotation = _camera.transform.eulerAngles;

        _statusRectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update() {
        _statusRectTransform.rotation = Quaternion.Euler(_cameraRotation.x, 0.0f, 0.0f);
    }
}

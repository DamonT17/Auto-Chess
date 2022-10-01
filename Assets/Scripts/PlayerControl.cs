using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// Base class for Player control
internal class PlayerControl : MonoBehaviour {
    // Assignables
    public Camera GameCamera;
    
    protected Rigidbody PlayerRb;
    protected NavMeshAgent NavMeshAgent;

    private Vector3 _playerPosition;
    private Vector3 _targetPosition;

    private bool _movePlayer = false;

    private int _leftClick = 0;
    private int _rightClick = 1;

    // Start is called before the first frame update
    private void Start() {
        PlayerRb = GetComponent<Rigidbody>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    private void Update() {
        _playerPosition = transform.position;

        // (left-click input)
        if (Input.GetMouseButtonDown(_leftClick)) {
            // TEST CODE, DELETE LATER
            //Instantiate(GameManager.Instance.Spawn.orbPrefab[0], GameManager.Instance.Spawn.GenerateCircularSpawnPosition(),
            //    GameManager.Instance.Spawn.orbPrefab[0].transform.rotation);
        }
        // Get target position player is to move to (right-click input)
        else if (Input.GetMouseButtonDown(_rightClick)) {
            _targetPosition = PlayerManager.Instance.GetMousePosition();
            //Debug.Log(_targetPosition);
            _movePlayer = true;
        }

        if(_movePlayer)
            MovePlayer(_targetPosition);
    }

    // ABSTRACTION
    // Moves player to target destination
    private void MovePlayer(Vector3 target) {
        const float tolerance = 0.0001f;
        
        NavMeshAgent.destination = target;

        if (Math.Abs(_playerPosition.x - target.x) < tolerance && Math.Abs(_playerPosition.z - target.z) < tolerance)
            _movePlayer = false;
    }

    // Player collider events
    private void OnTriggerEnter(Collider other) {
        var colliderTag = GetColliderTag(other);

        switch (colliderTag) {
            case "Coin":    // Coins increase player's money count

                Destroy(other.gameObject);
                break;

            case "Item":    // Items buff player's Agents

                Destroy(other.gameObject);
                break;

            case "Orb":     // Orbs drop either coin(s) or item(s) based off random spawn generator

                Destroy(other.gameObject);
                break;
        }

        if (other.CompareTag("Coin") || other.CompareTag("Item") || other.CompareTag("Orb"))
            Destroy(other.gameObject);
    }

    // Gets tag of game object collided with
    private string GetColliderTag(Collider objectCollider) {
        return objectCollider.tag;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// INHERITANCE
// Inherited manager class to handle spawning of objects
public class SpawnManager : Manager<SpawnManager> {
    // Assignables
    public GameObject CoinPrefab;
    public GameObject ItemPrefab;
    public GameObject[] OrbPrefab = new GameObject[3];

    // Spawn calculation variables
    private Vector3 _spawnCenter = new Vector3(0, 1, 0); // Reference to Game Manager object position
    private const float _spawnRadius = 5.0f;

    // Start is called before the first frame update
    void Start() {
        //Instantiate(orbPrefab[0], GenerateCircularSpawnPosition(), orbPrefab[0].transform.rotation);
    }

    // Update is called once per frame
    void Update() {
        
    }

    // Random generation of a spawn position for orbs
    public Vector3 GenerateCircularSpawnPosition() {
        Vector3 spawnPos;

        float rad = Random.Range(0, _spawnRadius); 
        float ang = Random.Range(0, 360);

        spawnPos.x = _spawnCenter.x + (rad * Mathf.Cos(ang * Mathf.Deg2Rad));
        spawnPos.y = _spawnCenter.y;
        spawnPos.z = _spawnCenter.z + (rad * Mathf.Sin(ang * Mathf.Deg2Rad));

        //Debug.Log("Spawn Point: " + spawnPos);
        return spawnPos;
    }
}

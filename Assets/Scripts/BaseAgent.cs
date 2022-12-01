using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAgent : MonoBehaviour {
    // Assignables
    [Header("Agent Stats")]
    public GameManager.Team Team;
    [Range(1, 5)] public int Cost = 1;
    public string Origin;
    public string Class;
    public int Health;
    public int MaxHealth;
    public int Mana;
    public int StartingMana;
    public int Armor;
    public int MagicResist;
    public int Damage;
    [Range(1, 7)] public int Range = 1;
    public float MoveSpeed = 2f;
    
    // Add header??
    [SerializeField] protected AgentStatusBar StatusBar;
    protected Agent CurrentTarget;
    protected Graph.Node CurrentNode;




    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}

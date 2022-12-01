using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent00 : Agent {
    private string _nodeParent;

    // Awake is called when the script instance is being loaded
    protected override void Awake() {
        base.Awake();

        // Initialize stats
        Cost = 1;
        Origin = "Trickster";
        Class = "Intellect";
        MaxHealth = 500;
        Health = 500; 
        Mana = 60;
        StartingMana = 15;
        Armor = 40;
        MagicResist = 40;
        Damage = 50;
        AttackSpeed = 0.7f;
        CritChance = 0.25f;
        CritDamage = 1.25f;
        Range = 1;
        MoveSpeed = 2f;
    }

    // Update is called once per frame
    public void Update() {
        _nodeParent = currentNode.Parent.parent.name;

        if (GameManager.Instance.IsFightActive && _nodeParent == "Battle Grid") {
            if (!HasEnemy) {
                FindTarget();
            }

            if (IsInRange && !Moving) {
                if (CanAttack) {
                    Attack();
                    CurrentTarget.TakeDamage(Damage);
                }
            }
            else {
                GetInRange();
            }
        }
    }

    /*
    protected override void OnRoundStart() {
        FindTarget();
    }*/
}

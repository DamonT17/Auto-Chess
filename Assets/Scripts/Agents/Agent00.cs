using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent00 : Agent {
    // Awake is called when the script instance is being loaded
    protected void Awake() {
        // Initialize stats
        Cost = 1;
        Origin = "Trickster";
        Class = "Intellect";
        Health = 500; 
        Mana = 60;
        StartingMana = 0;
        Armor = 40;
        MagicResist = 40;
        Damage = 50;
        AttackSpeed = 0.7f;
        CritChance = 0.25f;
        CritDamage = 1.25f;
        Range = 1;
        MoveSpeed = 1f;
    }

    // Update is called once per frame
    public void Update() {
        if(!HasEnemy)
            Debug.Log("No enemy!");
            //FindTarget();

        if (IsInRange && !Moving) {
            if (CanAttack) {
                Attack();
                CurrentTarget.TakeDamage(Damage);
            }
        }
        else GetInRange();
    }

    protected override void OnRoundStart() {
        FindTarget();
    }
}

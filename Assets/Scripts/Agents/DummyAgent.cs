using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAgent : Agent {
    private string _nodeParent;

    protected void Awake() {
        SetAttributes();
        
        AgentAnimator.SetFloat("MoveSpeed", MoveSpeed.Value * ANIMATION_SPEED_MULTIPLIER);
    }

    // Update is called once per frame
    public void Update() {
        _nodeParent = currentNode.Parent.parent.name;

        if (!GameManager.Instance.IsFightActive || _nodeParent != "Battle Grid") {
            return;
        }

        if (!HasEnemy) {
            FindTarget();
        }

        if (InRange && !IsMoving) {
            if (!CanAttack) {
                return;
            }
            
            if (Mana.Value < Mana.MaxValue) {
                // Basic attack
                Attack();
                CurrentTarget.ApplyDamage(Damage.Value, (int) PopupType.Physical, ApplyCriticalHit(CritRate));
            }
            else {
                // Ability attack
                // AbilityAttack();
                // ApplyAbilityDamage();

                ResetMana();
            }
        }
        else {
            GetInRange(CurrentTarget);
        }
    }

    // Set Agent's attributes on instantiation
    private void SetAttributes() {
        Cost.BaseValue = 1;

        Health.BaseValue = 500;
        Health.MaxValue = 500;
        
        Mana.BaseValue = 15;
        Mana.MaxValue = 60;

        Armor.BaseValue = 40;
        MagicResist.BaseValue = 40;
        
        Damage.BaseValue = 50;

        AttackSpeed.BaseValue = 0.7f;
        AttackSpeed.MaxValue = 5.0f;
        
        CritRate.BaseValue = 0.25f;
        CritRate.MaxValue = 1.0f;

        CritDamage.BaseValue = 0.3f;
        
        Range.BaseValue = 1;
        Range.MaxValue = 7;

        MoveSpeed.BaseValue = 2.0f;
    }
}

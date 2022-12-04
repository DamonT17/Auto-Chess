using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Base class for all Agents
public class Agent : MonoBehaviour {
    [Header("Agent Generics")]
    public GameManager.Team Team;
    [SerializeField] protected AgentStatusBar StatusBar;
    [SerializeField] protected Animator AgentAnimator;

    [Header("Agent Attributes")]
    public Attribute Cost;
    public Attribute Health;
    public Attribute Mana;
    public Attribute Armor;
    public Attribute MagicResist;
    public Attribute Damage;
    public Attribute AttackSpeed;
    public Attribute CritRate;
    public Attribute CritDamage;
    public Attribute Range;
    public Attribute MoveSpeed;

    // Variables
    protected Agent CurrentTarget;
    protected Graph.Node currentNode;
    protected Graph.Node DestinationNode;
    protected float WaitBetweenAttack;

    public Graph.Node CurrentNode => currentNode;

    // Flags
    protected bool HasEnemy => CurrentTarget != null;
    protected bool InRange => CurrentTarget != null && Vector3.Distance(this.transform.position, CurrentTarget.transform.position) <= Range.BaseValue;
    protected bool IsMoving;
    protected bool CanAttack = true;
    protected bool Dead;

    // Constants
    protected const float ANIMATION_SPEED_MULTIPLIER = 0.25f;
    protected const float PRE_MITIGATION_DAMAGE_PERCENT = 0.01f;
    protected const float POST_MITIGATION_DAMAGE_PERCENT = 0.07f;
    protected const float MANA_GENERATION_ON_ATTACK = 10f;

    protected enum StatusBarState {
        Shield,
        Damage,
        Health,
        Mana
    }

    // Start is called before the first frame update
    protected void Start() {
        

        // GameManager.Instance.OnRoundStart += OnRoundStart;      // Correctly called??
        // GameManager.Instance.OnRoundEnd += OnRoundEnd;          // Correctly called??
        // GameManager.Instance.OnAgentDeath += OnAgentDeath;      // Correctly called??
    }

    // Initialization of Agent for Player's team
    public void Setup(GameManager.Team team, Graph.Node node) {
        Team = team;

        this.currentNode = node;
        node.SetOccupied(true);
        transform.SetParent(node.Parent);

        if (team == GameManager.Team.Team1) {
            transform.SetPositionAndRotation(node.WorldPosition, Quaternion.Euler(0, node.YRotation, 0));
        }
        else {
            // Disable player drag capabilities for enemy Agents
            this.GetComponentInParent<EventTrigger>().enabled = false;

            transform.SetPositionAndRotation(node.WorldPosition,
                Quaternion.Euler(0, node.YRotation - 180.0f, 0));
        }
    }

    // Algorithm to find target for Agent to attack
    protected void FindTarget() {
        var allEnemyAgents = GameManager.Instance.GetAgents(
            Team == GameManager.Team.Team1 ? GameManager.Team.Team2 : GameManager.Team.Team1);

        if (allEnemyAgents == null) {
            return;
        }

        var minDistance = Mathf.Infinity;
        Agent target = null;

        foreach (var enemy in allEnemyAgents) {
            var enemyDistance = Vector3.Distance(enemy.transform.position, this.transform.position);

            if (!(enemyDistance <= minDistance)) {
                continue;
            }

            minDistance = enemyDistance;
            target = enemy;
        }

        CurrentTarget = target;
    }

    // Algorithm for Agent path to enemy along a Graph's nodes
    protected bool MoveTowards(Graph.Node nextNode) {
        if (nextNode == null) {
            return false;
        }

        var direction = nextNode.WorldPosition - this.transform.position;
        var rotation = Vector3.RotateTowards(transform.forward, direction,
            MoveSpeed.Value * Time.deltaTime, 0.0f);

        // Agent has made it to the next node
        if (direction.sqrMagnitude <= 0.005f) {
            transform.position = nextNode.WorldPosition;
            return false;
        }

        this.transform.position += MoveSpeed.Value * Time.deltaTime * direction.normalized;
        transform.rotation = Quaternion.LookRotation(rotation);

        return true;
    }

    // Move Agent to node that is in attacking range of target
    protected void GetInRange(Agent targetAgent) {
        if (targetAgent == null) {
            return;
        }

        if (!IsMoving) {
            DestinationNode = null;

            var possibleNodes = GridManager.Instance.GetNeighborNodes(targetAgent.CurrentNode);
            possibleNodes = possibleNodes.OrderBy(node => Vector3.Distance(
                node.WorldPosition, this.transform.position)).ToList();

            foreach (var node in possibleNodes) {
                if (node.IsOccupied) {
                    continue;
                }

                DestinationNode = node;
                break;
            }

            if (DestinationNode == null) {
                return;
            }

            var path = GridManager.Instance.GetNodePath(currentNode, DestinationNode);
            if (path == null || path[1].IsOccupied) {
                return;
            }

            path[1].SetOccupied(true);
            DestinationNode = path[1];
        }

        IsMoving = MoveTowards(DestinationNode);
        AgentAnimator.SetBool("IsMoving", IsMoving);

        if (!IsMoving) {
            // Free previous node
            currentNode.SetOccupied(false);
            SetCurrentNode(DestinationNode);
        }

        // Need check to see if actually in range of target, not just next node 
    }

    // Set current position of Agent on Graph
    public void SetCurrentNode(Graph.Node node) {
        currentNode = node;
        SetCurrentParent(currentNode);
    }

    // Set Agent's current node as the Parent object
    private void SetCurrentParent(Graph.Node node) {
        transform.SetParent(node.Parent);
    }

    // POLYMORPHISM
    // Agent attacking method 
    protected virtual void Attack() {
        if (!CanAttack) {
            return;
        }

        AgentAnimator.SetBool("CanAttack", CanAttack);
        ManaGenerationOnAttack(MANA_GENERATION_ON_ATTACK);

        WaitBetweenAttack = 1 / AttackSpeed.Value;
        StartCoroutine(AttackCoroutine());
    }

    // Time interval for Agent attack speed
    private IEnumerator AttackCoroutine() {
        CanAttack = false;
        yield return null;

        AgentAnimator.SetBool("CanAttack", CanAttack);

        yield return new WaitForSeconds(WaitBetweenAttack);
        CanAttack = true;
    }

    protected virtual bool ApplyCriticalHit(Attribute critRate) {
        return Random.Range(0f, 1f) <= critRate.Value;
    }

    // Calculates the pre-mitigation damage value based on Agent CritRate and CritDamage attributes
    protected float PreMitigationDamage(float amount, bool isCritical) {
        var preMitigationDamage = !isCritical ? amount : amount * (1 + CritDamage.Value);

        return preMitigationDamage;
    }

    // Calculates the post mitigation damage value based on Agent Armor/MagicResist attributes
    protected float PostMitigationDamage(float amount, int type, bool isCritical) {
        var postMitigationDamage = !isCritical ? amount : amount * (1 + CritDamage.Value);

        switch (type) {
            case (int) PopupType.Physical:
                if (Armor.Value >= 0) {
                    postMitigationDamage *= (100 / (100 + Armor.Value));
                }
                else {
                    postMitigationDamage *= (2 - (100 / (100 - Armor.Value)));
                }

                break;
            case (int) PopupType.Magic:
                if (Armor.Value >= 0) {
                    postMitigationDamage *= (100 / (100 + MagicResist.Value));
                }
                else {
                    postMitigationDamage *= (2 - (100 / (100 - MagicResist.Value)));
                }

                break;
        }

        return postMitigationDamage;
    }

    // Method to deal damage to Agent
    public void ApplyDamage(float amount, int type, bool isCritical) {
        var preMitigationDamage = PreMitigationDamage(amount, isCritical);
        var postMitigationDamage = PostMitigationDamage(amount, type, isCritical);

        Health.Value -= postMitigationDamage;
        StatusBar.SetImage((int) StatusBarState.Health, Health.Value / Health.MaxValue);
        StatusBar.StartDamageEffect();

        ManaGenerationOnDamage(preMitigationDamage, postMitigationDamage);

        StatusPopup.CreatePopup(CurrentTarget.transform.position + Vector3.up * 0.75f,
            (int) postMitigationDamage, type, isCritical);

        if (Health.Value <= 0 && !Dead) {
            Dead = true;
            //AgentAnimator.SetBool("IsDead", Dead);
            currentNode.SetOccupied(false);
            GameManager.Instance.AgentDead(this);
        }
    }

    // Increases Agent's mana by MANA_GENERATION_ON_ATTACK
    protected void ManaGenerationOnAttack(float amount) {
        Mana.Value += amount;
        StatusBar.SetImage((int) StatusBarState.Mana, Mana.Value / Mana.MaxValue);
    }

    // Increases Agent's mana by percentages of pre- and post-mitigation damages
    protected void ManaGenerationOnDamage(float preMitigationDamage, float postMitigationDamage) {
        Mana.Value += PRE_MITIGATION_DAMAGE_PERCENT * preMitigationDamage +
                      POST_MITIGATION_DAMAGE_PERCENT * postMitigationDamage;
        StatusBar.SetImage((int) StatusBarState.Mana, Mana.Value / Mana.MaxValue);
    }

    protected void ResetMana() {
        Mana.Value = 0f;
        StatusBar.SetImage((int) StatusBarState.Mana, Mana.Value);
    }

    // CONTINUE HERE WITH AGENT METHODS FOR ROUND START, END, AND DEATH

    protected virtual void OnRoundStart() { }
    protected virtual void OnRoundEnd() { }
    protected virtual void OnAgentDeath(Agent deadAgent) { }
}

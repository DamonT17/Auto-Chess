using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] protected bool CanAttack; // Continue here, value never changes state???
    protected bool Dead;

    // Constants
    protected const float ANIMATION_SPEED_MULTIPLIER = 0.25f;

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

    // ABSTRACTION
    // Method to deal damage to Agent
    public void TakeDamage(int amount) {
        Health.Value -= amount;
        StatusBar.SetImage((int) StatusBarState.Health, (float) Health.Value / Health.MaxValue);
        StatusBar.StartDamageEffect();

        Debug.Log($"{this.Team} {this.name}'s health: {Health}");

        if (Health.Value <= 0 && !Dead) {
            Dead = true;
            //AgentAnimator.SetBool("IsDead", Dead);
            currentNode.SetOccupied(false);
            GameManager.Instance.AgentDead(this);
        }
    }

    // POLYMORPHISM
    // Agent attacking method 
    protected virtual void Attack() {
        if (!CanAttack) {
            return;
        }
        
        StartCoroutine(AttackCoroutine());
    }

    // Time interval for Agent attack speed
    private IEnumerator AttackCoroutine() {
        CanAttack = false;
        AgentAnimator.SetBool("CanAttack", CanAttack);
        yield return null;

        yield return new WaitForSeconds(AttackSpeed.Value);
        CanAttack = true;
        AgentAnimator.SetBool("CanAttack", CanAttack);
    }

    // CONTINUE HERE WITH AGENT METHODS FOR ROUND START, END, AND DEATH

    protected virtual void OnRoundStart() { }
    protected virtual void OnRoundEnd() { }
    protected virtual void OnAgentDeath(Agent deadAgent) { }
}

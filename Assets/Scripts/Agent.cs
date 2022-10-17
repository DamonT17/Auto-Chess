using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

// Base class for all Agents
public class Agent : MonoBehaviour {
    public GameManager.Team Team;
    protected Agent CurrentTarget;
    protected Graph.Node currentNode;

    protected Animator AgentAnimator;
    
    public Graph.Node CurrentNode => currentNode;

    protected bool HasEnemy => CurrentTarget != null;

    protected bool IsInRange => CurrentTarget != null &&
                                Vector3.Distance(this.transform.position, CurrentTarget.transform.position) <= Range;

    protected bool Moving;
    protected Graph.Node DestinationNode;

    protected bool Dead;
    protected bool CanAttack = true;
    protected float WaitBetweenAttack;

    // Agent Stats
    [Range(1, 5)]
    public int Cost = 1;

    public string Origin;
    public string Class;
    public int Health, Mana, StartingMana, Armor, MagicResist, Damage;
    public float AttackSpeed, CritChance, CritDamage;
    [Range(1, 7)]
    public int Range = 1;
    public float MoveSpeed = 2f;
    
    // Start is called before the first frame update
    protected void Start() {
        AgentAnimator = GetComponentInChildren<Animator>();

        // GameManager.Instance.OnRoundStart += OnRoundStart;      // Correctly called??
        // GameManager.Instance.OnRoundEnd += OnRoundEnd;          // Correctly called??
        // GameManager.Instance.OnAgentDeath += OnAgentDeath;      // Correctly called??
    }

    // ABSTRACTION
    // Initialization of Agent for Player's team
    public void Setup(GameManager.Team team, Graph.Node node) {
        Team = team;

        this.currentNode = node;
        node.SetOccupied(true);

        transform.SetParent(node.Parent);

        if(team == GameManager.Team.Team1)
            transform.SetPositionAndRotation(node.WorldPosition, Quaternion.Euler(0, node.YRotation, 0));
        else {
            // Update the line below if no fighting events occur
            this.GetComponentInParent<EventTrigger>().enabled = false;

            transform.SetPositionAndRotation(node.WorldPosition,
                Quaternion.Euler(0, node.YRotation - 180.0f, 0));
        }
    }

    // ABSTRACTION
    // Algorithm to find target for Agent to attack
    protected void FindTarget() {
        var allEnemies = GameManager.Instance.GetAgents(
            Team == GameManager.Team.Team1 ? GameManager.Team.Team2 : GameManager.Team.Team1);

        var minDistance = Mathf.Infinity;
        Agent target = null;

        foreach (var enemy in allEnemies) {
            if (Vector3.Distance(enemy.transform.position, this.transform.position) <= minDistance) {
                minDistance = Vector3.Distance(enemy.transform.position, this.transform.position);
                target = enemy;
            }
        }

        CurrentTarget = target;
    }

    // ABSTRACTION
    // Algorithm for Agent path to along a Graph's nodes
    protected bool MoveTowards(Graph.Node nextNode) {
        var direction = nextNode.WorldPosition - this.transform.position;
        var rotation = Vector3.RotateTowards(transform.forward, direction,
            MoveSpeed * Time.deltaTime, 0.0f);

        if (direction.sqrMagnitude <= 0.005f) {
            transform.position = nextNode.WorldPosition;
            return true;
        }

        this.transform.position += MoveSpeed * Time.deltaTime * direction.normalized;
        
        Debug.DrawRay(transform.position, rotation, Color.red);
        transform.rotation = Quaternion.LookRotation(rotation);

        //AgentAnimator.SetFloat("MoveSpeed", MoveSpeed);

        return false;
    }

    // ABSTRACTION
    // Move Agent to get in attacking range of target
    protected void GetInRange() {
        if (CurrentTarget == null)
            return;

        if (!Moving) {
            DestinationNode = null;
            var candidateNodes = GridManager.Instance.GetNeighborNodes(CurrentTarget.CurrentNode);

            candidateNodes = candidateNodes.OrderBy(x => 
                    Vector3.Distance(x.WorldPosition, this.transform.position)).ToList();

            foreach (var n in candidateNodes) {
                if (!n.IsOccupied) {
                    DestinationNode = n;
                    break;
                }
            }

            if (DestinationNode == null)
                return;

            var path = GridManager.Instance.GetNodePath(currentNode, DestinationNode);

            if (path == null && path.Count >= 1)
                return;

            if (path[1].IsOccupied)
                return;

            path[1].SetOccupied(true);
            DestinationNode = path[1];
        }

        Moving = !MoveTowards(DestinationNode);

        if (!Moving) {
            // Free previous node
            currentNode.SetOccupied(false);
            SetCurrentNode(DestinationNode);
        }
    }

    // ABSTRACTION
    // Set of current position of Agent on Graph
    public void SetCurrentNode(Graph.Node node) {
        currentNode = node;
        SetCurrentParent(currentNode);
    }

    // Set of Agent's current node as the Parent object
    private void SetCurrentParent(Graph.Node node) {
        transform.SetParent(node.Parent);
    }

    // ABSTRACTION
    // Method to deal damage to Agent
    public void TakeDamage(int amount) {
        Health -= amount;

        if (Health <= 0 && !Dead) {
            Dead = true;
            currentNode.SetOccupied(false);
            GameManager.Instance.AgentDead(this);
        }
    }

    // POLYMORPHISM
    // Agent attacking method 
    protected virtual void Attack() {
        if (!CanAttack)
            return;

        WaitBetweenAttack = 1 / AttackSpeed;
        StartCoroutine(WaitCoroutine());
    }

    // Time interval for Agent attack speed
    private IEnumerator WaitCoroutine() {
        CanAttack = false;
        yield return null;

        yield return new WaitForSeconds(WaitBetweenAttack);
        CanAttack = true;
    }

    // CONTINUE HERE WITH AGENT METHODS FOR ROUND START, END, AND DEATH

    protected virtual void OnRoundStart() { }
    protected virtual void OnRoundEnd() { }
    protected virtual void OnAgentDeath(Agent deadAgent) { }
}

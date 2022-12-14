using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

// Base class for all Agents
public class Agent : MonoBehaviour {
    protected GameManager.Team MyTeam;
    protected Agent CurrentTarget;
    protected Graph.Node currentNode;
    
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
    public int Cost = 1;
    public string Origin;
    public string Class;
    public int Health, Mana, StartingMana, Armor, MagicResist, Damage;
    public float AttackSpeed, CritChance, CritDamage;
    [Range(1, 7)]
    public int Range = 1;
    public float MoveSpeed = 1f;
    
    // Start is called before the first frame update
    protected void Start() {
        GameManager.Instance.OnRoundStart += OnRoundStart;
        GameManager.Instance.OnRoundEnd += OnRoundEnd;
        GameManager.Instance.OnAgentDeath += OnAgentDeath;
    }

    // ABSTRACTION
    // Initialization of Agent for Player's team
    public void Setup(GameManager.Team team, Graph.Node node) {
        MyTeam = team;

        this.currentNode = node;
        node.SetOccupied(true);

        transform.SetParent(node.Parent);
        transform.SetPositionAndRotation(node.WorldPosition, Quaternion.Euler(0, node.YRotation, 0));
    }

    // ABSTRACTION
    // Algorithm to find target for Agent to attack
    protected void FindTarget() {
        var allEnemies = GameManager.Instance.GetAgents(MyTeam);
        float minDistance = Mathf.Infinity;
        Agent agent = null;

        foreach (Agent a in allEnemies) {
            if (Vector3.Distance(a.transform.position, this.transform.position) <= minDistance) {
                minDistance = Vector3.Distance(a.transform.position, this.transform.position);
                agent = a;
            }
        }

        CurrentTarget = agent;
    }

    // ABSTRACTION
    // Algorithm for Agent path to along a Graph's nodes
    protected bool MoveTowards(Graph.Node nextNode) {
        var direction = nextNode.WorldPosition - this.transform.position;

        if (direction.sqrMagnitude <= 0.005f) {
            transform.position = nextNode.WorldPosition;
            return true;
        }

        this.transform.position += direction.normalized * MoveSpeed * Time.deltaTime;
        return false;
    }

    // ABSTRACTION
    // Move Agent to get in attacking range of target
    protected void GetInRange() {
        if (CurrentTarget == null)
            return;

        if (!Moving) {
            DestinationNode = null;
            List<Graph.Node> candidateNodes = GridManager.Instance.GetNeighborNodes(CurrentTarget.CurrentNode);

            candidateNodes = candidateNodes.OrderBy(x => 
                    Vector3.Distance(x.WorldPosition, this.transform.position)).ToList();

            foreach (var n in candidateNodes) {
                if (n.IsOccupied) {
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

    // CONTINUE HERE WITH AGENT METHODS FOR ROUND START, END, AND DEATH

    protected virtual void OnRoundStart() { }
    protected virtual void OnRoundEnd() { }
    protected virtual void OnAgentDeath(Agent deadAgent) { }
}

using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UmbraProjects.Managers;

// INHERITANCE
// Inherited manager class to setup all environment grids
public class GridManager : Manager<GridManager> {
    // Assignables
    public Transform BattleGrid;
    public Transform MyBench;
    public Transform EnemyBench;

    // Variables
    public Graph Graph;
    public Graph BenchGraph;
    public Graph EnemyBenchGraph;
    protected Dictionary<GameManager.Team, int> StartPositionPerTeam;

    [SerializeField] private int _fromIndex = 0;
    [SerializeField] private int _toIndex = 0;

    private List<Tile> _allTiles = new List<Tile>();
    public List<Tile> MyTiles = new List<Tile>();
    public List<Tile> EnemyTiles = new List<Tile>();
    public Tile CenterTile;

    public List<Tile> MyBenchTiles = new List<Tile>();
    public List<Tile> EnemyBenchTiles = new List<Tile>();

    // Awake is called when the script instance is being loaded
    protected new void Awake() {
        base.Awake();

        SetupTiles();

        InitializeGraph();
        InitializeBench(MyBenchTiles, ref BenchGraph);
        InitializeBench(EnemyBenchTiles, ref EnemyBenchGraph);

        StartPositionPerTeam = new Dictionary<GameManager.Team, int> {
            {GameManager.Team.Team1, 0},
            {GameManager.Team.Team2, _allTiles.Count - 1}
        };
    }

    // ABSTRACTION
    // Initialization of graph nodes/edges for the battle grid
    private void InitializeGraph() {
        Graph = new Graph();

        foreach (var t in _allTiles) {
            var parentTransform = t.transform;
            Graph.AddNode(parentTransform);
        }

        var allNodes = Graph.Nodes;

        foreach (var from in allNodes) {
            foreach (var to in allNodes) {
                if (Vector3.Distance(from.WorldPosition, to.WorldPosition) <= 1f && from != to)
                    Graph.AddEdge(from, to);
            }
        }
    }

    // ABSTRACTION
    // Initialization of graph nodes for team benches
    private void InitializeBench(List<Tile> tiles, ref Graph graph) {
        graph = new Graph();

        foreach (var t in tiles) {
            var parentTransform = t.transform;
            graph.AddNode(parentTransform);
        }
    }

    // ABSTRACTION
    // Initialization of all tiles in environment
    private void SetupTiles() {
        // Initialization of battlefield tiles
        _allTiles = BattleGrid.GetComponentsInChildren<Tile>().ToList();
        MyTiles = _allTiles.GetRange(0, _allTiles.Count / 2);
        EnemyTiles = _allTiles.GetRange((_allTiles.Count / 2) + 1, _allTiles.Count / 2);
        CenterTile = _allTiles[30];

        // Initialization of bench tiles
        MyBenchTiles = MyBench.GetComponentsInChildren<Tile>().ToList();
        EnemyBenchTiles = EnemyBench.GetComponentsInChildren<Tile>().ToList();

        foreach (var t in _allTiles)
            t.SetAlpha(0f);

        foreach (var t in MyBenchTiles)
            t.SetAlpha(0f);

        foreach (var t in EnemyBenchTiles)
            t.SetAlpha(0f);
    }

    // ABSTRACTION
    // Get all tiles currently occupied by Agents in the given Graph
    public int GetOccupiedTiles(Graph graph, List<Tile> tiles) {
        var count = 0;

        for (var i = 0; i < tiles.Count; i++) {
            if (graph.Nodes[i].IsOccupied)
                count++;
        }

        return count;
    }

    // ABSTRACTION
    // Get all free nodes in the given Graph
    public Graph.Node GetFreeNode(GameManager.Team teamNumber, Graph graph) {
        var startIndex = StartPositionPerTeam[teamNumber];
        var currentIndex = startIndex;

        while (graph.Nodes[currentIndex].IsOccupied) {
            if (startIndex == 0) {
                currentIndex++;

                if (currentIndex == graph.Nodes.Count)
                    return null;
            }
            else {
                currentIndex--;

                if (currentIndex == -1)
                    return null;
            }
        }

        return graph.Nodes[currentIndex];
    }

    // ABSTRACTION
    // Get the shortest possible path between two nodes
    public List<Graph.Node> GetNodePath(Graph.Node from, Graph.Node to) {
        return Graph.GetShortestPath(from, to);
    }

    // ABSTRACTION
    // Get all neighboring Nodes of a given Node
    public List<Graph.Node> GetNeighborNodes(Graph.Node to) {
        return Graph.NeighborNodes(to);
    }

    // ABSTRACTION
    // Get the Node for the given Tile
    public Graph.Node GetNodeForTile(Tile tile) {
        var tileParent = tile.transform.parent.name;
        var graph = tileParent switch {
            "Battle Grid" => Graph,
            "Player Agents" => BenchGraph,
            "Enemy Agents" => EnemyBenchGraph,
            _ => null
        };

        if (graph != null) {
            var allNodes = graph.Nodes;

            foreach (var t in allNodes) {
                if (tile.transform.GetSiblingIndex() == t.Index)
                    return t;
            }
        }
        
        return null;
    }

    // ABSTRACTION
    // Check if the a Node is free in the Graph
    public bool IsNodeFree(Graph graph) {
        return graph.Nodes.Any(node => !node.IsOccupied);
    }
    
    // Draws the battle grid nodes/edges for debugging purposes
    [UsedImplicitly]
    private void OnDrawGizmos() {
        if (!Application.isPlaying)
            return;

        // Draw team benches
        var allBenchNodes = BenchGraph?.Nodes;

        if (allBenchNodes == null)
            return;

        foreach (var n in allBenchNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        // Draw enemy team benches
        var allEnemyBenchNodes = EnemyBenchGraph?.Nodes;

        if (allEnemyBenchNodes == null)
            return;

        foreach (var n in allEnemyBenchNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        // Draw battlefield
        var allNodes = Graph?.Nodes;

        if (allNodes == null)
            return;

        foreach (var n in allNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        var allEdges = Graph.Edges;

        if (allEdges == null)
            return;

        foreach (var e in allEdges)
            Debug.DrawLine(e.From.WorldPosition, e.To.WorldPosition, Color.magenta, 100);

        if (_fromIndex >= allNodes.Count || _toIndex >= allNodes.Count)   // Do not draw path if indices out of range
            return;

        // Test feature for GetShortestPath algorithm
        var path = GetNodePath(allNodes[_fromIndex], allNodes[_toIndex]);

        if (path.Count > 1) {
            for(var i = 1; i < path.Count; i++)
                Debug.DrawLine(path[i-1].WorldPosition, path[i].WorldPosition, Color.red, 10);
        }
    }
}

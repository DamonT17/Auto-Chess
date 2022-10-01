using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// INHERITANCE
// Inherited manager class to setup all environment grids
public class GridManager : Manager<GridManager> {
    // Assignables
    public Transform BattleGrid;
    public Transform TeamBench;
    public Transform EnemyTeamBench;

    // Variables
    public Graph Graph;
    public Graph BenchGraph;
    public Graph EnemyBenchGraph;
    protected Dictionary<GameManager.Team, int> StartPositionPerTeam;

    [SerializeField] private int fromIndex = 0;
    [SerializeField] private int toIndex = 0;

    private List<Tile> _allBattleTiles = new List<Tile>();
    public List<Tile> MyTiles = new List<Tile>();
    public List<Tile> EnemyTiles = new List<Tile>();
    public Tile CenterTile;

    public List<Tile> MyBenchTiles = new List<Tile>();
    public List<Tile> EnemyBenchTiles = new List<Tile>();

    // Awake is called when the script instance is being loaded
    protected new void Awake() {
        base.Awake();

        _allBattleTiles = BattleGrid.GetComponentsInChildren<Tile>().ToList();
        MyBenchTiles = TeamBench.GetComponentsInChildren<Tile>().ToList();
        EnemyBenchTiles = EnemyTeamBench.GetComponentsInChildren<Tile>().ToList();

        InitializeGraph();
        InitializeBench(MyBenchTiles, ref BenchGraph);
        InitializeBench(EnemyBenchTiles, ref EnemyBenchGraph);

        SetupTiles();

        StartPositionPerTeam = new Dictionary<GameManager.Team, int>();
        StartPositionPerTeam.Add(GameManager.Team.Team1, 0);
        StartPositionPerTeam.Add(GameManager.Team.Team2, 1);
    }

    // ABSTRACTION
    // Initialization of graph nodes/edges for the battle grid
    private void InitializeGraph() {
        Graph = new Graph();

        foreach (var t in _allBattleTiles) {
            var parentTransform = t.transform;
            Graph.AddNode(parentTransform);
        }

        var allNodes = Graph.Nodes;

        foreach (Graph.Node from in allNodes) {
            foreach (Graph.Node to in allNodes) {
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
        foreach (Tile tile in _allBattleTiles)
            tile.SetAlpha(0f);

        foreach (Tile tile in MyBenchTiles)
            tile.SetAlpha(0f);

        foreach (Tile tile in EnemyBenchTiles) {
            tile.SetAlpha(0f);
            tile.gameObject.SetActive(false);
        }

        MyTiles.AddRange(_allBattleTiles.GetRange(0, 30));
        EnemyTiles.AddRange(_allBattleTiles.GetRange(31, 30));
        CenterTile = _allBattleTiles[30];   // Midpoint of all battle tiles

        foreach (Tile tile in EnemyTiles)
            tile.gameObject.SetActive(false);

        CenterTile.gameObject.SetActive(false);
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
        int startIndex = StartPositionPerTeam[teamNumber];
        int currentIndex = startIndex;

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

        foreach (Graph.Node n in allBenchNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        // Draw enemy team benches
        var allEnemyBenchNodes = EnemyBenchGraph?.Nodes;

        if (allEnemyBenchNodes == null)
            return;

        foreach (Graph.Node n in allEnemyBenchNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        // Draw battlefield
        var allNodes = Graph?.Nodes;

        if (allNodes == null)
            return;

        foreach (Graph.Node n in allNodes) {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.WorldPosition, 0.2f);
        }

        var allEdges = Graph.Edges;

        if (allEdges == null)
            return;

        foreach (Graph.Edge e in allEdges) {
            Debug.DrawLine(e.From.WorldPosition, e.To.WorldPosition, Color.magenta, 100);
        }
        
        if (fromIndex >= allNodes.Count || toIndex >= allNodes.Count)   // Do not draw path if indices out of range
            return;

        // Test feature for GetShortestPath algorithm
        List<Graph.Node> path = GetNodePath(allNodes[fromIndex], allNodes[toIndex]);
        if (path.Count > 1) {
            for(int i = 1; i < path.Count; i++)
                Debug.DrawLine(path[i-1].WorldPosition, path[i].WorldPosition, Color.red, 10);
        }
    }
}

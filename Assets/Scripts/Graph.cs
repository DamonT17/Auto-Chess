using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Class for setting up the environment's graph data structures
public class Graph {
    // ENCAPSULATION
    public List<Node> Nodes { get; }
    public List<Edge> Edges { get; }

    // Constructor for Graph class
    public Graph() {
        Nodes = new List<Node>();   // Should the public list be called here??
        Edges = new List<Edge>();   // Should the public list be called here??
    }

    // Check if there is an adjacent node through a Node's edges
    public bool Adjacent(Node from, Node to) {
        foreach (var e in Edges) {
            if (e.From == from && e.To == to)
                return true;
        }

        return false;
    }

    // Return list of neighboring nodes given a reference node
    public List<Node> NeighborNodes(Node from) {
        var resultNodes = new List<Node>();

        foreach (var e in Edges) {
            if (e.From == from)
                resultNodes.Add(e.To);
        }

        return resultNodes;
    }

    // Creation of Node at position of a parent object
    public void AddNode(Transform parent) {
        Nodes.Add(new Node(Nodes.Count, parent));
        parent.SetParent(parent);
    }

    // Creation of an edge between two existing nodes
    public void AddEdge(Node from, Node to) {
        Edges.Add(new Edge(from, to , 1));
    }

    // Calculation of distance between two nodes
    public float Distance(Node from, Node to) {
        foreach (var e in Edges) {
            if (e.From == from && e.To == to)
                return e.GetWeight();
        }

        return Mathf.Infinity;
    }

    // POLYMORPHISM
    // Calculation of the shortest path between two given nodes
    public virtual List<Node> GetShortestPath(Node startNode, Node endNode) {
        var nodePath = new List<Node>();         // List of nodes along Agent path
        var unvisitedNodes = new List<Node>();   // List of unvisited nodes

        var previousNodes = new Dictionary<Node, Node>();    // Agent path's previous nodes
        var nodeDistances = new Dictionary<Node, float>();   // Node distances along Agent path

        // If the start & end node are equal, return start node
        if (startNode == endNode) {
            nodePath.Add(startNode);
            return nodePath;
        }

        // Initialize all nodes 'cost' to Infinity
        foreach (var node in Nodes) {
            unvisitedNodes.Add(node);                   // Initialize unvisitied nodes list to all nodes
            nodeDistances.Add(node, float.MaxValue);    // Initialize node distance to Infinity
        }

        nodeDistances[startNode] = 0f;                  // Re-initialize starting node 'cost' to zero

        while (unvisitedNodes.Count != 0) {
            // Order unvisitedNodes list to order of nodeDistances list
            unvisitedNodes = unvisitedNodes.OrderBy(node => nodeDistances[node]).ToList();

            // Get node with smallest 'cost' and remove from unvisitedNodes list (starting node)
            var currentNode = unvisitedNodes[0];
            unvisitedNodes.Remove(currentNode);

            // Return nodePath when currentNode equals endNode
            if (currentNode == endNode) {
                // Generate shortest node path
                while (previousNodes.ContainsKey(currentNode)) {
                    nodePath.Insert(0, currentNode);        // Insert node into final calculation of node path
                    currentNode = previousNodes[currentNode];    // Traverse from start to end node   
                }

                nodePath.Insert(0, currentNode);            // Insert node into final calculation of node path
                break;
            }

            // Loop through node connections (neighbors) & where connection to neighboring node is available
            var neighborNodes = NeighborNodes(currentNode);

            foreach (var neighborNode in neighborNodes) {
                var length = Vector3.Distance(currentNode.WorldPosition, neighborNode.WorldPosition);
                var alt = nodeDistances[currentNode] + length;

                if (alt < nodeDistances[neighborNode]) {
                    nodeDistances[neighborNode] = alt;
                    previousNodes[neighborNode] = currentNode;
                }
            }
        }

        return nodePath;
    }

    // Class for setting up the graph's nodes
    public class Node {
        public int Index;

        public Transform Parent;
        public Vector3 WorldPosition;
        public float YRotation;
        
        // ENCAPSULATION
        public bool IsOccupied { get; private set; }

        // Constructor for Node class
        public Node(int index, Transform parent) {
            this.Index = index;
            this.Parent = parent;
            this.WorldPosition = parent.position;
            this.YRotation = parent.eulerAngles.y;

            IsOccupied = false;
        }

        // Sets node to occupied when Agent is "over" node
        public void SetOccupied(bool val) {
            IsOccupied = val;
        }
    }

    // Class for setting up the environment's edges
    public class Edge {
        public Node From, To;

        private readonly float _weight;

        // Constructor for Edge class
        public Edge(Node from, Node to, float weight) {
            this.From = from;
            this.To = to;
            this._weight = weight;
        }

        // Value of constant for determining paths between nodes
        public float GetWeight() {
            return To.IsOccupied ? Mathf.Infinity : _weight;
        }
    }
}

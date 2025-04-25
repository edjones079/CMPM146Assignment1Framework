using UnityEngine;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour
{
    // Assignment 2: Implement AStar
    //
    // DO NOT CHANGE THIS SIGNATURE (parameter types + return type)
    // AStar will be given the start node, destination node and the target position, and should return 
    // a path as a list of positions the agent has to traverse to reach its destination, as well as the
    // number of nodes that were expanded to find this path
    // The last entry of the path will be the target position, and you can also use it to calculate the heuristic
    // value of nodes you add to your search frontier; the number of expanded nodes tells us if your search was
    // efficient
    //
    // Take a look at StandaloneTests.cs for some test cases
    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
    {
        var openSet = new SortedList<float, GraphNode>();
        var openCosts = new Dictionary<int, float>();
        var cameFrom = new Dictionary<int, (GraphNode, GraphNeighbor)>();
        var gScore = new Dictionary<int, float>();
        var fScore = new Dictionary<int, float>();
        var closedSet = new HashSet<int>();

        int expanded = 0;

        int startID = start.GetID();
        gScore[startID] = 0f;
        fScore[startID] = Vector3.Distance(start.GetCenter(), target);
        openSet.Add(fScore[startID] + startID * 1e-6f, start);
        openCosts[startID] = fScore[startID];

        while (openSet.Count > 0)
        {
            var current = openSet.Values[0];
            openSet.RemoveAt(0);
            int currentID = current.GetID();

            if (closedSet.Contains(currentID))
                continue;

            closedSet.Add(currentID);
            expanded++;

            if (currentID == destination.GetID())
            {
                List<Vector3> path = new List<Vector3>();
                int nodeId = currentID;

                while (cameFrom.ContainsKey(nodeId))
                {
                    var (prevNode, neighbor) = cameFrom[nodeId];
                    path.Add(neighbor.GetWall().midpoint);
                    nodeId = prevNode.GetID();
                }

                path.Reverse();
                path.Add(target);
                return (path, expanded);
            }

            foreach (var neighbor in current.GetNeighbors())
            {
                var neighborNode = neighbor.GetNode();
                int neighborID = neighborNode.GetID();

                if (closedSet.Contains(neighborID))
                    continue;

                float tentativeG = gScore[currentID] + Vector3.Distance(current.GetCenter(), neighborNode.GetCenter());

                if (!gScore.ContainsKey(neighborID) || tentativeG < gScore[neighborID])
                {
                    cameFrom[neighborID] = (current, neighbor);
                    gScore[neighborID] = tentativeG;
                    float h = Vector3.Distance(neighborNode.GetCenter(), target);
                    float totalCost = tentativeG + h;

                    if (!openCosts.ContainsKey(neighborID) || totalCost < openCosts[neighborID])
                    {
                        openSet.Add(totalCost + neighborID * 1e-6f, neighborNode);
                        openCosts[neighborID] = totalCost;
                    }
                }
            }
        }

        return (new List<Vector3>(), expanded);

    }

    public Graph graph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnTarget += PathFind;
        EventBus.OnSetGraph += SetGraph;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGraph(Graph g)
    {
        graph = g;
    }

    // entry point
    public void PathFind(Vector3 target)
    {
        if (graph == null) return;

        // find start and destination nodes in graph
        GraphNode start = null;
        GraphNode destination = null;
        foreach (var n in graph.all_nodes)
        {
            if (Util.PointInPolygon(transform.position, n.GetPolygon()))
            {
                start = n;
            }
            if (Util.PointInPolygon(target, n.GetPolygon()))
            {
                destination = n;
            }
        }
        if (destination != null)
        {
            // only find path if destination is inside graph
            EventBus.ShowTarget(target);
            (List<Vector3> path, int expanded) = PathFinder.AStar(start, destination, target);

            Debug.Log("found path of length " + path.Count + " expanded " + expanded + " nodes, out of: " + graph.all_nodes.Count);
            EventBus.SetPath(path);
        }
        

    }

    

 
}

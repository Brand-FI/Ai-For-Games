using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    GridBlock gridRef;

    void Awake()
    {
        gridRef = GetComponent<GridBlock>();
    }

    public List<Node>FindPath(Vector3 startPosInp, Vector3 targetPosInp)
    {
        Node startNode = gridRef.NodeFromWorldPoint(startPosInp);
        Node targetNode = gridRef.NodeFromWorldPoint(targetPosInp);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while(openList.Count > 0)
        {
            Node currentNode = openList[0];
            for(int i = 1; i < openList.Count; i++)
            {
                if(openList[i].TotalCost <= currentNode.TotalCost)
                {
                    if(openList[i].heuristicCost < currentNode.heuristicCost)currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if(currentNode == targetNode)
            {
                return GetFinalPath(startNode, targetNode);
               
            }

            foreach(Node neighborNode in gridRef.GetNeighboringNodes(currentNode))
            {
                if(neighborNode.isWall || closedList.Contains(neighborNode))
                {
                    continue;
                }

                int costToNeighbor = currentNode.moveCost + GetManhattanDistance(currentNode, neighborNode);

                if(costToNeighbor < neighborNode.moveCost || !openList.Contains(neighborNode))
                {
                    neighborNode.moveCost = costToNeighbor;
                    neighborNode.heuristicCost = GetManhattanDistance(neighborNode, targetNode);
                    neighborNode.ParentNode = currentNode;

                    if(!openList.Contains(neighborNode))
                        openList.Add(neighborNode);
                }
            }
        }
        return null;
    }

    List<Node> GetFinalPath(Node startNodeInp, Node endNodeInp)
    {
        List<Node> finalPath = new List<Node>();
        Node currentNode = endNodeInp;

        while(currentNode != startNodeInp)
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }

        finalPath.Reverse();
        return finalPath;
    }

    int GetManhattanDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return distanceX + distanceY;
    }
}

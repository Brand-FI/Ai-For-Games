using UnityEngine;

public class Node
{
    public int gridX;
    public int gridY;

    public bool isWall;

    public Vector3 worldPos;

    public Node ParentNode;

    public int moveCost;
    public int heuristicCost;

    public int TotalCost
    {
        get
        {
            return moveCost + heuristicCost;
        }
    }

    public Node(bool IsWallInp, Vector3 worldPosInp, int gridXInp, int gridYInp)
    {
        isWall = IsWallInp;
        worldPos = worldPosInp;
        gridX = gridXInp;
        gridY = gridYInp;
    }
}

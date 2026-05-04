using UnityEngine;
using System.Collections.Generic;

public class Gnome : MonoBehaviour
{
    public GridBlock grid;
    public float speed = 2f;
    public float reachDistance = 0.5f;

    private Node currentNode;
    private Node targetNode;
    private Node lastNode;

    void Start()
    {
        currentNode = grid.NodeFromWorldPoint(transform.position);
        PickNextNode();
    }

    void Update()
    {
        if (targetNode == null) return;

        MoveToTarget();

        if (ReachedTarget())
        {
            lastNode = currentNode;
            currentNode = targetNode;
            PickNextNode();
        }
    }
    void PickNextNode()
    {
        var neighbors = grid.GetNeighboringNodes(currentNode);

        if (neighbors == null || neighbors.Count == 0)
            return;

        List<Node> validNodes = new List<Node>();

        foreach (var n in neighbors)
        {
            if (n != lastNode)
                validNodes.Add(n);
        }

        if (validNodes.Count == 0)
            validNodes = neighbors;

        targetNode = validNodes[Random.Range(0, validNodes.Count)];
    }
    void MoveToTarget()
    {
        Vector3 dir = targetNode.worldPos - transform.position;
        dir.y = 0;

        transform.position += dir.normalized * speed * Time.deltaTime;
    }

    bool ReachedTarget()
    {
        Vector3 dir = targetNode.worldPos - transform.position;
        dir.y = 0;

        return dir.magnitude < reachDistance;
    }
}
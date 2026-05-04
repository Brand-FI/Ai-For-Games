using UnityEngine;
using System.Collections.Generic;

public class GnomeTitik3 : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float maxSpeed = 3f;
    public float satisfactionRadius = 0.5f;
    public float slowRadius = 0.5f;
    public float rotationSpeed = 5f;

    public float detectionRadius = 6f;
    public float retargetInterval = 1.5f;

    public GameObject player;
    public GameObject hunter;
    public GameObject gridObject;

    private Rigidbody rb;
    private GridBlock gridRef;
    private Pathfinding pathfinder;

    private Vector3 moveVector = Vector3.zero;
    private bool isMoving = false;

    private enum State { Wander, Flee, SeekHunter }
    private State currentState = State.Wander;
    private bool isDeciding = false;

    private Vector3 currentTarget = Vector3.zero;
    private Vector3 fleeTarget = Vector3.zero;
    private float retargetTimer = 0f;

    private List<Node> blockedNodes = new List<Node>();

    List<Node> path;
    int currentIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gridRef = gridObject.GetComponent<GridBlock>();
        pathfinder = gridObject.GetComponent<Pathfinding>();
        SetTarget(GetWanderNode());
    }

    void FixedUpdate()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (currentState != State.Wander)
        {
            UnblockNodes();
            BlockPlayerRadius(5f);
        }

        HandleStateTransition(distToPlayer);
        HandleSeekHunterRetarget();

        switch (currentState)
        {
            case State.Wander: UpdateWander(); break;
            case State.Flee: UpdateFlee(); break;
            case State.SeekHunter: UpdateSeekHunter(); break;
        }
        Debug.Log("State: " + currentState +
          " | finalPath: " + (path != null ? path.Count : -1) +
          " | moveSpeed: " + moveSpeed +
          " | moveVector: " + moveVector +
          " | isMoving: " + isMoving +
          " | distToPlayer: " + Vector3.Distance(transform.position, player.transform.position));
        rb.linearVelocity = moveVector * moveSpeed;
    }

    void HandleStateTransition(float distToPlayer)
    {
        if (distToPlayer > detectionRadius || isDeciding) return;

        isDeciding = true;

        if (Random.value < 0.5f)
        {
            currentState = State.Flee;
            fleeTarget = GetFleeNode();
            SetTarget(fleeTarget);
        }
        else
        {
            currentState = State.SeekHunter;
            retargetTimer = 0f;
            SetTarget(hunter.transform.position);
        }
    }

    void HandleSeekHunterRetarget()
    {
        if (currentState != State.SeekHunter) return;

        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer > 0f) return;

        retargetTimer = retargetInterval;
        if (Vector3.Distance(hunter.transform.position, currentTarget) > 2f)
            SetTarget(hunter.transform.position);
    }

    void TransitionToWander()
    {
        UnblockNodes();
        isDeciding = false;
        currentState = State.Wander;
        currentTarget = Vector3.zero;
        SetTarget(GetWanderNode());
    }


    void UpdateWander()
    {
        MoveAlongPath();
    }

    void UpdateFlee()
    {
        MoveAlongPathFlee();

        bool nearTarget = Vector3.Distance(transform.position, fleeTarget) < satisfactionRadius * 2f;
        bool pathEmpty = path == null || path.Count == 0;

        if (nearTarget || pathEmpty)
            TransitionToWander();
    }

    void UpdateSeekHunter()
    {
        MoveAlongPath();

        bool nearHunter = Vector3.Distance(transform.position, hunter.transform.position) < satisfactionRadius * 3f;
        if (nearHunter)
            TransitionToWander();
    }


    void MoveAlongPath()
    {
        if (path == null || path.Count == 0 || currentIndex >= path.Count)
        {
            moveVector = Vector3.zero;
            moveSpeed = 0f;

            if (currentState == State.Wander)
            {
                currentTarget = Vector3.zero;
                SetTarget(GetWanderNode());
            }
            return;
        }

        Vector3 targetNodePos = path[currentIndex].worldPos;
        targetNodePos.y = transform.position.y;

        float distToFirst = Vector3.Distance(transform.position, targetNodePos);

        if (distToFirst < satisfactionRadius)
        {
            currentIndex++;
            return;
        }

        isMoving = true;
        SteeringSeek(targetNodePos);
    }

    void SteeringSeek(Vector3 seekTarget)
    {
        Vector3 direction = seekTarget - transform.position;
        direction.y = 0;

        if (isMoving && direction.sqrMagnitude > 0.01f)
        {
            if (path.Count > 1)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,
                                                       rotationSpeed * Time.deltaTime);
            }
        }

        if (direction.magnitude < satisfactionRadius)
        {
            isMoving = false;
            moveSpeed = 0f;
        }
        else if (direction.magnitude < slowRadius)
        {
            moveSpeed = maxSpeed * direction.magnitude / slowRadius;
        }
        else
        {
            moveSpeed = maxSpeed;
        }

        moveVector = direction.normalized;
    }

    void MoveAlongPathFlee()
    {
        if (path == null || path.Count == 0)
        {
            moveVector = Vector3.zero;
            moveSpeed = 0f;
            TransitionToWander(); // langsung wander kalau path habis
            return;
        }

        Vector3 targetNodePos = path[0].worldPos;
        targetNodePos.y = transform.position.y;

        float distToFirst = Vector3.Distance(transform.position, targetNodePos);

        if (distToFirst < satisfactionRadius && path.Count > 1)
        {
            targetNodePos = path[1].worldPos;
            targetNodePos.y = transform.position.y;
        }
        else if (distToFirst < satisfactionRadius && path.Count == 1)
        {
            moveVector = Vector3.zero;
            moveSpeed = 0f;
            TransitionToWander();
            return;
        }

        isMoving = true;
        SteeringFlee(targetNodePos);
    }

    void SteeringFlee(Vector3 fleeNodeTarget)
    {
        Vector3 dirToNode = fleeNodeTarget - transform.position;
        dirToNode.y = 0;
        if (isMoving && dirToNode.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirToNode);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation,
                                                   rotationSpeed * Time.deltaTime);
        }

        float distToNode = Vector3.Distance(transform.position, fleeNodeTarget);

        if (distToNode < satisfactionRadius)
        {
            isMoving = false;
            moveSpeed = 0f;
        }
        else if (distToNode < slowRadius)
        {
            moveSpeed = maxSpeed * distToNode / slowRadius;
        }
        else
        {
            moveSpeed = maxSpeed;
        }
        moveVector = dirToNode.normalized;
    }

    void SetTarget(Vector3 targetPos)
    {
        if (Vector3.Distance(targetPos, currentTarget) < 0.5f) return;

        currentTarget = targetPos;

        path = pathfinder.FindPath(transform.position, targetPos);
        currentIndex = 0;
    }

    Vector3 GetFleeNode()
    {
        Vector3 bestPos = transform.position;
        float bestDist = 0f;
        float halfX = gridRef.gridWorldSize.x / 2;
        float halfY = gridRef.gridWorldSize.y / 2;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = new Vector3(
                gridObject.transform.position.x + Random.Range(-halfX, halfX),
                transform.position.y,
                gridObject.transform.position.z + Random.Range(-halfY, halfY)
            );
            Node node = gridRef.NodeFromWorldPoint(randomPos);
            if (node.isWall) continue;

            float distToPlayer = Vector3.Distance(node.worldPos, player.transform.position);
            if (distToPlayer > bestDist)
            {
                bestDist = distToPlayer;
                bestPos = node.worldPos;
            }
        }
        return bestPos;
    }

    Vector3 GetWanderNode()
    {
        float halfX = gridRef.gridWorldSize.x / 2;
        float halfY = gridRef.gridWorldSize.y / 2;

        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPos = new Vector3(
                gridObject.transform.position.x + Random.Range(-halfX, halfX),
                transform.position.y,
                gridObject.transform.position.z + Random.Range(-halfY, halfY)
            );
            Node node = gridRef.NodeFromWorldPoint(randomPos);
            if (!node.isWall) return node.worldPos;
        }
        return transform.position;
    }

    void BlockPlayerRadius(float radius)
    {
        int steps = Mathf.RoundToInt(radius / gridRef.nodeRadius * 2);
        for (int x = -steps; x <= steps; x++)
        {
            for (int y = -steps; y <= steps; y++)
            {
                Vector3 checkPos = player.transform.position +
                                   new Vector3(x * gridRef.nodeRadius, 0, y * gridRef.nodeRadius);
                Node node = gridRef.NodeFromWorldPoint(checkPos);
                if (!node.isWall && Vector3.Distance(node.worldPos, player.transform.position) < radius)
                {
                    node.isWall = true;
                    blockedNodes.Add(node);
                }
            }
        }
    }

    void UnblockNodes()
    {
        foreach (Node node in blockedNodes)
            node.isWall = false;
        blockedNodes.Clear();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("PLAYER MENANG!");
            gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.black;

            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawSphere(path[i].worldPos, 0.2f);

                if (i < path.Count - 1)
                {
                    Gizmos.DrawLine(
                        path[i].worldPos,
                        path[i + 1].worldPos
                    );
                }
            }

            if (currentIndex < path.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(path[currentIndex].worldPos, 0.35f);
            }
        }

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(player.transform.position, 0.4f);
        }

        if (hunter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hunter.transform.position, 0.4f);
            Gizmos.DrawLine(transform.position, hunter.transform.position);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}
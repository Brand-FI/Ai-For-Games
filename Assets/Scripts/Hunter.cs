using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    public float moveSpeed = 5;
    Vector3 moveVector = Vector3.zero;
    Rigidbody rb;
    public GameObject player;
    public GameObject target;
    public float satisfactionRadius = 0.5f;
    public float slowRadius = 0.5f;
    public float maxSpeed = 5f;
    public float overShot = 1.1f;
    public bool isRunning = false;
    public float rotationSpeed = 5.0f;
    public LayerMask obstacleLayer;
    public GameObject gridObject;
    private GridBlock gridRef;
    private Pathfinding pathfinder;
    private Animator animator;

    //untuk simpan path, karena skrng di masing ai simpan path
    List<Node> path;
    int currentIndex = 0;
    float repathTimer = 0f;
    public float repathInterval = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gridRef = gridObject.GetComponent<GridBlock>();
        pathfinder = gridObject.GetComponent<Pathfinding>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            UpdatePath();
            repathTimer = repathInterval;
        }


        if (path != null && path.Count > 0) //Make sure there is a path of nodes, and make sure not to break over index
        {
            Vector3 targetNodePos = path[currentIndex].worldPos;

            if (path.Count > 2) targetNodePos *= overShot;//Target closest node and then add Overshot as a "He's running. He's chasing. He nearly ran to a wall."
            isRunning = true;
            if (path.Count > 1)
            {
                if (currentIndex + 1 < path.Count)
                {
                    Vector3 nextNodePos = path[currentIndex + 1].worldPos;
                    Vector3 dirToNext = nextNodePos - transform.position;
                    float distToCurrent = Vector3.Distance(transform.position, targetNodePos);

                    if (distToCurrent < satisfactionRadius)
                    {
                        currentIndex++;
                        if (currentIndex >= path.Count)
                        {
                            path = null;
                            currentIndex = 0;
                            return;
                        }
                        targetNodePos = path[currentIndex].worldPos;
                    }
                }
            }
            SteeringSeek(targetNodePos);
        }

        animator.SetBool("isRunning", isRunning);
        rb.linearVelocity = moveVector * moveSpeed;
    }

    void SteeringSeek(Vector3 seekTarget)
    {
        Vector3 direction = seekTarget - transform.position;
        direction.y = 0;

        if(isRunning && direction.sqrMagnitude > 0.01f)
        {
            if(path.Count > 1)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(lookRotation, transform.rotation, rotationSpeed * Time.deltaTime);
            }
        }

        if(direction.magnitude < satisfactionRadius)
        {
            isRunning = false;
        }
        else if(direction.magnitude < slowRadius)
        {
            moveSpeed = maxSpeed * direction.magnitude/slowRadius;
        }
        else
        {
            moveSpeed = maxSpeed;
        }

        moveVector = direction.normalized;
    }

    void UpdatePath()
    {
        path = pathfinder.FindPath(transform.position, player.transform.position);
        currentIndex = 0;
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
        }

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(player.transform.position, 0.4f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}
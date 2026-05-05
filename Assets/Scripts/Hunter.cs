using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour
{
    public float moveSpeed = 5;
    Vector3 moveVector = Vector3.zero;
    Rigidbody rb;
    public GameObject player;
    public Transform sightPoint;
    public float satisfactionRadius = 0.5f;
    public float slowRadius = 1f;
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
    public int currentIndex = 0;
    public float repathTimer = 0f;
    public float repathInterval = 1f;

    public bool inSight = false;
    public float sightTimer = 0f;
    public float sightInterval = 0.2f;
    public float outOfSight = 0f;
    public float outOfSightMax = 5f;

    public string currentTarget = "";

    //3 Raycast
    public float sightRange = 10f;
    public float sightAngle = 15f;


    public float stopDistance = 0.5f;
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
        sightTimer -= Time.deltaTime;
        if (sightTimer <= 0f)
        {
            Sight();
            sightTimer = sightInterval;
        }

        if(repathTimer > 0f)repathTimer -= Time.deltaTime;

        if(!inSight && outOfSight > 0f)outOfSight -= Time.deltaTime;

        if(outOfSight <= 0f)
        {
            if(repathTimer <= 0f && currentTarget == "Player")
            {
                currentTarget = "Lost";
                repathTimer = repathInterval;
                UpdatePath();
                Debug.Log("Lost sight of the target, wandering.");
            }
        }

        if (currentTarget == "Player")//jika liat target dan sudah dekat maka berhenti dan win
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distToPlayer < stopDistance)
            {
                isRunning = false;
                moveVector = Vector3.zero;
                animator.SetBool("isRunning", isRunning);
                rb.linearVelocity = Vector3.zero;
                GameHelper.Instance.showPanelLose();
                return;
            }
        }

        if (path != null && path.Count > 0) //Make sure there is a path of nodes, and make sure not to break over index
        {
            Vector3 targetNodePos = path[currentIndex].worldPos;

            //if (path.Count - currentIndex > 1) targetNodePos *= overShot;//Target closest node and then add Overshot as a "He's running. He's chasing. He nearly ran to a wall."
            isRunning = true;

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
                    //Debug.Log(path.Count);
                    targetNodePos = path[currentIndex].worldPos;
                }
            }
            else
            {
                if(currentTarget != "Player")currentTarget = "Reached";
                UpdatePath();
            }
            
            SteeringSeek(targetNodePos);
        }
        else UpdatePath();

        animator.SetBool("isRunning", isRunning);
        rb.linearVelocity = moveVector * moveSpeed;
    }

    void Sight()
    {
        Vector3[] directions = new Vector3[]
        {
        sightPoint.forward,
        Quaternion.Euler(0, -sightAngle, 0) * sightPoint.forward,
        Quaternion.Euler(0,  sightAngle, 0) * sightPoint.forward,
        };

        bool hit = false;
        foreach (Vector3 dir in directions)
        {
            RaycastHit hitInfo;
            hit = Physics.Raycast(sightPoint.position, dir, out hitInfo, sightRange);
            if (hit && hitInfo.collider != null && hitInfo.collider.CompareTag("Player"))
            {
                currentTarget = "Player";
                UpdatePath();
                inSight = true;
                outOfSight = outOfSightMax;
                Debug.Log("Target in sight, following.");
                return;
            }
        }

        inSight = false;
    }

    void SteeringSeek(Vector3 seekTarget)
    {
        if (path == null) return;

        Vector3 direction = seekTarget - transform.position;
        Vector3 playerDirection = player.transform.position - transform.position;

        direction.y = 0;

        if(isRunning && direction.sqrMagnitude > 0.01f)
        {
            if(path.Count > 1)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(lookRotation, transform.rotation, rotationSpeed * Time.deltaTime);
                //coba transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        else if(direction.magnitude < satisfactionRadius)
        {
            isRunning = false;
            repathTimer = repathInterval;
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
        if(currentTarget == "Player")
        {
            path = pathfinder.FindPath(transform.position, player.transform.position);
        }
        else
        {
            Node randomNode = gridRef.GetRandomNode();
            path = pathfinder.FindPath(transform.position, randomNode.worldPos);
        }

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
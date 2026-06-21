using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HunterSensor : MonoBehaviour
{
    public float moveSpeed = 5;
    Vector3 moveVector = Vector3.zero;
    Rigidbody rb;
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

    public float outOfSight = 0f;
    public float outOfSightMax = 5f;

    public string currentTarget = "";

    //Sensor
    public Sensor sensor;
    private Vector3 lastSeenPosition;

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

        if(repathTimer > 0f)repathTimer -= Time.deltaTime;

        if (sensor.targetSensed && sensor.targetGO != null)
        {
            currentTarget = "Player";
            outOfSight = outOfSightMax;
            lastSeenPosition = sensor.targetGO.position;
            Debug.Log("Sensed = " + sensor.targetSensed + "Player in Cone");
            UpdatePath();
            

        }
        else
        {
            outOfSight -= Time.deltaTime;
        }

        if (outOfSight <= 0f)
        {
            if(repathTimer <= 0f && currentTarget == "Player")
            {
                currentTarget = "Lost";
                repathTimer = repathInterval;
                UpdatePath();
                Debug.Log("Sensed = " + sensor.targetSensed + "Player Not Detected");
            }
        }

        if (currentTarget == "Player" && sensor.targetGO != null)//jika liat target dan sudah dekat maka berhenti dan win
        {
            float distToPlayer = Vector3.Distance(transform.position, sensor.targetGO.position);
            Debug.Log(distToPlayer);
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
                if(currentTarget != "Player")
                {
                    currentTarget = "Reached";
                    UpdatePath();
                }
            }
            
            SteeringSeek(targetNodePos);
        }
        else UpdatePath();

        animator.SetBool("isRunning", isRunning);
        rb.linearVelocity = moveVector * moveSpeed;
    }

    void SteeringSeek(Vector3 seekTarget)
    {
        if (path == null) return;

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
            path = pathfinder.FindPath(transform.position, lastSeenPosition);
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

        if (sensor.targetGO != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(sensor.targetGO.position, 0.4f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}
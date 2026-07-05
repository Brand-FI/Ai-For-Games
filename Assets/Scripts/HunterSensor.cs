using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HunterSensor : MonoBehaviour
{
    public float moveSpeed = 5;
    Vector3 moveVector = Vector3.zero;
    Rigidbody rb;
    public float satisfactionRadius = 0.5f;
    public float stopRadius = 0.5f;
    public float slowRadius = 1f;
    public float maxSpeed = 5f;
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

    //Sensor
    public Sensor sensor;
    private Vector3 lastSeenPosition;

    //Look Around

    public bool isWaiting = false;
    float waitTimer = 0f;

    public float lookAngle = 45f;
    public float lookInterval = 1f;

    public float outOfSightMax = 2f;
    public float investigateSoundMax = 2f;

    bool hasLastSeenPosition;
    bool hasLastHearPosition;
    private Quaternion startRotation;

    bool waitingLastPosition = false;
    bool waitingSound = false;

    float lookTimer = 0f;
    bool lookRight = true;
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
        if (repathTimer > 0)
        {
            repathTimer -= Time.deltaTime;
        }
        MakeDecision();
        animator.SetBool("isRunning", isRunning);
        rb.linearVelocity = moveVector * moveSpeed;
    }
    void MakeDecision()
    {
        if (sensor.targetSensed && sensor.targetGO != null)
        {
            hasLastSeenPosition = true;
            lastSeenPosition = sensor.targetGO.position;
            isWaiting = false;
            waitingLastPosition = false;
            hasLastHearPosition = false;
            ChasePlayer();
        }
        else if (sensor.soundSensed || hasLastHearPosition)
        {
            hasLastHearPosition = true;
            hasLastSeenPosition = false;
            ChaseSound();
        }
        else if (hasLastSeenPosition)
        {
            ChaseLastPosition();
        }
        else if (isWaiting)
        {
            Wait();
        }
        else if (path == null || path.Count == 0)
        {
            Wander();
        }
        else
        {
            FollowPath();
        }
    }
    void ChasePlayer()
    {
        Debug.Log("Chase Player");
        lastSeenPosition = sensor.targetGO.position;
        if (repathTimer <= 0f)
        {
            path = pathfinder.FindPath(transform.position, lastSeenPosition);
            currentIndex = 0;
            repathTimer = repathInterval;
        }
        FollowPath();
        float dist = Vector3.Distance(transform.position, sensor.targetGO.position);
        if (dist < stopRadius)
        {
            GameHelper.Instance.showPanelLose();
        }
    }
    void ChaseLastPosition()
    {
        Debug.Log("Chase Last Pos");
        if (path == null || path.Count == 0)
        {
            waitingLastPosition = true;
            path = pathfinder.FindPath(transform.position, lastSeenPosition);
            currentIndex = 0;
        }
        FollowPath();
    }
    void ChaseSound()
    {
        Debug.Log("Chase Sound");
        path = null;
        if(path == null)
        {
            waitingSound = true;
            Node soundNode = gridRef.GetNearestNode(sensor.soundPosition);
            path = pathfinder.FindPath(transform.position, soundNode.worldPos);
            currentIndex = 0;
            FollowPath();
        }

    }
    void Wander()
    {
        if (path == null)
        {
            Debug.Log("Wandering");
            Node random = gridRef.GetRandomNode();
            path = pathfinder.FindPath(transform.position, random.worldPos);
            currentIndex = 0;
        }
        FollowPath();
    }

    void FollowPath()
    {
        if (path == null || path.Count == 0)
        {
            return;
        }
        Vector3 targetNodePos = path[currentIndex].worldPos;
        isRunning = true;

        float distToCurrent = Vector3.Distance(transform.position, targetNodePos);
        if (distToCurrent < satisfactionRadius)
        {
            if (currentIndex + 1 < path.Count)
            {
                currentIndex++;
                targetNodePos = path[currentIndex].worldPos;
            }
            else
            {
                path = null;
                currentIndex = 0;
                if (waitingLastPosition)
                {
                    hasLastSeenPosition = false;
                    isWaiting = true;
                    waitTimer = outOfSightMax;
                    startRotation = transform.rotation;
                    waitingLastPosition = false;
                }
                else if (waitingSound)
                {
                    hasLastHearPosition = false;
                    isWaiting = true;
                    waitTimer = investigateSoundMax;
                    startRotation = transform.rotation;
                    waitingSound = false;
                }
                return;
            }
        }
        SteeringSeek(targetNodePos);
    }
    void SteeringSeek(Vector3 seekTarget)
    {
        Vector3 direction = seekTarget - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance < satisfactionRadius)
        {
            isRunning = false;
            moveVector = Vector3.zero;
            moveSpeed = 0;
            return;
        }
        if (distance < slowRadius)
        {
            moveSpeed = maxSpeed * distance / slowRadius;
        }
        else
        {
            moveSpeed = maxSpeed;
        }
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(lookRotation, transform.rotation, rotationSpeed * Time.deltaTime);
        isRunning = true;
        moveVector = direction.normalized;
    }
    void Wait()
    {
        isRunning = false;
        moveVector = Vector3.zero;
        LookAround();
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            isWaiting = false;
        }
    }
    void LookAround()
    {
        lookTimer += Time.deltaTime;
        float angle = lookAngle;
        if (lookRight)
        {
            angle = lookAngle;
        }
        else
        {
            angle = -lookAngle;
        }
        Vector3 baseForward = startRotation * Vector3.forward;
        Vector3 direction = Quaternion.Euler(0, angle, 0) * baseForward;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation,lookRotation, 3f * Time.deltaTime);

        if (lookTimer >= lookInterval)
        {
            lookRight = !lookRight;
            lookTimer = 0f;
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
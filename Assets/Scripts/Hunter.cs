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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gridRef = gridObject.GetComponent<GridBlock>();
        pathfinder = gridObject.GetComponent<Pathfinding>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (gridRef.finalPath != null && gridRef.finalPath.Count > 0) //Make sure there is a path of nodes, and make sure not to break over index
        {
            Vector3 targetNodePos = gridRef.finalPath[0].worldPos; 
            if(gridRef.finalPath.Count > 2) targetNodePos *= overShot;//Target closest node and then add Overshot as a "He's running. He's chasing. He nearly ran to a wall."
            isRunning = true;
            if (gridRef.finalPath.Count > 1)
            {
                Vector3 nextNodePos = gridRef.finalPath[1].worldPos;
                Vector3 dirToNext = nextNodePos - transform.position;
                float distToCurrent = Vector3.Distance(transform.position, targetNodePos);
                if (distToCurrent < satisfactionRadius)
                {
                    targetNodePos = nextNodePos;
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
            if(gridRef.finalPath.Count > 1)
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
}
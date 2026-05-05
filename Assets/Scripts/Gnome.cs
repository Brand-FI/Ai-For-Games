using System.Collections.Generic;
using UnityEngine;

public class Gnome : MonoBehaviour
{
    enum GnomeState
    {
        Wander,
        Flee,
        Seek

    }
    GnomeState currentState;

    public float speed = 3f;
    public float reachDistance = 0.3f;

    public GameObject gridObject;
    public float rotationSpeed = 5f;

    Pathfinding pathfinder;
    GridBlock grid;

    List<Node> path;
    int currentIndex = 0;

    public float detectionRadius = 5f;
    public Transform player;
    public Transform hunter;

    bool wasInRange = false;

    public GameObject key;//buat ref key
    public GameObject panelWin; 

  
    void Start()
    {
        grid = gridObject.GetComponent<GridBlock>();
        pathfinder = gridObject.GetComponent<Pathfinding>();
        currentState = GnomeState.Wander;//awal muncul langsung wander
        PickNewDestination();
    }

    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        bool isInRange = distToPlayer < detectionRadius;

        //jika masuk range musuh maka DecideAction
        if (isInRange && !wasInRange)
        {
            Debug.Log("Masuk radius player");
            DecideAction();
        }
        if (!isInRange && wasInRange)//jika sudah keluar langsung wander
        {
            Debug.Log("Keluar dari radius player");
            currentState = GnomeState.Wander;
            PickNewDestination();
        }
        wasInRange = isInRange;
        Wandering();
    }

    void Wandering()
    {
        if (path == null || path.Count == 0) return;

        Vector3 targetPos = path[currentIndex].worldPos;

        Vector3 direction = targetPos - transform.position;
        direction.y = 0;

        if (direction.magnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < reachDistance)
        {
            currentIndex++;
            //sudah sampai tujuan akhir
            if (currentIndex >= path.Count)
            {
                currentIndex = path.Count - 1;

                if (currentState == GnomeState.Wander)
                {
                    PickNewDestination();
                }
                else if (currentState == GnomeState.Flee)
                {
                    SetFleePath();
                }
                else if (currentState == GnomeState.Seek)
                {
                    SetSeekPath();
                }
            }
        }
    }

    void PickNewDestination()
    {
        Node randomNode = grid.GetRandomNode();

        path = pathfinder.FindPath(transform.position, randomNode.worldPos);

        if (path == null || path.Count == 0)
        {
            PickNewDestination();
            return;
        }

        currentIndex = 0;
    }

    void DecideAction()
    {
        if (Random.Range(0, 2) == 0)
        {
            currentState = GnomeState.Flee;
            SetFleePath();
            Debug.Log("Flee");
        }
        else
        {
            float dist = Vector3.Distance(hunter.position, player.position);
            if (dist < detectionRadius)
            {
                currentState = GnomeState.Flee;
                SetFleePath();
                Debug.Log("Flee karena hunter dekat player");
            }
            else
            {
                currentState = GnomeState.Seek;
                SetSeekPath();
                Debug.Log("Seek hunter");
            }
        }
    }
    void SetFleePath()
    {
        Vector3 dirAway = (transform.position - player.position).normalized;
        Vector3 target = transform.position + dirAway * 5f;

        path = pathfinder.FindPath(transform.position, target);

        if (path == null || path.Count == 0)
        {
            PickNewDestination();
            return;
        }

        currentIndex = 0;
    }
    void SetSeekPath()
    {
        path = pathfinder.FindPath(transform.position, hunter.position);

        if (path == null || path.Count == 0)
        {
            PickNewDestination();
            return;
        }

        currentIndex = 0;
    }
    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.green;

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
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.3f);

        //radius sekitar
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(key);
            Debug.Log("Player Wins");
            GameHelper.Instance.showPanelWin();
        }
    }
}
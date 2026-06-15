using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
    public Transform target;
    NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine("SeekTarget");
    }

    private IEnumerator SeekTarget()
    {
        float interval = 2f;
        agent.destination = target.position;
        yield return new WaitForSeconds(interval);
    }
}

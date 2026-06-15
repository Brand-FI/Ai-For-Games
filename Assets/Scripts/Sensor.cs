using System.Collections;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    public Transform targetGO;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool targetSensed;

    void Start()
    {
        StartCoroutine(SensoryRoutine());
    }

    private IEnumerator SensoryRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;
            SensorCheck();
        }
    }

    public void SensorCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            targetGO = rangeChecks[0].transform;
            Vector3 directionToTarget = (targetGO.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetGO.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    targetSensed = true;
                }
                else
                {
                    targetSensed = false;
                }

            }
            else
            {
                targetSensed = false;
            }
        }
        else if (targetSensed)
        {
            targetSensed = false;
        }
    }
}

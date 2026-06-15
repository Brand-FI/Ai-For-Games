using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sensor))]
public class SensorEditor : Editor
{
    private void OnSceneGUI()
    {
        Sensor sense = (Sensor)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(sense.transform.position, Vector3.up, Vector3.forward, 360, sense.radius);

        Vector3 viewAngle1 = DirectionFromAngle(sense.transform.eulerAngles.y, -sense.angle / 2);
        Vector3 viewAngle2 = DirectionFromAngle(sense.transform.eulerAngles.y, sense.angle / 2);
        
        Handles.color = Color.green;
        Handles.DrawLine(sense.transform.position, sense.transform.position + viewAngle1 * sense.radius);
        Handles.DrawLine(sense.transform.position, sense.transform.position + viewAngle2 * sense.radius);

        if(sense.targetSensed)
        {
            Handles.color = Color.red;
            Handles.DrawLine(sense.transform.position, sense.targetGO.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f); // Ensure camera stays back

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;
    }
}


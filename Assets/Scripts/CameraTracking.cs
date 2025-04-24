using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    public Transform target;        // The player to follow
    public float smoothSpeed = 0.125f;
    public Vector3 offset;          // Offset from the player

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

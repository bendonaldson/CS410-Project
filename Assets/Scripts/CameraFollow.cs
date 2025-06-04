using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float zOffset = 6; // Ensure camera stays back

    public float smoothSpeed = 0.125f; // How smoothly the camera moves

    [Header("Vertical Offsets")]
    public float lockedGroundCameraY = 3f; // Camera's Y position when on ground (relative to player)
    public float playerFollowYOffset = 0f; // Camera's Y position when on platforms (relative to player)

    [Header("Ground Detection")]
    public float groundThresholdY = 0f; // The Y-coordinate below which the camera considers it "ground floor"

    private float currentCameraTargetY; // The target Y offset the camera is currently moving towards

    void LateUpdate()
    {
        if (target != null)
        {
            // Update the Y-offset
            UpdateCameraYOffset();

            // Create the final target position vector
            Vector3 targetPosition = new Vector3(target.position.x, currentCameraTargetY, target.position.z - zOffset);

            // Smoothly move the camera towards the target position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
    void UpdateCameraYOffset()
    {
        // Always follow the player’s Y directly:
        currentCameraTargetY = target.position.y + playerFollowYOffset;
    }
}
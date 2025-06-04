using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float zOffset = 6; // Ensure camera stays back

    public float smoothSpeed = 0.125f; // How smoothly the camera moves

    [Header("Vertical Offsets")]
    public static float onFloorYOffset = 3f; // Camera's Y position when on ground (relative to player)
    public float playerFollowYOffset = 0f; // Camera's Y position when on platforms (relative to player)

    // The Y-coordinate below which the camera considers it "ground floor"
    // This is updated in PlayerController on collision with floor.
    public static float floorThresholdY = 0f;
    public static float floorBottomY = 0f;
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
        // If player's Y is below or at the ground threshold, use ground offset
        if (floorBottomY < target.position.y || target.position.y <= floorThresholdY)
        {
            currentCameraTargetY = floorThresholdY;
        }
        // Otherwise, use platform offset
        else
        {
            currentCameraTargetY = target.position.y + playerFollowYOffset;
        }


        // Always follow the player's Y directly:
        currentCameraTargetY = target.position.y + playerFollowYOffset;
    }

    public static void UpdateFloorY(float amount)
    {
        floorThresholdY = amount + onFloorYOffset;
        floorBottomY = amount - 0.5f;
    }
}
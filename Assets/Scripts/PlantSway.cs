using UnityEngine;

public class PlantSway : MonoBehaviour
{
    // --- Public Variables (Adjust in Unity Inspector) ---
    [Header("Rustle Effect Settings")]
    public float maxRotationAngle = 30f; // Maximum angle of rotation (amplitude)
    public float initialFrequency = 10f; // How fast it shakes initially
    public float rustleDuration = 1.5f; // How long the rustle effect lasts (in seconds)
    public float dampingFactor = 2f; // Controls how quickly the effect slows down (higher = faster damping)

    // --- Private Variables ---
    private bool isRustling = false;
    private float rustleTimer = 0f;
    private Quaternion initialRotation; // To store the object's original rotation

    // --- Unity Lifecycle Methods ---

    void Start()
    {
        // Store the initial rotation of the object when the scene starts
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (isRustling)
        {
            // Update the timer
            rustleTimer += Time.deltaTime;

            // Calculate progress (0 to 1) of the rustle effect
            float progress = rustleTimer / rustleDuration;

            // --- Damping Logic ---
            // Decrease amplitude and frequency over time using an easing function
            // Example: Using an inverse exponential or a smooth step for damping
            float currentAmplitude = maxRotationAngle * Mathf.Exp(-dampingFactor * progress);
            float currentFrequency = initialFrequency * (1 - progress); // Linearly decrease frequency

            // Ensure frequency doesn't go below a small value to avoid division by zero or weird behavior
            currentFrequency = Mathf.Max(0.1f, currentFrequency);

            // --- Sine Wave Rotation ---
            // Calculate the sine wave value based on time and current frequency
            // Time.time is continuous, so it's good for continuous oscillation
            float sineValue = Mathf.Sin(Time.time * currentFrequency);

            // Apply the rotation
            // Rotate around the Z-axis for 2D objects
            float currentRotationZ = sineValue * currentAmplitude;
            transform.rotation = initialRotation * Quaternion.Euler(0, 0, currentRotationZ);

            // --- Stop Condition ---
            if (rustleTimer >= rustleDuration)
            {
                isRustling = false;
                rustleTimer = 0f;
                // Reset to initial rotation to ensure it stops cleanly
                transform.rotation = initialRotation;
            }
        }
    }

    // --- Trigger Event ---
    // This method is called when another 2D collider enters this object's trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        // Optional: Check if the 'other' collider is your player or specific object
        // if (other.CompareTag("Player") && !isRustling)
        // {
        // Start the rustle effect
        isRustling = true;
        rustleTimer = 0f; // Reset timer to start the effect from the beginning
        // }
    }
}

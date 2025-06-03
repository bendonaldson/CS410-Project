using UnityEngine;

// Require the Rigidbody2D component to ensure it exists
[RequireComponent(typeof(Rigidbody2D))]
public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 10f; // How far to move left/right from start
    public float speed = 2f;
    public float stoppingDistance = 0.1f; // How close to a point before turning

    [Header("Orientation")]
    // This property is now primarily for conceptual understanding.
    // The actual sprite orientation is controlled by 'flipSpriteOnDirectionChange'.
    public bool faceMovementDirection = true;
    public bool flipSpriteOnDirectionChange = true; // Common for 2D side-scrollers

    // Private Variables
    private Rigidbody2D _rb;
    private Vector2 _startPosition;
    private Vector2 _leftPatrolPosition;
    private Vector2 _rightPatrolPosition;
    private Vector2 _currentTargetPosition;
    private bool _movingRight; // To track direction for sprite flipping

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0; // No gravity affecting this enemy
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent physics from rotating it
    }

    void Start()
    {
        _startPosition = transform.position;
        _leftPatrolPosition = _startPosition - new Vector2(patrolDistance, 0);
        _rightPatrolPosition = _startPosition + new Vector2(patrolDistance, 0);

        // Start by moving towards the right patrol point
        _currentTargetPosition = _rightPatrolPosition;
        _movingRight = true;
        UpdateOrientation();
    }

    void FixedUpdate()
    {
        // Calculate direction and move
        Vector2 directionToTarget = (_currentTargetPosition - (Vector2)transform.position).normalized;
        _rb.linearVelocity = directionToTarget * speed;

        // Check if we've reached the current target point
        if (Vector2.Distance(transform.position, _currentTargetPosition) < stoppingDistance)
        {
            SwitchTarget();
        }
    }

    void SwitchTarget()
    {
        if (_currentTargetPosition == _rightPatrolPosition)
        {
            _currentTargetPosition = _leftPatrolPosition;
            _movingRight = false;
        }
        else
        {
            _currentTargetPosition = _rightPatrolPosition;
            _movingRight = true;
        }
        UpdateOrientation();
    }

    void UpdateOrientation()
    {
        // The previous 'faceMovementDirection' logic with Z-axis rotation was causing the sprite to flip upside down.
        // For typical 2D sprites, horizontal flipping is best achieved by scaling the X-axis.
        // This part of the code now solely relies on 'flipSpriteOnDirectionChange' for horizontal orientation.

        if (flipSpriteOnDirectionChange)
        {
            // Flip the sprite's local scale on the X-axis to change its facing direction.
            Vector3 localScale = transform.localScale;
            if (_movingRight)
            {
                // Ensure positive X scale to face right (assuming original sprite faces right)
                localScale.x = -Mathf.Abs(localScale.x);
            }
            else
            {
                // Set negative X scale to face left
                localScale.x = Mathf.Abs(localScale.x);
            }
            transform.localScale = localScale;
        }
    }

    void OnDrawGizmosSelected()
    {
        // To draw Gizmos correctly even before Start() is called in editor mode
        Vector2 currentStartPosition = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        Vector2 GizmoLeftPatrolPosition = currentStartPosition - new Vector2(patrolDistance, 0);
        Vector2 GizmoRightPatrolPosition = currentStartPosition + new Vector2(patrolDistance, 0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(GizmoLeftPatrolPosition, GizmoRightPatrolPosition);
        Gizmos.DrawWireSphere(GizmoLeftPatrolPosition, 0.3f);
        Gizmos.DrawWireSphere(GizmoRightPatrolPosition, 0.3f);

        if (Application.isPlaying) // Only draw current target if playing
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_currentTargetPosition, 0.4f);
        }
    }
}

using UnityEngine;

// Require the Rigidbody2D component to ensure it exists
[RequireComponent(typeof(Rigidbody2D))]
public class AiChaseRigidbody : MonoBehaviour // Renamed class for clarity
{
    // Public Variables
    public GameObject player;
    public float speed;
    public float activateDist;
    public float deactivateDist;

    public float idleSpeed;
    public float movementDuration;
    public float movementBoundaryX;
    public float movementBoundaryY;

    public float timeToRebound;
    public float reboundSpeed;
    public float timeToBounce;

    // Private Variables
    private Rigidbody2D _rb; // Reference to the Rigidbody2D component
    private Transform _playerTransform; // Cache player's transform for efficiency
    private Vector2 _targetDirection;
    private float _timeToChangeDirection;
    private Vector2 _movementBoundaryCenter; // Store the center point for boundaries
    private bool _bounded;
    private bool _seeking;
    //private bool _collided;
    private Vector2 _bounceDirection;
    private float _reboundTime;
    private float _collisionTimer;

    // Calculated Movement
    private Vector2 _nextMovement = Vector2.zero;
    private float _nextRotationAngle = 0f; // Store target rotation Z angle

    // Initialization
    void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject
        _rb = GetComponent<Rigidbody2D>();

        // Cache player transform
        if (player != null)
        {
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player GameObject is not assigned!", this);
            // Optional: Disable the script if player is 
            // this.enabled = false;
        }
    }

    void Start()
    {
        // Initialize idle movement
        _timeToChangeDirection = movementDuration;
        SetRandomDirection();
        _movementBoundaryCenter = _rb.position;
        _bounded = false; // Reset bounded state initially
        _seeking = false; // Reset seeking state
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate distance to player
        float distance = Vector2.Distance(_rb.position, _playerTransform.position);

        // State Switching Logic
        if (!_seeking && distance < activateDist)
        {
            _seeking = true;
            _bounded = false; // Exit bounded state when seeking starts
        }
        else if (_seeking && distance > deactivateDist)
        {
            _seeking = false;
            _bounded = false; // Reset bounded state when idle starts
            _timeToChangeDirection = 0f; // Force direction change immediately when starting idle
        }


        if (!_seeking) // Idle State
        {
            _reboundTime = 0f;
            if (!_bounded)
            {
                _movementBoundaryCenter = _rb.position;
                _bounded = true;
                _timeToChangeDirection = movementDuration;
                SetRandomDirection();
            }

            // Update idle timer
            _timeToChangeDirection -= Time.deltaTime;

            // Change direction if timer reaches zero
            if (_timeToChangeDirection <= 0f)
            {
                SetRandomDirection();
                _timeToChangeDirection = movementDuration;
            }
        }
    }

    // FixedUpdate is called for physics updates
    void FixedUpdate()
    {
        _nextMovement = Vector2.zero; // Reset movement for this physics step

        if (_seeking)
        {
            // Calculate movement towards player
            Vector2 directionToPlayer = ((Vector2)_playerTransform.position - _rb.position).normalized;
            Vector2 finalVelocity = directionToPlayer * speed;

            if (_reboundTime > 0f)
            {
                Vector2 reboundVelocity = _reboundTime * _bounceDirection * reboundSpeed; // Linear decay
                finalVelocity = finalVelocity + reboundVelocity;
                _rb.linearVelocity = finalVelocity;
                _reboundTime -= Time.fixedDeltaTime;
            }
            else
            {
                finalVelocity = directionToPlayer * speed;
                _rb.linearVelocity = finalVelocity;
            }

            Debug.DrawRay(_rb.position, finalVelocity, Color.red);

            // Calculate rotation
            _nextRotationAngle = (Mathf.Atan2(finalVelocity.y, finalVelocity.x) * Mathf.Rad2Deg) - 90;
        }
        else // Idle State
        {
            // Calculate target position
            Vector2 currentPosition = _rb.position;
            Vector2 targetPosition = currentPosition + _targetDirection * Time.fixedDeltaTime;

            // Apply boundary clamping if in idle
            if (_bounded)
            {
                float minX = _movementBoundaryCenter.x - movementBoundaryX;
                float maxX = _movementBoundaryCenter.x + movementBoundaryX;
                float minY = _movementBoundaryCenter.y - movementBoundaryY;
                float maxY = _movementBoundaryCenter.y + movementBoundaryY;

                if (currentPosition.x < minX ||
                    currentPosition.x > maxX ||
                    currentPosition.y < minY ||
                    currentPosition.y > maxY
                    )
                {
                    SetRandomDirection();
                }
            }
            _rb.MovePosition(targetPosition);
        }

        transform.rotation = Quaternion.Euler(0f, 0f, _nextRotationAngle);
    }

    // Collision Detection
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") && _seeking)
        {
            doBounce(collision);
        }
    }

    private void doBounce(Collision2D collision)
    {
        // Reflect the current target direction based on the collision normal
        ContactPoint2D contact = collision.GetContact(0);
        Vector3 directionToContact = contact.point - _rb.position;
        _bounceDirection = Vector2.Reflect(directionToContact, contact.normal).normalized;
        _reboundTime = timeToRebound;
    }

    // Helper Functions
    private void SetRandomDirection()
    {
        _targetDirection = Random.insideUnitCircle.normalized;
        _nextRotationAngle = (Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg) - 90;
    }
}
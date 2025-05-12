using UnityEngine;
using System.Collections;

// Require the Rigidbody2D component to ensure it exists
[RequireComponent(typeof(Rigidbody2D))]
public class AiChaseRigidbody : MonoBehaviour
{
    // Public Variables
    public GameObject player;

    [Header("Seeking")]
    public float speed;
    public float activateDist;
    public float deactivateDist;

    [Header("Idle")]
    public float idleSpeed;
    public float movementDuration;
    public float movementBoundaryX;
    public float movementBoundaryY;

    [Header("Bounce Parameters")]
    public float timeToRebound;

    // Private Variables
    private Rigidbody2D _rb;
    private Transform _playerTransform;
    private Vector2 _targetDirection;
    private float _timeToChangeDirection;
    private Vector2 _movementBoundaryCenter;
    private bool _bounded;
    private bool _seeking;
    private Vector2 _bounceDirection;
    private float _reboundTime;
    private float reboundSpeed;

    // Calculated Movement
    private float _nextRotationAngle = 0f;

    // Initialization
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(FindPlayerDelayed());

        // Initialize idle movement
        _timeToChangeDirection = movementDuration;
        SetRandomDirection();
        _movementBoundaryCenter = _rb.position;
        _bounded = false; // Reset bounded state initially
        _seeking = false; // Reset seeking state
        // set rebound speed as a funciton of normal speed
        reboundSpeed = speed * 1.3f;
    }

    IEnumerator FindPlayerDelayed()
    {
        yield return null; // Wait for one frame to ensure the player is in the scene

        // Find and cache player transform by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject;
            _playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("No GameObject with the 'Player' tag found (after delay)!", this);
            this.enabled = false;
            yield break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure player transform is still valid
        if (_playerTransform == null)
        {
            // Player might have been destroyed, or FindPlayerDelayed hasn't run yet
            return;
        }

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

    // FixedUpdate is called for physics updatesa
    void FixedUpdate()
    {
        if (_playerTransform == null) return; // Don't run AI if player isn't found yet

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
            Vector2 targetPosition = currentPosition + _targetDirection * Time.fixedDeltaTime * idleSpeed;

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
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 10f;
    public float speed = 2f;
    public float stoppingDistance = 0.1f;

    [Header("Orientation")]
    public bool faceMovementDirection = true;
    public bool flipSpriteOnDirectionChange = true;

    private Rigidbody2D _rb;
    private Vector2 _startPosition;
    private Vector2 _leftPatrolPosition;
    private Vector2 _rightPatrolPosition;
    private Vector2 _currentTargetPosition;
    private bool _movingRight;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        _startPosition = transform.position;
        _leftPatrolPosition = _startPosition - new Vector2(patrolDistance, 0);
        _rightPatrolPosition = _startPosition + new Vector2(patrolDistance, 0);

        _currentTargetPosition = _rightPatrolPosition;
        _movingRight = true;
        UpdateOrientation();
    }

    void FixedUpdate()
    {
        Vector2 directionToTarget = (_currentTargetPosition - (Vector2)transform.position).normalized;
        _rb.linearVelocity = directionToTarget * speed;

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
        if (flipSpriteOnDirectionChange)
        {
            Vector3 localScale = transform.localScale;
            if (_movingRight)
            {
                localScale.x = -Mathf.Abs(localScale.x);
            }
            else
            {
                localScale.x = Mathf.Abs(localScale.x);
            }
            transform.localScale = localScale;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 currentStartPosition = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        Vector2 GizmoLeftPatrolPosition = currentStartPosition - new Vector2(patrolDistance, 0);
        Vector2 GizmoRightPatrolPosition = currentStartPosition + new Vector2(patrolDistance, 0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(GizmoLeftPatrolPosition, GizmoRightPatrolPosition);
        Gizmos.DrawWireSphere(GizmoLeftPatrolPosition, 0.3f);
        Gizmos.DrawWireSphere(GizmoRightPatrolPosition, 0.3f);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_currentTargetPosition, 0.4f);
        }
    }
}

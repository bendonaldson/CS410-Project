using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("LAYERS")]
    [Tooltip("Set this to the layer your player is on")]
    public LayerMask PlayerLayer;

    [Header("INPUT")]
    [Tooltip("Set this to the move action input")]
    [SerializeField] private InputActionReference _moveAction;
    [Tooltip("Set this to the move action input")]
    [SerializeField] private InputActionReference _jumpAction;
    [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
    public bool SnapInput = true;
    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;
    [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Header("MOVEMENT")]
    [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;
    [Tooltip("The player's capacity to gain horizontal speed")]
    public float Acceleration = 120;
    [Tooltip("The pace at which the player comes to a stop")]
    public float GroundDeceleration = 60;
    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float AirDeceleration = 30;
    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;
    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;

    [Header("JUMP")]
    [Tooltip("The immediate velocity applied when jumping")]
    public float JumpPower = 36;
    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;
    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;
    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;
    [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public float CoyoteTime = .15f;
    [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
    public float JumpBuffer = .2f;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    public ResetScene reset;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;

    #region Animation Stuff
    Animator animator;
    bool isFacingRight = true;
    #endregion


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start()
    {
        reset = FindObjectOfType<ResetScene>();
        if (reset == null)
        {
            Debug.LogError("ResetScene script not found in the scene!", this);
        }
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _jumpAction.action.performed += OnJumpPerformed;
        _jumpAction.action.canceled += OnJumpCanceled;
        _moveAction.action.Enable();
        _jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        _jumpAction.action.performed -= OnJumpPerformed;
        _jumpAction.action.canceled -= OnJumpCanceled;
        _moveAction.action.Disable();
        _jumpAction.action.Disable();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            reset.ResetEntities();
        }
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
        FlipSprite();
        animator.SetBool("isJumping", !_grounded);
        animator.SetFloat("yVelocity", _rb.linearVelocityY);
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = _jumpToConsume,
            JumpHeld = _jumpHeld,
            Move = _moveAction.action.ReadValue<Vector2>()
        };

        if (SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        if (_jumpToConsume) _jumpToConsume = false;
    }

    private bool _jumpToConsume;
    private bool _jumpHeld;
    private float _timeJumpWasPressed;

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        _jumpToConsume = true;
        _jumpHeld = true;
        _timeJumpWasPressed = _time;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        _jumpHeld = false;
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
        animator.SetFloat("xVelocity", Math.Abs(_rb.linearVelocityX));
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, GrounderDistance, ~PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, GrounderDistance, ~PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion


    #region Jumping

    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = JumpPower;
        Jumped?.Invoke();
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? GroundDeceleration : AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * MaxSpeed, Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = GroundingForce;
        }
        else
        {
            var inAirGravity = FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Animator

    void FlipSprite()
    {
        if (isFacingRight && _frameInput.Move.x < 0f || !isFacingRight && _frameInput.Move.x > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    #endregion

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_moveAction == null) Debug.LogWarning("Please assign a Move InputActionReference to the Player Controller's Move Action slot", this);
        if (_jumpAction == null) Debug.LogWarning("Please assign a Jump InputActionReference to the Player Controller's Jump Action slot", this);
    }
#endif
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public Vector2 FrameInput { get; }
}

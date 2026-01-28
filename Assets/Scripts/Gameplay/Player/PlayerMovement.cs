using System.Runtime.CompilerServices;
using Unity.Hierarchy;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reverences")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _headColl;

    private Rigidbody2D _rb;

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private float _isPastApexThreshold;

    private float _jumpBufferTimer;
    private float _jumpReleasedDuringBuffer;

    private float _coyoteTimer;
    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        JumpChecks();
    }
    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        if(_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputController.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputController.Movement);

        }
    }
    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);
            Vector2 targetVelocety = Vector2.zero;
            if (InputController.RunIsHeld)
            {
                targetVelocety = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocety = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocety, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }
    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (_isFacingRight && moveInput.x < 0)
        {
            Turn(true);
        }
    }
    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }

        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
    #endregion
    #region Jump
    private void JumpChecks()
    {

    }
    private void Jump()
    {

    }
    #endregion
    #region Groundcheck
    //collision checks 
    private void IsGrounded()
    {
        Vector2 boxCastOrign = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrign, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

    }
    private void CollisionChecks()
    {
        IsGrounded();
    }
    #endregion
}
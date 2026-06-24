using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Roguelite.Player;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    public Rigidbody2D Rb => rb;
    public Animator Animator => animator;

    // === Debug Config cho Module này ===
    private const string MODULE_NAME = "PlayerController";
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true; // Config riêng cho PlayerController
    [SerializeField] private bool logMovement = true; // Log chi tiết về di chuyển
    [SerializeField] private bool logActions = true; // Log các hành động (jump, attack, run)
    [SerializeField] private bool logStateChanges = true; // Log thay đổi trạng thái
    
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpImpulse = 10f;
    public float airWalkSpeed = 3f;

    [Header("Attack Settings")]
    public float attackMovementMultiplier = 0f; // 0 = no movement during attack, 1 = full movement

    Vector2 moveInput;
    TouchingDirections touchingDirections;
    PlayerStats playerStats;

    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !touchingDirections.IsOnWall && !touchingDirections.IsOnCeiling)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        {
                            if (IsRunning)
                            {
                                return runSpeed;
                            }
                            else
                            {
                                return walkSpeed;
                            }
                        }
                    }
                    else
                    {
                        // Air move
                        return airWalkSpeed;
                    }
                }
                else
                {
                    // Idle speed là 0
                    return 0;
                }
            }
            else
            {
                // Không thể di chuyển
                return 0;
            }
        }
    }


    // Bool trạng thái di chuyển
    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving
    {
        get { return _isMoving; }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    // Bool trạng thái chạy
    [SerializeField]
    private bool _isRunning = false;
    public bool IsRunning
    {
        get { return _isRunning; }
        set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }


    // Bool trạng thái có thể di chuyển
    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.canMove);
        }
    }

    public bool IsAlive
    {
        get
        {
            return animator.GetBool(AnimationStrings.isAlive);
        }
    }

    public bool IsAttacking
    {
        get
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Attack") || stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2") || stateInfo.IsName("Combo");
        }
    }


    // Hướng mặt của nhân vật
    public bool _isFacingRight = true;
    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        }
    }


    // Khởi tạo các thành phần cần thiết
    private void Awake()
    {
        // Đăng ký module debug với DebugLogger
        DebugLogger.SetModuleDebug(MODULE_NAME, enableDebug);
        DebugLogger.Log("PlayerController đã cài đặt", MODULE_NAME, DebugLogger.LogType.Success);



        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        playerStats = GetComponent<PlayerStats>();
   }


    private void Update()
    {
    }

    // Cập nhật vật lý mỗi khung hình cố định
    private void FixedUpdate()
    {

        float moveSpeed = CurrentMoveSpeed;
        if (IsAttacking) moveSpeed *= attackMovementMultiplier;
        if(playerStats == null || !playerStats.LockVelocity)
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }



    // Xử lý di chuyển dựa trên input
    public void OnMove(InputAction.CallbackContext context)
    {

        moveInput = context.ReadValue<Vector2>();
        if(IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;
            SetFacingDirection(moveInput);
            
            // Debug log di chuyển
            if (logMovement && IsMoving)
            {
                DebugLogger.Log($"Moving: {moveInput}, Speed: {CurrentMoveSpeed}", MODULE_NAME);
            }
        }
        else
        {
            IsMoving = false;
        }
    }


    // Cập nhật hướng mặt dựa trên input di chuyển
    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
            if (logStateChanges)
            {
                DebugLogger.Log("Facing RIGHT", MODULE_NAME);
            }
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
            if (logStateChanges)
            {
                DebugLogger.Log("Facing LEFT", MODULE_NAME);
            }
        }
    }



    // Xử lý chạy dựa trên input
    public void onRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
            if (logActions)
            {
                DebugLogger.Log("Started RUNNING", MODULE_NAME, DebugLogger.LogType.Info);
            }
        }
        else if (context.canceled)
        {
            IsRunning = false;
            if (logActions)
            {
                DebugLogger.Log("Stopped running", MODULE_NAME);
            }
        }
    }


    // Xử lý nhảy dựa trên input
    public void onJump(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirections.IsGrounded && CanMove)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);
            
            if (logActions)
            {
                DebugLogger.Log($"JUMP! Impulse: {jumpImpulse}", MODULE_NAME, DebugLogger.LogType.Success);
            }
        }
        else if (context.started && logActions)
        {
            // Log lý do không thể nhảy
            if (!touchingDirections.IsGrounded)
            {
                DebugLogger.LogWarning("Cannot jump: Not grounded", MODULE_NAME);
            }
            else if (!CanMove)
            {
                DebugLogger.LogWarning("Cannot jump: Movement disabled", MODULE_NAME);
            }
        }
    }




    // Xử lý tấn công dựa trên input
    public void onAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
            
            if (logActions)
            {
                DebugLogger.Log("ATTACK triggered!", MODULE_NAME, DebugLogger.LogType.Info);
            }
        }
    }

    public void onHit(float damage, Vector2 knockback)
    {
        rb.velocity = new Vector2(knockback.x, rb.velocity.y + knockback.y );
    }
    
    public void SetVelocityX(float x)
    {
        rb.velocity = new Vector2(x, rb.velocity.y);
    }
    

    public void SetDebugEnabled(bool enabled)
    {
        enableDebug = enabled;
        DebugLogger.SetModuleDebug(MODULE_NAME, enabled);
    }
    

    public void ConfigureDebugLogs(bool movement, bool actions, bool stateChanges)
    {
        logMovement = movement;
        logActions = actions;
        logStateChanges = stateChanges;
        DebugLogger.Log($"Debug config updated - Movement:{movement}, Actions:{actions}, States:{stateChanges}", MODULE_NAME);
    }


    public void onPause(InputAction.CallbackContext context)
    {

    }

}

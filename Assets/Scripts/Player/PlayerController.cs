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

    // [Header("Healing Settings")]
    // public int healAmount = 30;
    // public int maxHealingItems = 5;
    // [SerializeField] private int _healingItemCount = 3;
    // public float healingAnimationDuration = 1.1f; // Duration to lock movement
    // private bool isHealing = false;

    // [Header("Dash Settings")]
    // public float dashSpeed = 15f;
    // public float dashDuration = 0.2f;
    // public float dashCooldown = 1f;
    // private bool isDashing = false;
    // private bool canDash = true;
    // private float dashTimeLeft = 0f;
    // private float dashCooldownTimer = 0f;

    // public int HealingItemCount
    // {
    //     get { return _healingItemCount; }
    //     set { _healingItemCount = value; } // Changed from private to public for checkpoint system
    // }


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

    // public bool IsHealing
    // {
    //     get { return isHealing; }
    // }

    // public bool IsDashing
    // {
    //     get { return isDashing; }
    //     private set
    //     {
    //         isDashing = value;
    //         animator.SetBool(AnimationStrings.isDashing, value);
    //     }
    // }








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
        // Handle dash cooldown
        // if (!canDash)
        // {
        //     dashCooldownTimer -= Time.deltaTime;
        //     if (dashCooldownTimer <= 0f)
        //     {
        //         canDash = true;
        //         if (logActions)
        //         {
        //             DebugLogger.Log("Dash ready!", MODULE_NAME, DebugLogger.LogType.Info);
        //         }
        //     }
        // }

        // // Handle dash duration
        // if (IsDashing)
        // {
        //     dashTimeLeft -= Time.deltaTime;
        //     if (dashTimeLeft <= 0f)
        //     {
        //         EndDash();
        //     }
        // }
    }

    // Cập nhật vật lý mỗi khung hình cố định
    private void FixedUpdate()
    {
        // Handle dash movement
        // if (IsDashing)
        // {
        //     float dashDirection = IsFacingRight ? 1f : -1f;
        //     rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
        //     return;
        // }

        float moveSpeed = CurrentMoveSpeed;
        if (IsAttacking) moveSpeed *= attackMovementMultiplier;
        // if (IsHealing) moveSpeed = 0; // Block movement during healing
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

    // public void onHealing(InputAction.CallbackContext context)
    // {
    //     // Debug: Check if input is received
    //     if (logActions)
    //     {
    //         DebugLogger.Log($"onHealing called - started:{context.started} performed:{context.performed} canceled:{context.canceled}", MODULE_NAME, DebugLogger.LogType.Info);
    //         DebugLogger.Log($"Conditions - IsAlive:{IsAlive} CanMove:{CanMove} IsHealing:{isHealing} Items:{_healingItemCount}", MODULE_NAME, DebugLogger.LogType.Info);
    //     }

    //     if (context.started && IsAlive && CanMove && !isHealing)
    //     {
    //         if (_healingItemCount <= 0)
    //         {
    //             isHealing = true;
    //             animator.SetTrigger(AnimationStrings.emptyHealTrigger);
    //             StartCoroutine(HealingAnimationLock(healingAnimationDuration));
                
    //             if (logActions)
    //             {
    //                 DebugLogger.LogWarning("No healing items left!", MODULE_NAME);
    //             }
    //             return;
    //         }

    //         if (playerStats.Heal(healAmount))
    //         {
    //             _healingItemCount--;
    //             isHealing = true;
    //             animator.SetTrigger(AnimationStrings.healTrigger);
    //             StartCoroutine(HealingAnimationLock(healingAnimationDuration));
                
    //             if (logActions)
    //             {
    //                 DebugLogger.Log($"HEAL triggered! +{healAmount} HP. Items left: {_healingItemCount}/{maxHealingItems}", MODULE_NAME, DebugLogger.LogType.Success);
    //             }
    //         }
    //         else
    //         {
    //             if (logActions)
    //             {
    //                 DebugLogger.LogWarning("Already at full health!", MODULE_NAME);
    //             }
    //         }
    //     }
    // }

    // private IEnumerator HealingAnimationLock(float duration)
    // {
    //     if (logActions)
    //     {
    //         DebugLogger.Log($"Healing animation started, movement locked for {duration}s", MODULE_NAME, DebugLogger.LogType.Info);
    //     }

    //     yield return new WaitForSeconds(duration);
        
    //     isHealing = false;
        
    //     if (logActions)
    //     {
    //         DebugLogger.Log("Healing animation ended, can move and heal again", MODULE_NAME);
    //     }
    // }

    // public void AddHealingItem(int amount = 1)
    // {
    //     int oldCount = _healingItemCount;
    //     _healingItemCount = Mathf.Min(_healingItemCount + amount, maxHealingItems);
        
    //     if (logActions)
    //     {
    //         DebugLogger.Log($"Added {_healingItemCount - oldCount} healing item(s). Total: {_healingItemCount}/{maxHealingItems}", MODULE_NAME, DebugLogger.LogType.Success);
    //     }
    // }

    public void onPause(InputAction.CallbackContext context)
    {

    }

    // public void onDash(InputAction.CallbackContext context)
    // {
    //     if (context.started && IsAlive && CanMove && canDash && !IsDashing && !isHealing)
    //     {
    //         StartDash();
    //     }
    //     else if (context.started && logActions)
    //     {
    //         // Log reason for not dashing
    //         if (!IsAlive)
    //         {
    //             DebugLogger.LogWarning("Cannot dash: Not alive", MODULE_NAME);
    //         }
    //         else if (!CanMove)
    //         {
    //             DebugLogger.LogWarning("Cannot dash: Movement disabled", MODULE_NAME);
    //         }
    //         else if (!canDash)
    //         {
    //             DebugLogger.LogWarning($"Cannot dash: Cooldown ({dashCooldownTimer:F1}s remaining)", MODULE_NAME);
    //         }
    //         else if (IsDashing)
    //         {
    //             DebugLogger.LogWarning("Cannot dash: Already dashing", MODULE_NAME);
    //         }
    //         // else if (isHealing)
    //         // {
    //         //     DebugLogger.LogWarning("Cannot dash: Currently healing", MODULE_NAME);
    //         // }
    //     }
    // }

    // private void StartDash()
    // {
    //     IsDashing = true;
    //     canDash = false;
    //     dashTimeLeft = dashDuration;
    //     dashCooldownTimer = dashCooldown;
    //     animator.SetTrigger(AnimationStrings.dashTrigger);

    //     // Optionally make player invincible during dash
    //     // playerStats.isInvincible = true;

    //     if (logActions)
    //     {
    //         DebugLogger.Log($"DASH! Direction: {(IsFacingRight ? "RIGHT" : "LEFT")}, Speed: {dashSpeed}", MODULE_NAME, DebugLogger.LogType.Success);
    //     }
    // }

    // private void EndDash()
    // {
    //     IsDashing = false;
    //     rb.velocity = new Vector2(0f, rb.velocity.y); // Reset horizontal velocity

    //     // Optionally remove invincibility
    //     // playerStats.isInvincible = false;

    //     if (logActions)
    //     {
    //         DebugLogger.Log("Dash ended", MODULE_NAME);
    //     }
    // }
}

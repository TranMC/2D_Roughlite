using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    // === Debug Config ===
    private const string MODULE_NAME = "TouchingDirections";
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = false;
    [SerializeField] private bool logGroundedChanges = false;
    [SerializeField] private bool logWallContact = false;
    [SerializeField] private bool logCeilingContact = false;
    
    [Header("Detection Settings")]
    CapsuleCollider2D touchingCol;
    Animator animator;

    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    public float wallCheckDistance = 0.2f;
    public float ceilingCheckDistance = 0.05f;


    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];


    [SerializeField]
    private bool _isGrounded;

    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set
        {
            if (_isGrounded != value && logGroundedChanges)
            {
                DebugLogger.Log($"Grounded: {value}", MODULE_NAME, value ? DebugLogger.LogType.Success : DebugLogger.LogType.Warning);
            }
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }
    }

    [SerializeField]
    private bool _isOnWall;

    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            if (_isOnWall != value && logWallContact)
            {
                DebugLogger.Log($"On Wall: {value}", MODULE_NAME, DebugLogger.LogType.Info);
            }
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
        }
    }

    [SerializeField]
    private bool _isOnCeiling;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            if (_isOnCeiling != value && logCeilingContact)
            {
                DebugLogger.Log($"On Ceiling: {value}", MODULE_NAME, DebugLogger.LogType.Info);
            }
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.IsOnCeiling, value);
        }
    }

    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        
        // Đăng ký module debug
        DebugLogger.SetModuleDebug(MODULE_NAME, enableDebug);
        DebugLogger.Log("TouchingDirections initialized", MODULE_NAME, DebugLogger.LogType.Success);
    }

    void Start()
    {

    }
    private void FixedUpdate()
    {
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        IsOnWall = touchingCol.Cast(wallCheckDirection, castFilter, wallHits, wallCheckDistance) > 0;
        IsOnCeiling = touchingCol.Cast(Vector2.up, castFilter, ceilingHits, ceilingCheckDistance) > 0;
    }

}

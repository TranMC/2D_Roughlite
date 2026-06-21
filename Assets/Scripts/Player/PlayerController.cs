using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Bộ điều khiển nhân vật chính. Quản lý Rigidbody2D, Animator, thu thập Input,
    /// thực hiện kiểm tra tiếp đất và vận hành Máy trạng thái (State Machine).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public Rigidbody2D Rb { get; private set; }
        public Animator Animator { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public PlayerStats Stats { get; private set; }

        [Header("Movement Settings")]
        [Tooltip("Tốc độ di chuyển ngang trên mặt đất.")]
        [SerializeField] private float moveSpeed = 8f;

        [Tooltip("Lực nhảy hướng lên.")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Hệ số kiểm soát di chuyển ngang khi đang ở trên không (0-1).")]
        [Range(0f, 1f)] [SerializeField] private float airControlFactor = 0.8f;

        [Tooltip("Hệ số giảm lực nhảy khi nhả nút nhảy sớm (Variable Jump Height).")]
        [Range(0f, 1f)] [SerializeField] private float cutJumpHeightFactor = 0.5f;

        [Tooltip("Hệ số trọng lực bổ sung khi rơi xuống để tạo cảm giác nặng hơn (Snappy Fall).")]
        [SerializeField] private float fallMultiplier = 2.5f;

        [Header("Ground Check Settings")]
        [Tooltip("Điểm kiểm tra tiếp đất nằm ở chân nhân vật.")]
        [SerializeField] private Transform groundCheckPoint;

        [Tooltip("Bán kính vùng quét hình cầu kiểm tra tiếp đất.")]
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Tooltip("Layer được coi là mặt đất có thể đi lên và nhảy từ đó.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Game Feel (Juice)")]
        [Tooltip("Thời gian (giây) đệm cho phép nhảy sau khi vừa rời khỏi rìa mặt đất.")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Thời gian (giây) lưu giữ lệnh nhảy nếu người chơi nhấn trước khi tiếp đất.")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        // Properties công khai để các State truy xuất cấu hình
        public float MoveSpeed => moveSpeed;
        public float JumpForce => jumpForce;
        public float AirControlFactor => airControlFactor;
        public float CutJumpHeightFactor => cutJumpHeightFactor;
        public float FallMultiplier => fallMultiplier;

        // Các Timer hỗ trợ game feel
        public float CoyoteTimer { get; private set; }
        public float JumpBufferTimer { get; private set; }

        // Giá trị Input được cập nhật liên tục
        public float InputX { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpReleased { get; private set; }

        // Máy trạng thái & các trạng thái cụ thể
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerFallState FallState { get; private set; }

        // Hướng quay mặt của nhân vật
        private bool isFacingRight = true;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            // Tìm Animator và SpriteRenderer ở bản thân hoặc các Object con (thường Prefab tách phần Model/Visual ra con)
            Animator = GetComponentInChildren<Animator>();
            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Stats = GetComponent<PlayerStats>();

            // Khởi tạo máy trạng thái và các state tương ứng với chuỗi Animator bool
            StateMachine = new PlayerStateMachine();
            
            IdleState = new PlayerIdleState(this, StateMachine, "isIdle");
            MoveState = new PlayerMoveState(this, StateMachine, "isMoving");
            JumpState = new PlayerJumpState(this, StateMachine, "isJumping");
            FallState = new PlayerFallState(this, StateMachine, "isFalling");
        }

        private void Start()
        {
            // Bắt đầu với trạng thái Idle
            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            // Thu thập các tín hiệu input từ bàn phím/tay cầm
            GatherInput();

            // Cập nhật các bộ đếm thời gian
            UpdateTimers();

            // Cập nhật tham số Animator
            UpdateAnimator();

            // Cập nhật logic máy trạng thái
            StateMachine.CurrentState.LogicUpdate();
        }

        /// <summary>
        /// Cập nhật tham số isGrounded trong Animator.
        /// </summary>
        private void UpdateAnimator()
        {
            if (Animator != null)
            {
                Animator.SetBool("isGrounded", IsGrounded());
            }
        }

        private void FixedUpdate()
        {
            // Cập nhật các thay đổi vật lý của máy trạng thái
            StateMachine.CurrentState.PhysicsUpdate();
        }

        /// <summary>
        /// Thu thập dữ liệu đầu vào.
        /// </summary>
        private void GatherInput()
        {
            InputX = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                JumpBufferTimer = jumpBufferTime;
                JumpPressed = true;
            }
            else
            {
                JumpPressed = false;
            }

            if (Input.GetButtonUp("Jump"))
            {
                JumpReleased = true;
            }
            else
            {
                JumpReleased = false;
            }
        }

        /// <summary>
        /// Cập nhật Coyote Time và Jump Buffer.
        /// </summary>
        private void UpdateTimers()
        {
            // Cập nhật Coyote Time (thời gian được phép nhảy sau khi rơi khỏi mặt đất)
            if (IsGrounded())
            {
                CoyoteTimer = coyoteTime;
            }
            else
            {
                CoyoteTimer -= Time.deltaTime;
            }

            // Cập nhật Jump Buffer (hàng đợi nút nhảy)
            if (JumpBufferTimer > 0f)
            {
                JumpBufferTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Kiểm tra nhân vật có đang đứng trên mặt đất không.
        /// </summary>
        public bool IsGrounded()
        {
            if (groundCheckPoint == null)
            {
                // Fallback nếu người chơi chưa gán groundCheckPoint
                // Bắt đầu raycast từ dưới chân nhân vật xuống dưới để tránh tự va chạm
                Collider2D col = GetComponent<Collider2D>();
                Vector2 startPos = transform.position;
                if (col != null)
                {
                    startPos = new Vector2(transform.position.x, col.bounds.min.y + 0.05f);
                }
                RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.down, 0.15f, groundLayer);
                return hit.collider != null && hit.collider.transform != transform;
            }

            Collider2D[] results = Physics2D.OverlapCircleAll(groundCheckPoint.position, groundCheckRadius, groundLayer);
            foreach (var result in results)
            {
                // Bỏ qua các collider thuộc về chính bản thân Player
                if (result.transform != transform)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Thiết lập tốc độ di chuyển ngang.
        /// </summary>
        public void SetVelocityX(float velocityX)
        {
            Rb.velocity = new Vector2(velocityX, Rb.velocity.y);
        }

        /// <summary>
        /// Thiết lập tốc độ di chuyển dọc.
        /// </summary>
        public void SetVelocityY(float velocityY)
        {
            Rb.velocity = new Vector2(Rb.velocity.x, velocityY);
        }

        /// <summary>
        /// Tiêu thụ tín hiệu nhảy (triệt tiêu ngay lập tức Coyote và Jump Buffer).
        /// </summary>
        public void UseJumpInput()
        {
            JumpBufferTimer = 0f;
            CoyoteTimer = 0f;
        }

        /// <summary>
        /// Kiểm tra hướng và thực hiện lật nhân vật nếu đổi chiều di chuyển.
        /// </summary>
        public void CheckFlip(float horizontalInput)
        {
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }

        /// <summary>
        /// Lật sprite nhân vật bằng cách nhân scale X với -1.
        /// </summary>
        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // Vẽ Gizmos để dễ tinh chỉnh bán kính groundCheck trong Unity Editor
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
    }
}

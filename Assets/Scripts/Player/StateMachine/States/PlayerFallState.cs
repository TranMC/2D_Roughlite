using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Trạng thái rơi tự do của người chơi (khi đi xuống).
    /// </summary>
    public class PlayerFallState : PlayerState
    {
        public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName)
            : base(player, stateMachine, animBoolName) { }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            player.CheckFlip(player.InputX);

            // Coyote Jump: Nếu nút nhảy được nhấn và vẫn nằm trong thời gian Coyote đệm
            if (player.JumpBufferTimer > 0f && player.CoyoteTimer > 0f)
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }

            // Kiểm tra tiếp đất (Landing)
            if (player.IsGrounded())
            {
                // Nếu người chơi nhấn nhảy trước khi tiếp đất (Jump Buffering) -> thực hiện cú nhảy tiếp theo ngay lập tức
                if (player.JumpBufferTimer > 0f)
                {
                    stateMachine.ChangeState(player.JumpState);
                }
                // Nếu có nhập hướng di chuyển ngang -> chuyển sang MoveState
                else if (Mathf.Abs(player.InputX) > 0.01f)
                {
                    stateMachine.ChangeState(player.MoveState);
                }
                // Ngược lại chuyển về IdleState
                else
                {
                    stateMachine.ChangeState(player.IdleState);
                }
                return;
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // Cho phép di chuyển ngang hạn chế trên không (Air Control)
            player.SetVelocityX(player.InputX * player.MoveSpeed * player.AirControlFactor);

            // Cơ chế Snappy Fall: Nhân vật rơi xuống nhanh hơn để tạo cảm giác kiểm soát vật lý chặt chẽ hơn
            if (player.Rb.velocity.y < 0.01f)
            {
                float extraGravity = Physics2D.gravity.y * (player.FallMultiplier - 1f) * Time.fixedDeltaTime;
                player.Rb.velocity += new Vector2(0f, extraGravity);
            }
        }
    }
}

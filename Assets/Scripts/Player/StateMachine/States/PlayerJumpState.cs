using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Trạng thái nhảy lên của người chơi.
    /// </summary>
    public class PlayerJumpState : PlayerState
    {
        private bool isJumping;

        public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName)
            : base(player, stateMachine, animBoolName) { }

        public override void Enter()
        {
            base.Enter();

            // Áp dụng lực nhảy hướng lên
            player.Rb.velocity = new Vector2(player.Rb.velocity.x, player.JumpForce);

            // Tiêu thụ tín hiệu nhảy (reset các bộ đệm)
            player.UseJumpInput();

            isJumping = true;
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            player.CheckFlip(player.InputX);

            // Cơ chế Variable Jump Height: Nếu người dùng thả nút nhảy sớm khi nhân vật đang đi lên
            // Ta triệt tiêu một phần vận tốc hướng lên để nhân vật rơi xuống sớm hơn (nhảy thấp)
            if (isJumping && player.JumpReleased)
            {
                if (player.Rb.velocity.y > 0f)
                {
                    player.Rb.velocity = new Vector2(player.Rb.velocity.x, player.Rb.velocity.y * player.CutJumpHeightFactor);
                }
                isJumping = false;
            }

            // Khi vận tốc hướng lên kết thúc (bắt đầu đi xuống hoặc đứng im trên đỉnh nhảy) -> chuyển sang FallState
            if (player.Rb.velocity.y <= 0.01f)
            {
                stateMachine.ChangeState(player.FallState);
                return;
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // Cho phép di chuyển ngang hạn chế trên không (Air Control)
            player.SetVelocityX(player.InputX * player.MoveSpeed * player.AirControlFactor);
        }
    }
}

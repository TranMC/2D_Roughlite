using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Trạng thái đứng yên của người chơi.
    /// </summary>
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName)
            : base(player, stateMachine, animBoolName) { }

        public override void Enter()
        {
            base.Enter();
            player.SetVelocityX(0f);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Nếu có di chuyển ngang -> chuyển sang MoveState
            if (Mathf.Abs(player.InputX) > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
                return;
            }

            // Nếu nhấn nút nhảy (được lưu trong jump buffer) và có thể nhảy -> chuyển sang JumpState
            if (player.JumpBufferTimer > 0f && player.CoyoteTimer > 0f)
            {
                stateMachine.ChangeState(player.JumpState);
                return;
            }

            // Nếu không còn chạm đất -> chuyển sang FallState
            if (!player.IsGrounded())
            {
                stateMachine.ChangeState(player.FallState);
                return;
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            // Đảm bảo triệt tiêu vận tốc X khi đứng yên
            player.SetVelocityX(0f);
        }
    }
}

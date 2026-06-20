using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Trạng thái di chuyển trên mặt đất của người chơi.
    /// </summary>
    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName)
            : base(player, stateMachine, animBoolName) { }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Kiểm tra và lật sprite nhân vật theo hướng di chuyển
            player.CheckFlip(player.InputX);

            // Nếu không còn nhập di chuyển ngang -> chuyển sang IdleState
            if (Mathf.Abs(player.InputX) < 0.01f)
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            // Nếu nhấn nút nhảy và có thể nhảy -> chuyển sang JumpState
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
            // Di chuyển nhân vật dựa theo input ngang
            player.SetVelocityX(player.InputX * player.MoveSpeed);
        }
    }
}

using UnityEngine;

namespace Roguelite.Player
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho tất cả các trạng thái của người chơi.
    /// </summary>
    public abstract class PlayerState
    {
        protected PlayerController player;
        protected PlayerStateMachine stateMachine;
        protected string animBoolName;

        protected PlayerState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName)
        {
            this.player = player;
            this.stateMachine = stateMachine;
            this.animBoolName = animBoolName;
        }

        /// <summary>
        /// Được gọi khi bắt đầu bước vào trạng thái này.
        /// </summary>
        public virtual void Enter()
        {
            if (player.Animator != null && !string.IsNullOrEmpty(animBoolName))
            {
                player.Animator.SetBool(animBoolName, true);
            }
        }

        /// <summary>
        /// Được gọi trước khi rời khỏi trạng thái này.
        /// </summary>
        public virtual void Exit()
        {
            if (player.Animator != null && !string.IsNullOrEmpty(animBoolName))
            {
                player.Animator.SetBool(animBoolName, false);
            }
        }

        /// <summary>
        /// Chạy mỗi khung hình trong hàm Update() của PlayerController.
        /// </summary>
        public virtual void LogicUpdate() { }

        /// <summary>
        /// Chạy mỗi chu kỳ vật lý trong hàm FixedUpdate() của PlayerController.
        /// </summary>
        public virtual void PhysicsUpdate() { }
    }
}

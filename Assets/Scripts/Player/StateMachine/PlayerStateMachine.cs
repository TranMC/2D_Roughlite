namespace Roguelite.Player
{
    /// <summary>
    /// Quản lý trạng thái hiện tại của người chơi và xử lý việc chuyển đổi giữa các trạng thái.
    /// </summary>
    public class PlayerStateMachine
    {
        public PlayerState CurrentState { get; private set; }

        /// <summary>
        /// Khởi tạo trạng thái ban đầu của Player.
        /// </summary>
        public void Initialize(PlayerState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Thay đổi trạng thái hiện tại sang trạng thái mới.
        /// </summary>
        public void ChangeState(PlayerState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}

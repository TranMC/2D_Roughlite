using UnityEngine;
using Roguelite.Enemy;

public class Enemy_AI : EnemyBase
{
    // === Debug Config cho Module này ===
    private const string MODULE_NAME = "Enemy_AI";
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true; // Config riêng cho Enemy_AI
    [SerializeField] private bool logBehavior = true; // Log các hành vi của AI
    

    protected override void OnStateEnter(EnemyState enteringState, EnemyState previousState)
    {
        base.OnStateEnter(enteringState, previousState);
        
        switch(enteringState)
        {
            case EnemyState.Idle:
                DebugLogger.Log("Đã vào trạng thái IDLE");
                break;
            case EnemyState.Patrol:
                DebugLogger.Log("Đã vào trạng thái PATROL");
                break;
            case EnemyState.Attack:
                DebugLogger.Log("Thực hiện đòn Attack");
                anim.SetTrigger("AI_attack");
                break;
            case EnemyState.Dead:
                DebugLogger.Log("Đã vào trạng thái DEAD");
                anim.SetTrigger("AI_die");
                break;
            case EnemyState.Hit:
                DebugLogger.Log("Bị tấn công");
                anim.SetTrigger("AI_hit");
                break;
        }
    }
    protected override void OnStateExit(EnemyState exitingState)
    {
        base.OnStateExit(exitingState);
    }
    
}

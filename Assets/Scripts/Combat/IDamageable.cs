using UnityEngine;

namespace Roguelite.Combat
{
    /// <summary>
    /// Giao diện chung cho tất cả các thực thể có thể nhận sát thương và lực đẩy (Knockback).
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Gây sát thương lên thực thể mà không có lực đẩy.
        /// </summary>
        /// <param name="damage">Lượng sát thương</param>
        void TakeDamage(float damage);

        /// <summary>
        /// Gây sát thương lên thực thể kèm theo lực đẩy (Knockback).
        /// </summary>
        /// <param name="damage">Lượng sát thương</param>
        /// <param name="knockback">Lực đẩy áp dụng</param>
        void TakeDamage(float damage, Vector2 knockback);
    }
}

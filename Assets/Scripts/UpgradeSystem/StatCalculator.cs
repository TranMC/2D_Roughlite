using UnityEngine;

namespace Roguelite.UpgradeSystem
{
    /// <summary>
    /// Cấu trúc chứa gom nhóm các Modifier tác động lên 1 chỉ số duy nhất,
    /// phục vụ việc tính toán tập trung và giải quyết xung đột thứ tự áp dụng.
    /// </summary>
    public struct StatModifierGroup
    {
        public float flatSum;
        public float percentAdditiveSum;
        public float percentMultiplicativeProduct;

        public static StatModifierGroup Default => new StatModifierGroup
        {
            flatSum = 0f,
            percentAdditiveSum = 0f,
            percentMultiplicativeProduct = 1f
        };

        public void AddFlat(float value)
        {
            flatSum += value;
        }

        public void AddPercentAdditive(float percentValue)
        {
            percentAdditiveSum += percentValue;
        }

        public void AddPercentMultiplicative(float percentValue)
        {
            percentMultiplicativeProduct *= (1f + percentValue);
        }
    }

    /// <summary>
    /// Trình tính toán và giải quyết xung đột thứ tự khi nhiều Upgrade (Vĩnh viễn hoặc Trong trận)
    /// cùng tác động lên 1 chỉ số của Player.
    /// </summary>
    public static class StatCalculator
    {
        /// <summary>
        /// Tính toán giá trị chỉ số cuối cùng (Final Value) dựa theo công thức tiêu chuẩn Roguelite:
        /// FinalValue = (BaseValue + FlatSum) * (1 + AdditivePercentSum) * MultiplicativeProduct
        /// </summary>
        /// <param name="baseValue">Giá trị cơ bản gốc của chỉ số</param>
        /// <param name="group">Tập hợp các modifier cộng dồn</param>
        /// <param name="minValue">Giá trị tối thiểu cho phép (tránh chỉ số bị âm hoặc bằng 0 ngoài ý muốn)</param>
        /// <returns>Giá trị chỉ số sau khi giải quyết xung đột</returns>
        public static float CalculateFinalValue(float baseValue, StatModifierGroup group, float minValue = 0f)
        {
            float baseWithFlat = baseValue + group.flatSum;
            float additiveMultiplier = 1f + group.percentAdditiveSum;
            float multiplicativeMultiplier = group.percentMultiplicativeProduct;

            float finalValue = baseWithFlat * additiveMultiplier * multiplicativeMultiplier;
            return Mathf.Max(finalValue, minValue);
        }

        /// <summary>
        /// Tính toán giá trị với hệ số đơn giản (Flat + Additive Percent).
        /// </summary>
        public static float CalculateSimpleValue(float baseValue, float flatBonus, float percentBonus, float minValue = 0f)
        {
            float finalValue = (baseValue + flatBonus) * (1f + percentBonus);
            return Mathf.Max(finalValue, minValue);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Roguelite.BuffSystem
{
    /// <summary>
    /// Hiển thị các icon buff phía trên đầu player, căn giữa theo số lượng buff đang có.
    /// Icon sẽ nhấp nháy khi buff sắp hết hiệu lực rồi biến mất khi buff hết thời gian.
    /// </summary>
    public class PlayerBuffDisplay : MonoBehaviour
    {
        [Header("Anchor")]
        [Tooltip("Transform neo icon. Nếu để trống sẽ tự tạo child 'BuffIconAnchor'.")]
        [SerializeField] private Transform iconAnchor;

        [Tooltip("Vị trí neo icon so với player (local space). Chỉnh để căn giữa trên đầu.")]
        [SerializeField] private Vector2 anchorOffset = new Vector2(0f, 2.5f);

        [Header("Layout")]
        [Tooltip("Khoảng cách giữa các icon (world units).")]
        [SerializeField] private float iconSpacing = 0.1f;

        [Tooltip("Scale hiển thị của từng icon.")]
        [SerializeField] private float iconScale = 0.4f;

        [SerializeField] private int sortingOrder = 20;

        [Header("Expiry Warning")]
        [Tooltip("Bắt đầu nhấp nháy khi còn lại bao nhiêu giây trước khi buff hết hiệu lực.")]
        [SerializeField] private float warningThreshold = 3f;

        [Tooltip("Tốc độ nhấp nháy icon khi sắp hết hiệu lực.")]
        [SerializeField] private float blinkSpeed = 6f;

        [Tooltip("Độ mờ tối thiểu khi nhấp nháy.")]
        [SerializeField] private float minAlpha = 0.25f;

        [Tooltip("Độ mờ tối đa khi nhấp nháy.")]
        [SerializeField] private float maxAlpha = 1f;

        private Transform iconContainer;
        private readonly List<SpriteRenderer> iconRenderers = new List<SpriteRenderer>();
        private readonly List<BuffDisplayInfo> activeDisplayInfos = new List<BuffDisplayInfo>();
        private float blinkTime;

        private void Awake()
        {
            EnsureAnchor();
        }

        private void LateUpdate()
        {
            if (iconAnchor == null)
            {
                return;
            }

            iconAnchor.localPosition = anchorOffset;

            // Giữ icon không bị lật ngược khi player đổi hướng
            float flipSign = transform.localScale.x < 0f ? -1f : 1f;
            iconAnchor.localScale = new Vector3(flipSign, 1f, 1f);
        }

        private void Update()
        {
            UpdateBlinkAnimation();
        }

        public void SetActiveBuffs(IReadOnlyList<BuffDisplayInfo> buffs)
        {
            EnsureAnchor();
            activeDisplayInfos.Clear();
            activeDisplayInfos.AddRange(buffs);

            if (buffs.Count == 0)
            {
                iconAnchor.gameObject.SetActive(false);
                return;
            }

            iconAnchor.gameObject.SetActive(true);
            iconContainer.gameObject.SetActive(true);

            while (iconRenderers.Count < buffs.Count)
            {
                iconRenderers.Add(CreateIconRenderer(iconRenderers.Count));
            }

            for (int i = 0; i < iconRenderers.Count; i++)
            {
                bool isActive = i < buffs.Count;
                SpriteRenderer renderer = iconRenderers[i];
                renderer.gameObject.SetActive(isActive);

                if (!isActive)
                {
                    continue;
                }

                renderer.sprite = buffs[i].Definition.Icon;
                SetRendererAlpha(renderer, maxAlpha);
            }

            LayoutIcons(buffs.Count);
        }

        public void UpdateBuffTimers(IReadOnlyList<BuffDisplayInfo> buffs)
        {
            activeDisplayInfos.Clear();
            activeDisplayInfos.AddRange(buffs);
        }

        private void UpdateBlinkAnimation()
        {
            if (activeDisplayInfos.Count == 0)
            {
                return;
            }

            blinkTime += Time.deltaTime * blinkSpeed;

            for (int i = 0; i < activeDisplayInfos.Count; i++)
            {
                if (i >= iconRenderers.Count || !iconRenderers[i].gameObject.activeSelf)
                {
                    continue;
                }

                SpriteRenderer renderer = iconRenderers[i];
                float remainingTime = activeDisplayInfos[i].RemainingTime;

                if (remainingTime <= warningThreshold)
                {
                    float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(blinkTime) + 1f) / 2f);
                    SetRendererAlpha(renderer, alpha);
                }
                else
                {
                    SetRendererAlpha(renderer, maxAlpha);
                }
            }
        }

        private static void SetRendererAlpha(SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }

        private void EnsureAnchor()
        {
            if (iconAnchor == null)
            {
                Transform existing = transform.Find("BuffIconAnchor");
                if (existing != null)
                {
                    iconAnchor = existing;
                }
                else
                {
                    GameObject anchorObject = new GameObject("BuffIconAnchor");
                    iconAnchor = anchorObject.transform;
                    iconAnchor.SetParent(transform, false);
                }
            }

            iconAnchor.localPosition = anchorOffset;

            if (iconContainer == null)
            {
                Transform existingContainer = iconAnchor.Find("BuffIcons");
                if (existingContainer != null)
                {
                    iconContainer = existingContainer;
                }
                else
                {
                    GameObject containerObject = new GameObject("BuffIcons");
                    iconContainer = containerObject.transform;
                    iconContainer.SetParent(iconAnchor, false);
                }
            }
        }

        private SpriteRenderer CreateIconRenderer(int index)
        {
            GameObject iconObject = new GameObject($"BuffIcon_{index}");
            iconObject.transform.SetParent(iconContainer, false);

            SpriteRenderer renderer = iconObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            iconObject.transform.localScale = Vector3.one * iconScale;

            return renderer;
        }

        private void LayoutIcons(int activeCount)
        {
            if (activeCount <= 0)
            {
                return;
            }

            float totalWidth = 0f;
            for (int i = 0; i < activeCount; i++)
            {
                totalWidth += GetIconWidth(iconRenderers[i]);
                if (i < activeCount - 1)
                {
                    totalWidth += iconSpacing;
                }
            }

            float cursorX = -totalWidth * 0.5f;

            for (int i = 0; i < activeCount; i++)
            {
                float iconWidth = GetIconWidth(iconRenderers[i]);
                Transform iconTransform = iconRenderers[i].transform;
                iconTransform.localPosition = new Vector3(cursorX + iconWidth * 0.5f, 0f, 0f);
                iconTransform.localScale = Vector3.one * iconScale;
                cursorX += iconWidth + iconSpacing;
            }
        }

        private float GetIconWidth(SpriteRenderer renderer)
        {
            if (renderer.sprite == null)
            {
                return iconSpacing;
            }

            return renderer.sprite.bounds.size.x * iconScale;
        }
    }
}

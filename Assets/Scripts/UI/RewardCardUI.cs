using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Roguelite.UpgradeSystem;

namespace Roguelite.UI
{
    /// <summary>
    /// Hiển thị thông tin của một Perk lên Card UI với hiệu ứng hover và animation.
    /// </summary>
    public class RewardCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;

        [Header("Rarity Outline Materials")]
        [SerializeField] private Material commonOutline;
        [SerializeField] private Material rareOutline;
        [SerializeField] private Material epicOutline;
        [SerializeField] private Material legendaryOutline;

        [Header("Animation & Hover Settings")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverOffsetY = 12f;
        [SerializeField] private float animationSpeed = 15f;
        [SerializeField] private float clickPunchScale = 0.92f;

        private PerkData currentPerkData;
        public PerkData CurrentPerkData => currentPerkData;

        private RectTransform rectTransform;
        private Vector3 baseLocalPosition;
        private Vector3 targetLocalPosition;
        private Vector3 targetScale = Vector3.one;
        private CanvasGroup cardCanvasGroup;
        private Coroutine entranceCoroutine;
        private bool isHovered = false;
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (isInitialized) return;
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                baseLocalPosition = rectTransform.anchoredPosition;
                targetLocalPosition = baseLocalPosition;
            }
            cardCanvasGroup = GetComponent<CanvasGroup>();
            if (cardCanvasGroup == null)
            {
                cardCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            isInitialized = true;
        }

        private void OnEnable()
        {
            InitializeReferences();
            ResetVisualState();
        }

        private void Update()
        {
            if (rectTransform == null) return;

            // Interpolate scale & position using Time.unscaledDeltaTime so it operates during game pause
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
            rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, targetLocalPosition, Time.unscaledDeltaTime * animationSpeed);
        }

        public void ResetVisualState()
        {
            isHovered = false;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = baseLocalPosition;
                rectTransform.localScale = Vector3.one;
            }
            targetLocalPosition = baseLocalPosition;
            targetScale = Vector3.one;

            if (cardCanvasGroup != null)
            {
                cardCanvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Gán dữ liệu Perk và cập nhật giao diện hiển thị.
        /// </summary>
        public void SetupCard(PerkData data)
        {
            InitializeReferences();
            currentPerkData = data;

            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            ResetVisualState();

            // Gán Text & Icon
            if (nameText != null) nameText.text = data.PerkName;
            if (descriptionText != null) descriptionText.text = data.Description;
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
                iconImage.color = Color.white;
                iconImage.material = null;
            }

            // Áp outline rarity lên viền card (không phải icon)
            Image outlineTarget = borderImage != null ? borderImage : GetComponent<Image>();
            if (outlineTarget != null)
            {
                outlineTarget.preserveAspect = true;
                outlineTarget.color = Color.white;

                Material sourceMat = GetRarityMaterial(data.Rarity);
                if (sourceMat != null)
                    outlineTarget.material = Instantiate(sourceMat);
            }
        }

        /// <summary>
        /// Chạy hiệu ứng xuất hiện (Pop-in staggered) cho Card.
        /// </summary>
        public void PlayEntranceAnimation(float delay)
        {
            InitializeReferences();
            if (entranceCoroutine != null)
            {
                StopCoroutine(entranceCoroutine);
            }
            entranceCoroutine = StartCoroutine(AnimateEntrance(delay));
        }

        private IEnumerator AnimateEntrance(float delay)
        {
            if (cardCanvasGroup != null) cardCanvasGroup.alpha = 0f;
            if (rectTransform != null) rectTransform.localScale = Vector3.zero;

            if (delay > 0f)
            {
                float elapsedDelay = 0f;
                while (elapsedDelay < delay)
                {
                    elapsedDelay += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease out overshoot (elastic pop)
                float scaleProgress = Mathf.Sin(t * Mathf.PI * 0.5f);
                float overscale = t >= 1f ? 1f : Mathf.Lerp(0f, 1.06f, scaleProgress);

                if (cardCanvasGroup != null) cardCanvasGroup.alpha = t;
                if (rectTransform != null && !isHovered)
                {
                    rectTransform.localScale = Vector3.one * overscale;
                }
                yield return null;
            }

            if (cardCanvasGroup != null) cardCanvasGroup.alpha = 1f;
            if (!isHovered)
            {
                targetScale = Vector3.one;
            }
            entranceCoroutine = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            targetScale = Vector3.one * hoverScale;
            targetLocalPosition = baseLocalPosition + new Vector3(0f, hoverOffsetY, 0f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            targetScale = Vector3.one;
            targetLocalPosition = baseLocalPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerClickEffect();
        }

        /// <summary>
        /// Tạo hiệu ứng nảy/co lại khi chọn thẻ (bằng chuột hoặc phím tắt).
        /// </summary>
        public void TriggerClickEffect()
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * clickPunchScale;
            }
        }

        private Material GetRarityMaterial(PerkRarity rarity)
        {
            switch (rarity)
            {
                case PerkRarity.Common: return commonOutline;
                case PerkRarity.Rare: return rareOutline;
                case PerkRarity.Epic: return epicOutline;
                case PerkRarity.Legendary: return legendaryOutline;
                default: return commonOutline;
            }
        }
    }
}

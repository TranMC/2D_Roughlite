using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;

    [Tooltip("Kích thước thanh máu trong world units (đơn vị game).")]
    [SerializeField] private Vector2 worldDisplaySize = new Vector2(3f, 0.45f);

    [SerializeField] private int worldSpaceSortingOrder = 100;

    private RectTransform rectTransform;
    private Transform followTarget;
    private Vector3 worldOffset;
    private SpriteRenderer followSpriteRenderer;
    private float followPadding;
    private Camera mainCamera;
    private bool useWorldSpace;
    private bool worldSpaceConfigured;

    public Vector2 WorldDisplaySize => worldDisplaySize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void SetWorldDisplaySize(Vector2 size)
    {
        worldDisplaySize = size;
        ApplyWorldDisplaySize();
    }

    /// <summary>Gán mục tiêu world-space để thanh máu bám theo (ví dụ: đầu Boss).</summary>
    public void SetFollowTarget(Transform target, Vector3 offset)
    {
        followTarget = target;
        worldOffset = offset;
        followSpriteRenderer = null;
        followPadding = 0f;

        ConfigureRenderMode();
    }

    /// <summary>Gán mục tiêu và tự tính offset theo chiều cao sprite (cập nhật khi boss đổi scale/phase).</summary>
    public void SetFollowTarget(Transform target, SpriteRenderer spriteRenderer, float padding)
    {
        followTarget = target;
        followSpriteRenderer = spriteRenderer;
        followPadding = padding;
        worldOffset = Vector3.zero;

        ConfigureRenderMode();
    }

    public void ClearFollowTarget()
    {
        followTarget = null;
        followSpriteRenderer = null;
    }

    public void Show()
    {
        ConfigureRenderMode();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        ClearFollowTarget();
        gameObject.SetActive(false);
    }

    public void SetMaxHealth(float health)
    {
        if (slider == null)
        {
            Debug.LogWarning("[HealthBar] Slider chưa được gán.");
            return;
        }

        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(float health)
    {
        if (slider == null)
        {
            return;
        }

        slider.value = health;
    }

    private void ConfigureRenderMode()
    {
        if (followTarget == null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.WorldSpace)
        {
            useWorldSpace = false;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            return;
        }

        useWorldSpace = true;
        EnsureWorldSpaceCanvas();
        NormalizeChildLayout();
        ApplyWorldDisplaySize();

        // Không parent vào boss để tránh bị lật theo localScale.x khi boss quay trái/phải.
        if (transform.parent == followTarget)
        {
            transform.SetParent(null, true);
        }
    }

    private void EnsureWorldSpaceCanvas()
    {
        if (worldSpaceConfigured)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = worldSpaceSortingOrder;

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        worldSpaceConfigured = true;
    }

    private void ApplyWorldDisplaySize()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.sizeDelta = worldDisplaySize;
        transform.localScale = Vector3.one;
    }

    /// <summary>Ép các phần tử con fill full khung, tránh Image giữ native size 500px.</summary>
    private void NormalizeChildLayout()
    {
        Transform background = transform.Find("Background");
        if (background != null && background.TryGetComponent(out RectTransform backgroundRect))
        {
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = Vector2.zero;

            if (background.TryGetComponent(out Image backgroundImage))
            {
                backgroundImage.preserveAspect = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (followTarget == null || rectTransform == null)
        {
            return;
        }

        Vector3 offset = worldOffset;
        if (followSpriteRenderer != null)
        {
            offset.y = followSpriteRenderer.bounds.extents.y + followPadding;
        }

        if (useWorldSpace)
        {
            Vector3 worldPos = followTarget.position + offset;
            if (followSpriteRenderer != null)
            {
                worldPos.x = followSpriteRenderer.bounds.center.x;
                worldPos.y = followSpriteRenderer.bounds.max.y + followPadding;
            }

            transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            transform.localScale = Vector3.one;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(followTarget.position + offset);

        if (screenPos.z < 0f)
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        rectTransform.position = screenPos;
    }
}

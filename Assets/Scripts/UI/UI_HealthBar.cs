using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerStats))]
public class UI_HealthBar : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] private Sprite portraitSprite;
    [SerializeField] private float portraitSize = 52f;

    [Header("Bar")]
    [SerializeField] private float barWidth = 220f;
    [SerializeField] private float barHeight = 22f;
    [SerializeField] private float gap = 8f;
    [SerializeField] private Vector2 screenOffset = new Vector2(20f, -20f);
    [SerializeField] [Range(0f, 11f)] private float cornerRadius = 10f;

    [Header("Colors")]
    [SerializeField] private Color bgColor = new Color(0.06f, 0.03f, 0.10f, 0.85f);
    [SerializeField] private Color fillColor = new Color(0.75f, 0.25f, 0.35f, 1f);
    [SerializeField] private Color lowHpColor = new Color(0.90f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color borderColor = new Color(0.35f, 0.20f, 0.50f, 0.9f);

    [Header("Low HP threshold")]
    [SerializeField] [Range(0f, 1f)] private float lowHpPercent = 0.3f;

    private PlayerStats stats;

    private GameObject canvasObject;
    private RectTransform fillRect;
    private Image fillImage;

    private void Awake()
    {
        if (portraitSprite == null)
        {
            portraitSprite = Resources.Load<Sprite>("WitchHead");
        }

        BuildUi();
    }

    private void Start()
    {
        stats = GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("[UI_HealthBar] PlayerStats not found on " + gameObject.name);
            return;
        }

        stats.StatsChanged += RefreshBar;
        RefreshBar();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.StatsChanged += RefreshBar;
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.StatsChanged -= RefreshBar;
        }
    }

    private void OnDestroy()
    {
        if (canvasObject != null)
        {
            Destroy(canvasObject);
        }
    }

    private void BuildUi()
    {
        canvasObject = new GameObject("HealthBarCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        Sprite roundedSprite = CreateRoundedSprite();
        float border = 2f;

        float totalWidth = portraitSize + gap + barWidth;
        float totalHeight = Mathf.Max(portraitSize, barHeight + border * 2f);

        // Root
        GameObject root = new GameObject("HPRoot");
        root.transform.SetParent(canvasObject.transform, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = screenOffset;
        rootRect.sizeDelta = new Vector2(totalWidth, totalHeight);

        // Portrait
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(root.transform, false);

        RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0f, 0.5f);
        portraitRect.anchorMax = new Vector2(0f, 0.5f);
        portraitRect.pivot = new Vector2(0f, 0.5f);
        portraitRect.anchoredPosition = Vector2.zero;
        portraitRect.sizeDelta = new Vector2(portraitSize, portraitSize);

        Image portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        if (portraitSprite != null) portraitImage.sprite = portraitSprite;
        else portraitImage.color = new Color(0.3f, 0.15f, 0.4f, 1f);

        // Bar border
        float outerH = barHeight + border * 2f;
        GameObject bgObj = new GameObject("BarBorder");
        bgObj.transform.SetParent(root.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.5f);
        bgRect.anchorMax = new Vector2(0f, 0.5f);
        bgRect.pivot = new Vector2(0f, 0.5f);
        bgRect.anchoredPosition = new Vector2(portraitSize + gap, 0f);
        bgRect.sizeDelta = new Vector2(barWidth, outerH);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = borderColor;
        bgImage.sprite = roundedSprite;
        bgImage.type = Image.Type.Sliced;

        // Bar inner bg
        GameObject innerBgObj = new GameObject("BarBg");
        innerBgObj.transform.SetParent(bgObj.transform, false);

        RectTransform innerBgRect = innerBgObj.AddComponent<RectTransform>();
        innerBgRect.anchorMin = Vector2.zero;
        innerBgRect.anchorMax = Vector2.one;
        innerBgRect.offsetMin = new Vector2(border, border);
        innerBgRect.offsetMax = new Vector2(-border, -border);

        Image innerBgImage = innerBgObj.AddComponent<Image>();
        innerBgImage.color = bgColor;
        innerBgImage.sprite = roundedSprite;
        innerBgImage.type = Image.Type.Sliced;

        // Fill
        GameObject fillObj = new GameObject("HPFill");
        fillObj.transform.SetParent(innerBgObj.transform, false);

        fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.sprite = roundedSprite;
        fillImage.type = Image.Type.Sliced;
    }

    private void RefreshBar()
    {
        if (stats == null || fillRect == null)
        {
            return;
        }

        float current = stats.CurrentHealth;
        float max = stats.MaxHealth;
        float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        fillRect.anchorMax = new Vector2(ratio, 1f);
        fillRect.offsetMax = Vector2.zero;
        fillImage.color = ratio <= lowHpPercent ? lowHpColor : fillColor;
    }

    private Sprite CreateRoundedSprite()
    {
        int r = Mathf.RoundToInt(cornerRadius * 4);
        r = Mathf.Max(r, 4);
        int size = r * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;

                if (x < r && y < r)
                    inside = Dist(x, y, r, r) <= r;
                else if (x >= size - r && y < r)
                    inside = Dist(x, y, size - r - 1, r) <= r;
                else if (x < r && y >= size - r)
                    inside = Dist(x, y, r, size - r - 1) <= r;
                else if (x >= size - r && y >= size - r)
                    inside = Dist(x, y, size - r - 1, size - r - 1) <= r;

                tex.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }

    private float Dist(float x1, float y1, float x2, float y2)
    {
        float dx = x1 - x2;
        float dy = y1 - y2;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}

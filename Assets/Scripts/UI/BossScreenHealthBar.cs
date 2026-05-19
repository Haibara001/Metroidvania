using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Enemy_MiniBoss))]
public class BossScreenHealthBar : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 rootSize = new Vector2(2000f, 64f);
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 36f);
    [SerializeField] private float leftPadding = 20f;
    [SerializeField] private float rightPadding = 20f;
    [SerializeField] private float nameWidth = 180f;
    [SerializeField] private float nameGap = 1f;
    [SerializeField] private float barHeight = 24f;
    [SerializeField] private float borderSize = 3f;
    [SerializeField] private int nameFontSize = 32;
    [SerializeField] private Font nameFont;

    [Header("Colors")]
    [SerializeField] private Color borderColor = new Color(0.55f, 0.42f, 0.2f, 1f);
    [SerializeField] private Color bgColor = new Color(0.18f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color fillColor = new Color(0.82f, 0.18f, 0.24f, 1f);
    [SerializeField] private Color nameColor = new Color(1f, 0.92f, 0.72f, 1f);

    private Enemy_MiniBoss boss;
    private GameObject canvasObject;
    private RectTransform fillRect;
    private Text nameText;
    private float lastHp = -1f;

    private bool revealed;

    private void Awake()
    {
        boss = GetComponent<Enemy_MiniBoss>();
        BuildUi();
        canvasObject.SetActive(false);
    }

    public void Reveal()
    {
        revealed = true;
        canvasObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (boss == null || canvasObject == null || !revealed)
        {
            return;
        }

        if (boss.IsDead())
        {
            canvasObject.SetActive(false);
            return;
        }

        if (!canvasObject.activeSelf)
        {
            canvasObject.SetActive(true);
        }

        if (nameText != null)
        {
            nameText.text = boss.BossDisplayName;
        }

        float hp = Mathf.Clamp01(boss.HealthPercent);
        if (Mathf.Abs(hp - lastHp) > 0.001f)
        {
            lastHp = hp;
            fillRect.anchorMax = new Vector2(hp, 1f);
            fillRect.offsetMax = Vector2.zero;
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
        canvasObject = new GameObject("BossHealthBarCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = nameFont != null ? nameFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject root = new GameObject("BossHealthRoot");
        root.transform.SetParent(canvasObject.transform, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.anchoredPosition = screenOffset;
        rootRect.sizeDelta = rootSize;

        GameObject bossName = new GameObject("BossName");
        bossName.transform.SetParent(root.transform, false);
        RectTransform nameRect = bossName.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(0f, 1f);
        nameRect.pivot = new Vector2(0f, 0.5f);
        nameRect.anchoredPosition = new Vector2(leftPadding, 0f);
        nameRect.sizeDelta = new Vector2(nameWidth, 0f);

        nameText = bossName.AddComponent<Text>();
        nameText.font = font;
        nameText.fontSize = nameFontSize;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.color = nameColor;
        nameText.text = boss != null ? boss.BossDisplayName : "Boss";

        float barWidth = rootSize.x - leftPadding - nameWidth - nameGap - rightPadding;

        GameObject border = new GameObject("BarBorder");
        border.transform.SetParent(root.transform, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0.5f);
        borderRect.anchorMax = new Vector2(0f, 0.5f);
        borderRect.pivot = new Vector2(0f, 0.5f);
        borderRect.anchoredPosition = new Vector2(leftPadding + nameWidth + nameGap, 0f);
        borderRect.sizeDelta = new Vector2(barWidth, barHeight);

        Image borderImage = border.AddComponent<Image>();
        borderImage.color = borderColor;

        GameObject barBg = new GameObject("BarBg");
        barBg.transform.SetParent(border.transform, false);
        RectTransform bgRect = barBg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(borderSize, borderSize);
        bgRect.offsetMax = new Vector2(-borderSize, -borderSize);

        Image bgImage = barBg.AddComponent<Image>();
        bgImage.color = bgColor;

        GameObject fill = new GameObject("BarFill");
        fill.transform.SetParent(barBg.transform, false);
        fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
    }
}

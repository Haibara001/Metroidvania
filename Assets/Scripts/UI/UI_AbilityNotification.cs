using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_AbilityNotification : MonoBehaviour
{
    [Header("Font")]
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("Text")]
    [SerializeField] private string titleText = "NEW ABILITY UNLOCKED";
    [SerializeField] private string dashText = "Shadow Dash";
    [SerializeField] private string doubleJumpText = "Double Jump";
    [SerializeField] private string wallJumpText = "Wall Jump";
    [SerializeField] private string airDashText = "Air Dash";
    [SerializeField] private string echoSwapText = "Echo Swap";
    [SerializeField] private string rangedAttackText = "Ranged Attack";

    [Header("Panel")]
    [SerializeField] private Vector2 panelSize = new Vector2(360f, 100f);
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, -100f);

    [Header("Title")]
    [SerializeField] private float titleFontSize = 14f;
    [SerializeField] private Vector2 titlePosition = new Vector2(10f, -5f);
    [SerializeField] private Vector2 titleSize = new Vector2(340f, 35f);

    [Header("Ability Name")]
    [SerializeField] private float abilityFontSize = 20f;
    [SerializeField] private Vector2 abilityPosition = new Vector2(10f, 2f);
    [SerializeField] private Vector2 abilitySize = new Vector2(340f, 38f);

    [Header("Description")]
    [SerializeField] private float descriptionFontSize = 12f;
    [SerializeField] private Vector2 descriptionPosition = new Vector2(10f, -15f);
    [SerializeField] private Vector2 descriptionSize = new Vector2(340f, 30f);

    [Header("Display")]
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Colors")]
    [SerializeField] private Color bgColor = new Color(0.06f, 0.03f, 0.10f, 0.90f);
    [SerializeField] private Color borderColor = new Color(0.60f, 0.40f, 0.85f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.95f, 0.90f, 1.0f, 1f);
    [SerializeField] private Color accentColor = new Color(0.80f, 0.55f, 1.0f, 1f);

    private GameObject canvasObject;
    private GameObject notifPanel;
    private TMP_Text titleLabel;
    private TMP_Text abilityLabel;
    private TMP_Text descriptionLabel;
    private CanvasGroup canvasGroup;
    private Coroutine activeNotif;

    private static UI_AbilityNotification instance;

    private void Awake()
    {
        instance = this;
        BuildUi();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (canvasObject != null)
        {
            Destroy(canvasObject);
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    public static void Show(PlayerAbilityType ability, string description = null)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("AbilityNotification");
            instance = go.AddComponent<UI_AbilityNotification>();
        }

        instance.ShowInternal(ability, description);
    }

    private void ShowInternal(PlayerAbilityType ability, string description)
    {
        if (activeNotif != null)
        {
            StopCoroutine(activeNotif);
        }

        titleLabel.text = titleText;
        abilityLabel.text = GetAbilityName(ability);

        if (descriptionLabel != null)
        {
            descriptionLabel.text = description ?? string.Empty;
            descriptionLabel.gameObject.SetActive(!string.IsNullOrEmpty(description));
        }

        activeNotif = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetVisible(true);
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        SetVisible(false);
        activeNotif = null;
    }

    private void SetVisible(bool visible)
    {
        if (notifPanel != null)
        {
            notifPanel.SetActive(visible);
        }
    }

    private void BuildUi()
    {
        canvasObject = new GameObject("AbilityNotifCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Notification panel
        notifPanel = new GameObject("NotifPanel");
        notifPanel.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = notifPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = screenOffset;
        panelRect.sizeDelta = panelSize;

        Image panelBg = notifPanel.AddComponent<Image>();
        panelBg.color = bgColor;

        canvasGroup = notifPanel.AddComponent<CanvasGroup>();

        // Top accent line
        GameObject accentLine = new GameObject("AccentLine");
        accentLine.transform.SetParent(notifPanel.transform, false);

        RectTransform accentRect = accentLine.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0f, 3f);

        Image accentImage = accentLine.AddComponent<Image>();
        accentImage.color = accentColor;

        // Title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(notifPanel.transform, false);

        titleLabel = titleObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) titleLabel.font = fontAsset;
        titleLabel.fontSize = titleFontSize;
        titleLabel.fontStyle = FontStyles.Normal;
        titleLabel.alignment = TextAlignmentOptions.Center;
        titleLabel.color = accentColor;

        RectTransform titleRect = titleLabel.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(0f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.anchoredPosition = titlePosition;
        titleRect.sizeDelta = titleSize;

        // Ability name text
        GameObject abilityObj = new GameObject("AbilityName");
        abilityObj.transform.SetParent(notifPanel.transform, false);

        abilityLabel = abilityObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) abilityLabel.font = fontAsset;
        abilityLabel.fontSize = abilityFontSize;
        abilityLabel.fontStyle = FontStyles.Bold;
        abilityLabel.alignment = TextAlignmentOptions.Center;
        abilityLabel.color = textColor;

        RectTransform abilityRect = abilityLabel.rectTransform;
        abilityRect.anchorMin = new Vector2(0f, 0f);
        abilityRect.anchorMax = new Vector2(0f, 0f);
        abilityRect.pivot = new Vector2(0f, 0f);
        abilityRect.anchoredPosition = abilityPosition;
        abilityRect.sizeDelta = abilitySize;

        // Description text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(notifPanel.transform, false);

        descriptionLabel = descObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) descriptionLabel.font = fontAsset;
        descriptionLabel.fontSize = descriptionFontSize;
        descriptionLabel.fontStyle = FontStyles.Normal;
        descriptionLabel.alignment = TextAlignmentOptions.Center;
        descriptionLabel.color = new Color(textColor.r, textColor.g, textColor.b, 0.7f);

        RectTransform descRect = descriptionLabel.rectTransform;
        descRect.anchorMin = new Vector2(0f, 0f);
        descRect.anchorMax = new Vector2(0f, 0f);
        descRect.pivot = new Vector2(0f, 0f);
        descRect.anchoredPosition = descriptionPosition;
        descRect.sizeDelta = descriptionSize;
    }

    private string GetAbilityName(PlayerAbilityType ability)
    {
        switch (ability)
        {
            case PlayerAbilityType.Dash: return dashText;
            case PlayerAbilityType.DoubleJump: return doubleJumpText;
            case PlayerAbilityType.WallJump: return wallJumpText;
            case PlayerAbilityType.AirDash: return airDashText;
            case PlayerAbilityType.EchoSwap: return echoSwapText;
            case PlayerAbilityType.RangedAttack: return rangedAttackText;
            default: return ability.ToString();
        }
    }
}

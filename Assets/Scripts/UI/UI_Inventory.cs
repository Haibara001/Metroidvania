using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerEquipment))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerProgression))]
public class UI_Inventory : MonoBehaviour
{
    public static bool IsMenuVisible { get; private set; }

    private enum PlayerMenuTab
    {
        Status,
        Inventory,
        Equipment,
        Settings
    }

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    [Header("Fonts")]
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private TMP_FontAsset titleFont;
    [SerializeField] private Material uiFontMaterial;

    [Header("Text Templates")]
    [SerializeField] private string menuTitleText = "✦ Witch's Grimoire ✦";
    [SerializeField] private string closeHintTemplate = "Press {0} to close";
    [SerializeField] private string statusTabText = "Status";
    [SerializeField] private string inventoryTabText = "Inventory";
    [SerializeField] private string equipmentTabText = "Equipment";
    [SerializeField] private string overviewHeaderText = "Overview";
    [SerializeField] private string statsHeaderText = "Stats";
    [SerializeField] private string bagHeaderText = "Bag";
    [SerializeField] private string equipmentHeaderText = "Equipment";
    [SerializeField] private string detailsHeaderText = "Details";
    [SerializeField] private string bagEmptyText = "Bag is empty";
    [SerializeField] private string emptyEquippedSlotText = "Empty";
    [SerializeField] private string noItemSelectedText = "No item selected.";
    [SerializeField] private string defaultDescriptionText = "Pick up gear, then equip the item you want to use.";
    [SerializeField] private string inspectButtonText = "Inspect";
    [SerializeField] private string equipButtonText = "Equip";
    [SerializeField] private string unequipButtonText = "Unequip";
    [SerializeField] private string settingsTabText = "Settings";
    [SerializeField] private string saveSlotHeaderText = "Save Slots";
    [SerializeField] private string saveButtonText = "Save";
    [SerializeField] private string loadButtonText = "Load";
    [SerializeField] private string deleteButtonText = "Delete";
    [SerializeField] private string emptySlotText = "Empty";
    [SerializeField] private string saveSuccessText = "Save successful!";
    [SerializeField] private string loadSuccessText = "Load successful!";
    [SerializeField] private string deleteSuccessText = "Save deleted.";
    [SerializeField] private string noSaveToLoadText = "No save data in this slot.";

    [Header("Equipment Slot Labels")]
    [SerializeField] private string charmSlotText = "Charm";
    [SerializeField] private string relicSlotText = "Relic";

    [Header("Colors - Witch Theme / 魔女风格")]
    [SerializeField] private Color panelColor = new Color(0.04f, 0.02f, 0.08f, 0.97f);           // 深紫黑
    [SerializeField] private Color sectionColor = new Color(0.08f, 0.04f, 0.14f, 0.95f);        // 深紫
    [SerializeField] private Color rowColor = new Color(0.10f, 0.05f, 0.18f, 0.93f);             // 中紫
    [SerializeField] private Color textColor = new Color(0.92f, 0.88f, 1.0f, 1f);                // 冰蓝白
    [SerializeField] private Color tabActiveColor = new Color(0.70f, 0.50f, 0.95f, 1f);          // 明亮紫
    [SerializeField] private Color tabInactiveColor = new Color(0.35f, 0.25f, 0.50f, 1f);        // 暗紫
    [SerializeField] private Color buttonNormalColor = new Color(0.50f, 0.35f, 0.75f, 0.9f);     // 紫罗兰
    [SerializeField] private Color buttonHighlightColor = new Color(0.70f, 0.50f, 0.95f, 1f);   // 亮紫
    [SerializeField] private Color buttonPressedColor = new Color(0.40f, 0.25f, 0.65f, 1f);      // 深紫
    [SerializeField] private Color buttonDisabledColor = new Color(0.15f, 0.10f, 0.22f, 0.7f);   // 极暗紫

    private PlayerInventory inventory;
    private PlayerEquipment equipment;
    private PlayerStats stats;
    private PlayerProgression progression;

    private GameObject canvasObject;
    private GameObject panelObject;
    private GameObject statusPage;
    private GameObject inventoryPage;
    private GameObject equipmentPage;
    private GameObject settingsPage;

    private RectTransform inventoryContent;
    private RectTransform equippedContent;

    private TMP_Text statusSummaryText;
    private TMP_Text statusStatsText;
    private TMP_Text inventoryDescriptionText;
    private TMP_Text equipmentDescriptionText;

    private Button statusTabButton;
    private Button inventoryTabButton;
    private Button equipmentTabButton;
    private Button settingsTabButton;

    private TMP_Text settingsStatusText;
    private Player player;

    private PlayerMenuTab currentTab = PlayerMenuTab.Status;

    // ── 拖动相关变量 ──────────────────────────────────────────────────
    private RectTransform panelRectTransform;
    private Vector2 initialPanelPosition;
    private static bool suppressGameplayMouseUntilRelease;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        equipment = GetComponent<PlayerEquipment>();
        stats = GetComponent<PlayerStats>();
        progression = GetComponent<PlayerProgression>();
        player = GetComponent<Player>();

        // 确保应用魔女风格颜色
        InitializeWitchThemeColors();

        EnsureEventSystemExists();
        BuildUi();
        RefreshUi();
        ShowTab(PlayerMenuTab.Status);
        SetMenuVisible(false);
    }

    private void InitializeWitchThemeColors()
    {
        // 强制应用魔女风格颜色（深紫黑系）
        panelColor            = new Color(0.04f, 0.02f, 0.08f, 0.97f);           // 深紫黑
        sectionColor          = new Color(0.08f, 0.04f, 0.14f, 0.95f);          // 深紫
        rowColor              = new Color(0.10f, 0.05f, 0.18f, 0.93f);           // 中紫
        textColor             = new Color(0.92f, 0.88f, 1.0f, 1f);               // 冰蓝白
        tabActiveColor        = new Color(0.70f, 0.50f, 0.95f, 1f);              // 明亮紫
        tabInactiveColor      = new Color(0.35f, 0.25f, 0.50f, 1f);              // 暗紫
        buttonNormalColor     = new Color(0.50f, 0.35f, 0.75f, 0.9f);            // 紫罗兰
        buttonHighlightColor  = new Color(0.70f, 0.50f, 0.95f, 1f);             // 亮紫
        buttonPressedColor    = new Color(0.40f, 0.25f, 0.65f, 1f);              // 深紫
        buttonDisabledColor   = new Color(0.15f, 0.10f, 0.22f, 0.7f);            // 极暗紫
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged += RefreshUi;
        }

        if (equipment != null)
        {
            equipment.EquipmentChanged += RefreshUi;
        }

        if (stats != null)
        {
            stats.StatsChanged += RefreshUi;
        }

        if (progression != null)
        {
            progression.ProgressionChanged += RefreshUi;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= RefreshUi;
        }

        if (equipment != null)
        {
            equipment.EquipmentChanged -= RefreshUi;
        }

        if (stats != null)
        {
            stats.StatsChanged -= RefreshUi;
        }

        if (progression != null)
        {
            progression.ProgressionChanged -= RefreshUi;
        }
    }

    private void OnDestroy()
    {
        if (canvasObject != null)
        {
            Destroy(canvasObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void OpenInventory()
    {
        SetMenuVisible(true);
        ShowTab(PlayerMenuTab.Inventory);
    }

    public void OpenStatus()
    {
        SetMenuVisible(true);
        ShowTab(PlayerMenuTab.Status);
    }

    public void OpenEquipment()
    {
        SetMenuVisible(true);
        ShowTab(PlayerMenuTab.Equipment);
    }

    public void OpenSettings()
    {
        SetMenuVisible(true);
        ShowTab(PlayerMenuTab.Settings);
    }

    public void CloseInventory()
    {
        SetMenuVisible(false);
    }

    public void ToggleInventory()
    {
        bool nextVisible = panelObject == null || !panelObject.activeSelf;
        SetMenuVisible(nextVisible);

        if (nextVisible)
        {
            ShowTab(currentTab);
        }
    }

    private void BuildUi()
    {
        canvasObject = new GameObject("PlayerMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform root = canvas.GetComponent<RectTransform>();
        panelObject = CreatePanel("PlayerMenuPanel", root, panelColor, new Vector2(1040f, 720f)).gameObject;

        panelRectTransform = panelObject.GetComponent<RectTransform>();
        RectTransform panel = panelRectTransform;
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        initialPanelPosition = panel.anchoredPosition;

        // 为面板添加拖动功能
        panelObject.AddComponent<GraphicRaycaster>();
        panelObject.AddComponent<CanvasGroup>();

        // 创建拖动句柄（在标题区域）
        GameObject dragHandle = new GameObject("DragHandle");
        dragHandle.transform.SetParent(panel, false);
        RectTransform dragRect = dragHandle.AddComponent<RectTransform>();
        dragRect.anchorMin = Vector2.zero;
        dragRect.anchorMax = new Vector2(1f, 1f);
        dragRect.offsetMin = Vector2.zero;
        dragRect.offsetMax = Vector2.zero;
        
        Image dragImage = dragHandle.AddComponent<Image>();
        dragImage.color = Color.clear; // 透明，不可见
        dragImage.raycastTarget = true;
        
        // 添加拖动脚本组件
        UIDragHandler dragHandler = dragHandle.AddComponent<UIDragHandler>();
        dragHandler.SetTargetPanel(panelRectTransform);

        CreateText("Title", panel, menuTitleText, 40, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(40f, -32f), new Vector2(320f, 44f), fontOverride: titleFont);
        CreateText("Hint", panel, string.Format(closeHintTemplate, toggleKey), 20, FontStyles.Normal, TextAlignmentOptions.Right, new Vector2(-40f, -34f), new Vector2(220f, 32f), new Vector2(1f, 1f));

        BuildTabBar(panel);

        statusPage = CreatePageRoot("StatusPage", panel, new Vector2(40f, -176f), new Vector2(960f, 500f));
        inventoryPage = CreatePageRoot("InventoryPage", panel, new Vector2(40f, -176f), new Vector2(960f, 500f));
        equipmentPage = CreatePageRoot("EquipmentPage", panel, new Vector2(40f, -176f), new Vector2(960f, 500f));
        settingsPage = CreatePageRoot("SettingsPage", panel, new Vector2(40f, -176f), new Vector2(960f, 500f));

        BuildStatusPage(statusPage.transform);
        BuildInventoryPage(inventoryPage.transform);
        BuildEquipmentPage(equipmentPage.transform);
        BuildSettingsPage(settingsPage.transform);
    }

    private void BuildTabBar(RectTransform panel)
    {
        RectTransform tabBar = CreatePanel("TabBar", panel, sectionColor, new Vector2(960f, 64f));
        tabBar.anchorMin = new Vector2(0.5f, 1f);
        tabBar.anchorMax = new Vector2(0.5f, 1f);
        tabBar.pivot = new Vector2(0.5f, 1f);
        tabBar.anchoredPosition = new Vector2(0f, -92f);

        statusTabButton = CreateButton("StatusTab", tabBar, statusTabText, new Vector2(20f, -12f), new Vector2(180f, 40f), new Vector2(0f, 1f), tabInactiveColor);
        inventoryTabButton = CreateButton("InventoryTab", tabBar, inventoryTabText, new Vector2(220f, -12f), new Vector2(220f, 40f), new Vector2(0f, 1f), tabInactiveColor);
        equipmentTabButton = CreateButton("EquipmentTab", tabBar, equipmentTabText, new Vector2(460f, -12f), new Vector2(220f, 40f), new Vector2(0f, 1f), tabInactiveColor);
        settingsTabButton = CreateButton("SettingsTab", tabBar, settingsTabText, new Vector2(700f, -12f), new Vector2(220f, 40f), new Vector2(0f, 1f), tabInactiveColor);

        statusTabButton.onClick.AddListener(() => ShowTab(PlayerMenuTab.Status));
        inventoryTabButton.onClick.AddListener(() => ShowTab(PlayerMenuTab.Inventory));
        equipmentTabButton.onClick.AddListener(() => ShowTab(PlayerMenuTab.Equipment));
        settingsTabButton.onClick.AddListener(() => ShowTab(PlayerMenuTab.Settings));
    }

    private void BuildStatusPage(Transform parent)
    {
        RectTransform summaryPanel = CreatePanel("SummaryPanel", parent, sectionColor, new Vector2(300f, 420f));
        summaryPanel.anchorMin = new Vector2(0f, 1f);
        summaryPanel.anchorMax = new Vector2(0f, 1f);
        summaryPanel.pivot = new Vector2(0f, 1f);
        summaryPanel.anchoredPosition = new Vector2(0f, 0f);

        RectTransform statsPanel = CreatePanel("StatsPanel", parent, sectionColor, new Vector2(620f, 420f));
        statsPanel.anchorMin = new Vector2(1f, 1f);
        statsPanel.anchorMax = new Vector2(1f, 1f);
        statsPanel.pivot = new Vector2(1f, 1f);
        statsPanel.anchoredPosition = new Vector2(0f, 0f);

        CreateText("SummaryHeader", summaryPanel, overviewHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));
        CreateText("StatsHeader", statsPanel, statsHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));

        statusSummaryText = CreateText("SummaryText", summaryPanel, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Vector2(20f, -66f), new Vector2(260f, 320f));
        statusStatsText = CreateText("StatsText", statsPanel, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Vector2(20f, -66f), new Vector2(580f, 320f));
    }

    private void BuildInventoryPage(Transform parent)
    {
        RectTransform inventoryPanel = CreatePanel("InventoryListPanel", parent, sectionColor, new Vector2(620f, 500f));
        inventoryPanel.anchorMin = new Vector2(0f, 1f);
        inventoryPanel.anchorMax = new Vector2(0f, 1f);
        inventoryPanel.pivot = new Vector2(0f, 1f);
        inventoryPanel.anchoredPosition = new Vector2(0f, 0f);

        RectTransform descriptionPanel = CreatePanel("InventoryDescriptionPanel", parent, sectionColor, new Vector2(300f, 500f));
        descriptionPanel.anchorMin = new Vector2(1f, 1f);
        descriptionPanel.anchorMax = new Vector2(1f, 1f);
        descriptionPanel.pivot = new Vector2(1f, 1f);
        descriptionPanel.anchoredPosition = new Vector2(0f, 0f);

        CreateText("InventoryHeader", inventoryPanel, bagHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));
        CreateText("DescriptionHeader", descriptionPanel, detailsHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));

        inventoryContent = CreateContentArea("InventoryContent", inventoryPanel, new Vector2(20f, 20f), new Vector2(-20f, -62f));
        inventoryDescriptionText = CreateText("InventoryDescription", descriptionPanel, defaultDescriptionText, 20, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Vector2(20f, -66f), new Vector2(260f, 400f));
    }

    private void BuildEquipmentPage(Transform parent)
    {
        RectTransform equippedPanel = CreatePanel("EquippedPanel", parent, sectionColor, new Vector2(620f, 500f));
        equippedPanel.anchorMin = new Vector2(0f, 1f);
        equippedPanel.anchorMax = new Vector2(0f, 1f);
        equippedPanel.pivot = new Vector2(0f, 1f);
        equippedPanel.anchoredPosition = new Vector2(0f, 0f);

        RectTransform descriptionPanel = CreatePanel("EquipmentDescriptionPanel", parent, sectionColor, new Vector2(300f, 500f));
        descriptionPanel.anchorMin = new Vector2(1f, 1f);
        descriptionPanel.anchorMax = new Vector2(1f, 1f);
        descriptionPanel.pivot = new Vector2(1f, 1f);
        descriptionPanel.anchoredPosition = new Vector2(0f, 0f);

        CreateText("EquippedHeader", equippedPanel, equipmentHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));
        CreateText("DescriptionHeader", descriptionPanel, detailsHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));

        equippedContent = CreateContentArea("EquippedContent", equippedPanel, new Vector2(20f, 20f), new Vector2(-20f, -62f));
        equipmentDescriptionText = CreateText("EquipmentDescription", descriptionPanel, defaultDescriptionText, 20, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Vector2(20f, -66f), new Vector2(260f, 400f));
    }

    private void BuildSettingsPage(Transform parent)
    {
        RectTransform settingsPanel = CreatePanel("SettingsPanel", parent, sectionColor, new Vector2(960f, 500f));
        settingsPanel.anchorMin = new Vector2(0f, 1f);
        settingsPanel.anchorMax = new Vector2(0f, 1f);
        settingsPanel.pivot = new Vector2(0f, 1f);
        settingsPanel.anchoredPosition = new Vector2(0f, 0f);

        CreateText("SettingsHeader", settingsPanel, saveSlotHeaderText, 28, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(20f, -18f), new Vector2(220f, 32f));

        settingsStatusText = CreateText("SettingsStatus", settingsPanel, string.Empty, 20, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(20f, -440f), new Vector2(600f, 32f));

        for (int slot = 0; slot < 3; slot++)
        {
            CreateSaveSlotRow(settingsPanel.transform, slot);
        }
    }

    private void CreateSaveSlotRow(Transform parent, int slot)
    {
        float yOffset = -70f - slot * 120f;

        RectTransform row = CreatePanel("SaveSlot_" + slot, parent, rowColor, new Vector2(900f, 100f));
        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(0f, 1f);
        row.pivot = new Vector2(0f, 1f);
        row.anchoredPosition = new Vector2(30f, yOffset);

        string slotLabel = "Slot " + (slot + 1);
        CreateText("SlotLabel", row, slotLabel, 22, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(16f, -12f), new Vector2(120f, 28f));

        string timeInfo = GetSlotInfoText(slot);
        CreateText("SlotInfo", row, timeInfo, 18, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(16f, -46f), new Vector2(400f, 24f));

        Button saveBtn = CreateButton("SaveBtn", row, saveButtonText, new Vector2(-430f, -16f), new Vector2(120f, 36f));
        int capturedSlot = slot;
        saveBtn.onClick.AddListener(() => OnSaveClicked(capturedSlot));

        Button loadBtn = CreateButton("LoadBtn", row, loadButtonText, new Vector2(-290f, -16f), new Vector2(120f, 36f));
        loadBtn.onClick.AddListener(() => OnLoadClicked(capturedSlot));

        Button deleteBtn = CreateButton("DeleteBtn", row, deleteButtonText, new Vector2(-150f, -16f), new Vector2(120f, 36f));
        deleteBtn.onClick.AddListener(() => OnDeleteClicked(capturedSlot));
    }

    private string GetSlotInfoText(int slot)
    {
        if (SaveSystem.Instance == null)
        {
            return emptySlotText;
        }

        string time = SaveSystem.Instance.GetSaveTime(slot);
        return string.IsNullOrEmpty(time) ? emptySlotText : time;
    }

    private void OnSaveClicked(int slot)
    {
        if (SaveSystem.Instance == null || player == null)
        {
            return;
        }

        SaveSystem.Instance.Save(slot, player);
        ShowSettingsStatus(saveSuccessText);
        RefreshSettingsSlots();
    }

    private void OnLoadClicked(int slot)
    {
        if (SaveSystem.Instance == null || player == null)
        {
            return;
        }

        if (!SaveSystem.Instance.HasSave(slot))
        {
            ShowSettingsStatus(noSaveToLoadText);
            return;
        }

        SaveSystem.Instance.Load(slot, player);
        ShowSettingsStatus(loadSuccessText);
        SetMenuVisible(false);
    }

    private void OnDeleteClicked(int slot)
    {
        if (SaveSystem.Instance == null)
        {
            return;
        }

        SaveSystem.Instance.DeleteSave(slot);
        ShowSettingsStatus(deleteSuccessText);
        RefreshSettingsSlots();
    }

    private void ShowSettingsStatus(string message)
    {
        if (settingsStatusText != null)
        {
            settingsStatusText.text = message;
        }
    }

    private void RefreshSettingsSlots()
    {
        if (settingsPage == null)
        {
            return;
        }

        // Rebuild the settings page to update slot info
        ClearChildren(settingsPage.transform);
        BuildSettingsPage(settingsPage.transform);
    }

    private void RefreshUi()
    {
        RefreshStatusPage();
        RefreshInventoryPage();
        RefreshTabVisuals();
    }

    private void RefreshStatusPage()
    {
        if (statusSummaryText == null || statusStatsText == null || stats == null || progression == null)
        {
            return;
        }

        statusSummaryText.text =
            "等级 " + progression.Level + "\n" +
            "经验 " + progression.CurrentExperience + " / " + progression.GetRequiredExperienceForNextLevel() + "\n" +
            "金钱 " + progression.Gold + "\n" +
            "生命值 " + stats.CurrentHealth.ToString("0") + " / " + stats.MaxHealth.ToString("0");

        statusStatsText.text =
            "攻击 " + stats.AttackPower.ToString("0.0") + "\n" +
            "移动速度 " + stats.MoveSpeed.ToString("0.0") + "\n" +
            "跳跃力度 " + stats.JumpForce.ToString("0.0") + "\n" +
            "冲刺速度 " + stats.DashSpeed.ToString("0.0");
    }

    private void RefreshInventoryPage()
    {
        if (inventoryContent == null || equippedContent == null || inventory == null || equipment == null)
        {
            return;
        }

        ClearChildren(inventoryContent);
        ClearChildren(equippedContent);

        IReadOnlyList<EquipmentItemData> items = inventory.Items;
        if (items.Count == 0)
        {
            CreateRowLabel(inventoryContent, "BagEmpty", bagEmptyText);
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                CreateInventoryRow(items[i]);
            }
        }

        Dictionary<EquipmentSlotType, EquipmentItemData> equippedLookup = new Dictionary<EquipmentSlotType, EquipmentItemData>();
        List<PlayerEquipment.EquipmentEntry> equippedEntries = equipment.GetEquippedEntries();

        for (int i = 0; i < equippedEntries.Count; i++)
        {
            equippedLookup[equippedEntries[i].slotType] = equippedEntries[i].itemData;
        }

        EquipmentSlotType[] slotTypes = (EquipmentSlotType[])Enum.GetValues(typeof(EquipmentSlotType));
        for (int i = 0; i < slotTypes.Length; i++)
        {
            EquipmentSlotType slotType = slotTypes[i];
            equippedLookup.TryGetValue(slotType, out EquipmentItemData equippedItem);
            CreateEquippedRow(slotType, equippedItem);
        }
    }

    private void ShowTab(PlayerMenuTab tab)
    {
        currentTab = tab;

        if (statusPage != null)
        {
            statusPage.SetActive(tab == PlayerMenuTab.Status);
        }

        if (inventoryPage != null)
        {
            inventoryPage.SetActive(tab == PlayerMenuTab.Inventory);
        }

        if (equipmentPage != null)
        {
            equipmentPage.SetActive(tab == PlayerMenuTab.Equipment);
        }

        if (settingsPage != null)
        {
            settingsPage.SetActive(tab == PlayerMenuTab.Settings);

            if (tab == PlayerMenuTab.Settings)
            {
                RefreshSettingsSlots();
            }
        }

        RefreshTabVisuals();
    }

    private void RefreshTabVisuals()
    {
        ApplyTabButtonStyle(statusTabButton, currentTab == PlayerMenuTab.Status);
        ApplyTabButtonStyle(inventoryTabButton, currentTab == PlayerMenuTab.Inventory);
        ApplyTabButtonStyle(equipmentTabButton, currentTab == PlayerMenuTab.Equipment);
        ApplyTabButtonStyle(settingsTabButton, currentTab == PlayerMenuTab.Settings);
    }

    private void ApplyTabButtonStyle(Button button, bool isActive)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = isActive ? tabActiveColor : tabInactiveColor;
        }
    }

    private void CreateInventoryRow(EquipmentItemData item)
    {
        RectTransform row = CreateRowContainer(inventoryContent, item != null ? item.itemName : "Empty");

        bool hasIcon = item != null && item.icon != null;
        float textX = 16f;

        if (hasIcon)
        {
            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(row, false);
            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.sizeDelta = new Vector2(56f, 56f);
            iconRect.anchoredPosition = new Vector2(16f, -6f);

            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;

            textX = 84f;
        }

        string itemLabel = item != null && !string.IsNullOrWhiteSpace(item.itemName)
            ? item.itemName
            : "Unnamed Item";

        CreateText("ItemName", row, itemLabel, 22, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(textX, -12f), new Vector2(220f, 28f));
        CreateText("ItemSlot", row, item != null ? GetSlotDisplayName(item.slotType) : "-", 18, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(textX, -40f), new Vector2(180f, 24f));

        Button inspectButton = CreateButton("InspectButton", row, inspectButtonText, new Vector2(-140f, -16f), new Vector2(110f, 36f));
        inspectButton.onClick.AddListener(() => ShowDescription(item));

        Button equipButton = CreateButton("EquipButton", row, equipButtonText, new Vector2(-16f, -16f), new Vector2(110f, 36f));
        equipButton.onClick.AddListener(() => EquipItemFromInventory(item));
    }

    private void CreateEquippedRow(EquipmentSlotType slotType, EquipmentItemData item)
    {
        RectTransform row = CreateRowContainer(equippedContent, slotType.ToString());

        bool hasItem = item != null;
        bool hasIcon = hasItem && item.icon != null;
        float textX = 16f;

        if (hasIcon)
        {
            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(row, false);
            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.sizeDelta = new Vector2(56f, 56f);
            iconRect.anchoredPosition = new Vector2(16f, -8f);

            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;

            textX = 84f;
        }

        string itemLabel = hasItem && !string.IsNullOrWhiteSpace(item.itemName)
            ? item.itemName
            : emptyEquippedSlotText;

        CreateText("EquippedName", row, itemLabel, 20, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(textX, -12f), new Vector2(180f, 28f));

        CreateText("SlotName", row, GetSlotDisplayName(slotType), 16, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(textX, -40f), new Vector2(160f, 24f));

        Button inspectButton = CreateButton("InspectButton", row, inspectButtonText, new Vector2(-140f, -16f), new Vector2(110f, 36f));
        inspectButton.interactable = item != null;
        inspectButton.onClick.AddListener(() => ShowDescription(item));

        Button unequipButton = CreateButton("UnequipButton", row, unequipButtonText, new Vector2(-16f, -16f), new Vector2(110f, 36f));
        unequipButton.interactable = item != null;
        unequipButton.onClick.AddListener(() => UnequipItem(slotType));
    }

    private void EquipItemFromInventory(EquipmentItemData item)
    {
        if (item == null || inventory == null || equipment == null)
        {
            return;
        }

        if (!inventory.RemoveItem(item))
        {
            return;
        }

        EquipmentItemData previousItem = equipment.Equip(item);
        if (previousItem != null)
        {
            inventory.AddItem(previousItem);
        }

        ShowDescription(item);
        ShowTab(PlayerMenuTab.Inventory);
    }

    private void UnequipItem(EquipmentSlotType slotType)
    {
        if (inventory == null || equipment == null)
        {
            return;
        }

        EquipmentItemData item = equipment.Unequip(slotType);
        if (item != null)
        {
            inventory.AddItem(item);
            ShowDescription(item);
            ShowTab(PlayerMenuTab.Inventory);
        }
    }

    private void ShowDescription(EquipmentItemData item)
    {
        string descriptionValue;

        if (item == null)
        {
            descriptionValue = noItemSelectedText;
        }
        else
        {
            string itemLabel = !string.IsNullOrWhiteSpace(item.itemName) ? item.itemName : "Unnamed Item";
            string description = !string.IsNullOrWhiteSpace(item.description) ? item.description : "No description.";

            descriptionValue =
                itemLabel + "\n" +
                GetSlotDisplayName(item.slotType) + "\n" +
                description + "\n\n" +
                "Max HP +" + item.statBonuses.maxHealth.ToString("0.0") + "\n" +
                "Attack +" + item.statBonuses.attackPower.ToString("0.0") + "\n" +
                "Move +" + item.statBonuses.moveSpeed.ToString("0.0") + "\n" +
                "Jump +" + item.statBonuses.jumpForce.ToString("0.0") + "\n" +
                "Dash +" + item.statBonuses.dashSpeed.ToString("0.0");
        }

        if (inventoryDescriptionText != null)
        {
            inventoryDescriptionText.text = descriptionValue;
        }

        if (equipmentDescriptionText != null)
        {
            equipmentDescriptionText.text = descriptionValue;
        }
    }

    private string GetSlotDisplayName(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Charm:
                return charmSlotText;
            case EquipmentSlotType.Relic:
                return relicSlotText;
            default:
                return slotType.ToString();
        }
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        return rect;
    }

    private RectTransform CreateContentArea(string name, RectTransform parent, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject contentObject = new GameObject(name);
        contentObject.transform.SetParent(parent, false);

        RectTransform rect = contentObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rect;
    }

    private RectTransform CreateRowContainer(Transform parent, string name)
    {
        RectTransform row = CreatePanel(name + "Row", parent, rowColor, new Vector2(0f, 100f));
        LayoutElement layoutElement = row.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 100f;
        layoutElement.flexibleWidth = 1f;
        return row;
    }

    private void CreateRowLabel(Transform parent, string name, string value)
    {
        RectTransform row = CreateRowContainer(parent, name);
        CreateText("Label", row, value, 22, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, -22f), new Vector2(320f, 40f), new Vector2(0.5f, 1f));
    }

    private TMP_Text CreateText(
        string name,
        Transform parent,
        string value,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2? anchor = null,
        TMP_FontAsset fontOverride = null)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = fontOverride != null ? fontOverride : uiFont;

        if (uiFontMaterial != null)
        {
            text.fontSharedMaterial = uiFontMaterial;
        }

        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.color = textColor;

        RectTransform rect = text.rectTransform;
        Vector2 anchorValue = anchor ?? new Vector2(0f, 1f);
        rect.anchorMin = anchorValue;
        rect.anchorMax = anchorValue;
        rect.pivot = anchorValue;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Vector2? anchor = null, Color? overrideColor = null)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = overrideColor ?? buttonNormalColor;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        ColorBlock colors = button.colors;
        colors.normalColor = buttonImage.color;
        colors.highlightedColor = buttonHighlightColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonHighlightColor;
        colors.disabledColor = buttonDisabledColor;
        button.colors = colors;

        RectTransform rect = button.GetComponent<RectTransform>();
        Vector2 anchorValue = anchor ?? new Vector2(1f, 1f);
        rect.anchorMin = anchorValue;
        rect.anchorMax = anchorValue;
        rect.pivot = anchorValue;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TMP_Text buttonLabel = CreateText("Label", rect, label, 18, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, size, new Vector2(0.5f, 0.5f));
        buttonLabel.rectTransform.anchorMin = Vector2.zero;
        buttonLabel.rectTransform.anchorMax = Vector2.one;
        buttonLabel.rectTransform.offsetMin = Vector2.zero;
        buttonLabel.rectTransform.offsetMax = Vector2.zero;
        buttonLabel.rectTransform.anchoredPosition = Vector2.zero;
        buttonLabel.rectTransform.sizeDelta = Vector2.zero;

        return button;
    }

    private GameObject CreatePageRoot(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject page = new GameObject(name);
        page.transform.SetParent(parent, false);

        RectTransform rect = page.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return page;
    }

    private void SetMenuVisible(bool visible)
    {
        if (!visible && IsMenuVisible)
        {
            suppressGameplayMouseUntilRelease = Input.GetMouseButton(0);
        }

        IsMenuVisible = visible;

        if (panelObject != null)
        {
            panelObject.SetActive(visible);
            
            // 打开时重置位置到初始位置
            if (visible && panelRectTransform != null)
            {
                panelRectTransform.anchoredPosition = initialPanelPosition;
            }
        }

        // 打开背包时暂停游戏，关闭时恢复
        Time.timeScale = visible ? 0f : 1f;
    }

    public static bool ShouldBlockGameplayInput()
    {
        if (IsMenuVisible)
        {
            return true;
        }

        if (!suppressGameplayMouseUntilRelease)
        {
            return false;
        }

        if (Input.GetMouseButton(0))
        {
            return true;
        }

        suppressGameplayMouseUntilRelease = false;
        return false;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void EnsureEventSystemExists()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}

// ══════════════════════════════════════════════════════════════════════════
//  UI 拖动句柄 - 使背包面板可拖动
// ══════════════════════════════════════════════════════════════════════════
public class UIDragHandler : MonoBehaviour, IDragHandler
{
    private RectTransform targetPanel;

    public void SetTargetPanel(RectTransform panel)
    {
        targetPanel = panel;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetPanel == null)
            return;

        targetPanel.anchoredPosition += eventData.delta;
    }
}

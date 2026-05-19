using TMPro;
using UnityEngine;

public class EquipmentPickup : MonoBehaviour
{
    [SerializeField] private EquipmentItemData equipmentItem;

    [Header("Prompt")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private TMP_FontAsset promptFont;
    [SerializeField] private Material promptFontMaterial;
    [SerializeField] private string promptTemplate = "[{0}] Pick Up\n{1}";
    [SerializeField] private float promptFontSize = 18f;
    [SerializeField] private Color promptColor = Color.white;
    [SerializeField] private bool openInventoryOnPickup = true;
    [SerializeField] private bool addToInventoryOnPickup = true;
    [SerializeField] private bool autoEquipOnPickup;
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector3 promptScale = new Vector3(0.2f, 0.2f, 0.2f);

    private PlayerEquipment nearbyEquipment;
    private PlayerInventory nearbyInventory;
    private UI_Inventory nearbyInventoryUi;
    private GameObject promptObject;
    private TextMeshPro promptText;

    private void Awake()
    {
        CreatePrompt();
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (nearbyInventory == null || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        // Collect ability first if exists
        AbilityPickup abilityPickup = GetComponent<AbilityPickup>();

        if (abilityPickup != null)
        {
            Player player = nearbyEquipment != null ? nearbyEquipment.GetComponent<Player>() : null;

            if (player != null)
            {
                abilityPickup.CollectFromEquipment(player);
            }
        }

        PickupItem();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerEquipment equipment = collision.GetComponent<PlayerEquipment>();
        PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
        UI_Inventory inventoryUi = collision.GetComponent<UI_Inventory>();

        if (equipment == null)
        {
            equipment = collision.GetComponentInParent<PlayerEquipment>();
        }

        if (inventory == null)
        {
            inventory = collision.GetComponentInParent<PlayerInventory>();
        }

        if (inventoryUi == null)
        {
            inventoryUi = collision.GetComponentInParent<UI_Inventory>();
        }

        if (equipmentItem == null || inventory == null)
        {
            return;
        }

        nearbyEquipment = equipment;
        nearbyInventory = inventory;
        nearbyInventoryUi = inventoryUi;
        UpdatePromptLabel();
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = collision.GetComponentInParent<PlayerInventory>();
        }

        if (inventory == null || inventory != nearbyInventory)
        {
            return;
        }

        nearbyEquipment = null;
        nearbyInventory = null;
        nearbyInventoryUi = null;
        SetPromptVisible(false);
    }

    private void PickupItem()
    {
        if (equipmentItem == null || nearbyInventory == null)
        {
            return;
        }

        if (addToInventoryOnPickup)
        {
            nearbyInventory.AddItem(equipmentItem);
        }

        if (autoEquipOnPickup && nearbyEquipment != null)
        {
            EquipmentItemData previousItem = nearbyEquipment.Equip(equipmentItem);

            if (addToInventoryOnPickup)
            {
                nearbyInventory.RemoveItem(equipmentItem);

                if (previousItem != null)
                {
                    nearbyInventory.AddItem(previousItem);
                }
            }
        }

        if (openInventoryOnPickup && nearbyInventoryUi != null)
        {
            nearbyInventoryUi.OpenInventory();
        }

        // Trigger AbilityPickup if exists
        AbilityPickup abilityPickup = GetComponent<AbilityPickup>();

        if (abilityPickup != null)
        {
            Player player = nearbyEquipment != null ? nearbyEquipment.GetComponent<Player>() : null;

            if (player != null)
            {
                abilityPickup.CollectFromEquipment(player);
            }
        }

        SetPromptVisible(false);

        if (SFXManager.instance != null)
            SFXManager.instance.PlaySFX(pickupSFX);

        if (destroyAfterPickup)
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.MarkSceneObjectRemoved(this);
            }

            Destroy(gameObject);
        }
    }

    private void CreatePrompt()
    {
        promptObject = new GameObject("PickupPrompt");
        promptObject.transform.SetParent(transform, false);
        promptObject.transform.localPosition = promptOffset;
        promptObject.transform.localRotation = Quaternion.identity;
        promptObject.transform.localScale = promptScale;

        promptText = promptObject.AddComponent<TextMeshPro>();
        if (promptFont != null)
        {
            promptText.font = promptFont;
        }

        if (promptFontMaterial != null)
        {
            promptText.fontSharedMaterial = promptFontMaterial;
        }

        promptText.fontSize = promptFontSize;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = promptColor;
        promptText.enableWordWrapping = false;
        promptText.text = string.Empty;

        MeshRenderer renderer = promptText.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                renderer.sortingLayerID = spriteRenderer.sortingLayerID;
                renderer.sortingOrder = spriteRenderer.sortingOrder + 10;
            }
            else
            {
                renderer.sortingOrder = 50;
            }
        }
    }

    private void UpdatePromptLabel()
    {
        if (promptText == null)
        {
            return;
        }

        string itemName = equipmentItem != null && !string.IsNullOrWhiteSpace(equipmentItem.itemName)
            ? equipmentItem.itemName
            : "Item";

        string template = string.IsNullOrWhiteSpace(promptTemplate)
            ? "[{0}] Pick Up\n{1}"
            : promptTemplate;

        promptText.text = string.Format(template, interactKey, itemName);
    }

    private void SetPromptVisible(bool isVisible)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(isVisible);
        }
    }

    public EquipmentItemData GetEquipmentItem()
    {
        return equipmentItem;
    }
}

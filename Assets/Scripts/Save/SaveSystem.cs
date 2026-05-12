using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    [SerializeField] private List<EquipmentItemData> allEquipmentItems = new List<EquipmentItemData>();

    private const string Reward2ObjectName = "Reward (2)";
    private const PlayerAbilityType Reward2Ability = PlayerAbilityType.EchoSwap;

    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    private static SaveData pendingLoadData;
    private readonly Dictionary<string, EquipmentItemData> itemLookup = new Dictionary<string, EquipmentItemData>();
    private readonly HashSet<string> removedSceneObjectIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildItemLookup();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        removedSceneObjectIds.Clear();

        if (pendingLoadData == null)
        {
            return;
        }

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            ApplyPlayerState(player, pendingLoadData);
            pendingLoadData = null;
        }
    }

    public string GetSlotPath(int slot) => Path.Combine(SaveDirectory, $"save_{slot}.json");

    public bool HasSave(int slot) => File.Exists(GetSlotPath(slot));

    public static bool HasSaveStatic(int slot)
    {
        return File.Exists(GetSlotPathStatic(slot));
    }

    public static int GetMostRecentSaveSlot(int slotCount = 3)
    {
        int bestSlot = -1;
        DateTime bestTime = DateTime.MinValue;

        for (int slot = 0; slot < slotCount; slot++)
        {
            string path = GetSlotPathStatic(slot);

            if (!File.Exists(path))
            {
                continue;
            }

            DateTime lastWriteTime = File.GetLastWriteTime(path);
            if (bestSlot < 0 || lastWriteTime > bestTime)
            {
                bestSlot = slot;
                bestTime = lastWriteTime;
            }
        }

        return bestSlot;
    }

    public static bool LoadFromSlotStatic(int slot)
    {
        string path = GetSlotPathStatic(slot);
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                return false;
            }

            pendingLoadData = data;

            string targetScene = !string.IsNullOrEmpty(data.sceneName)
                ? data.sceneName
                : SceneManager.GetActiveScene().name;

            SceneManager.LoadScene(targetScene);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load save slot " + slot + ": " + e.Message);
            return false;
        }
    }

    public string GetSaveTime(int slot)
    {
        string path = GetSlotPath(slot);
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data?.saveTime ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void Save(int slot, Player player)
    {
        if (player == null)
        {
            return;
        }

        SaveData data = CapturePlayerState(player);
        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string json = JsonUtility.ToJson(data, true);

        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        File.WriteAllText(GetSlotPath(slot), json);
    }

    public bool Load(int slot, Player player)
    {
        return LoadFromSlotStatic(slot);
    }

    public void DeleteSave(int slot)
    {
        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private SaveData CapturePlayerState(Player player)
    {
        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;
        data.playerX = player.transform.position.x;
        data.playerY = player.transform.position.y;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerProgression progression = player.GetComponent<PlayerProgression>();
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();

        if (stats != null)
        {
            data.currentHealth = stats.CurrentHealth;
        }

        if (progression != null)
        {
            data.level = progression.Level;
            data.currentExperience = progression.CurrentExperience;
            data.gold = progression.Gold;
        }

        if (inventory != null)
        {
            foreach (EquipmentItemData item in inventory.Items)
            {
                if (item != null)
                {
                    data.inventoryItemIds.Add(GetItemSaveKey(item));
                }
            }
        }

        if (equipment != null)
        {
            foreach (PlayerEquipment.EquipmentEntry entry in equipment.GetEquippedEntries())
            {
                if (entry.itemData != null)
                {
                    data.equippedItems.Add(new SaveEquipmentEntry
                    {
                        slotType = (int)entry.slotType,
                        itemId = GetItemSaveKey(entry.itemData)
                    });
                }
            }
        }

        // Abilities
        HashSet<PlayerAbilityType> abilities = player.GetUnlockedAbilities();
        if (abilities != null)
        {
            foreach (PlayerAbilityType ability in abilities)
            {
                data.unlockedAbilities.Add((int)ability);
            }
        }

        // Discovered areas
        UndiscoveredAreaManager areaManager = FindObjectOfType<UndiscoveredAreaManager>();
        if (areaManager != null)
        {
            List<string> discovered = areaManager.GetDiscoveredAreaIds();
            if (discovered != null)
            {
                data.discoveredAreas = discovered;
            }
        }

        data.removedSceneObjectIds = new List<string>(removedSceneObjectIds);
        data.reward2Collected = IsReward2Collected(player);
        data.reward2AbilityUnlocked = player.HasAbility(Reward2Ability);

        return data;
    }

    private void ApplyPlayerState(Player player, SaveData data)
    {
        // Position
        player.transform.position = new Vector3(data.playerX, data.playerY, 0f);

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerProgression progression = player.GetComponent<PlayerProgression>();
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();

        // Progression
        if (progression != null)
        {
            progression.LoadState(data.level, data.currentExperience, data.gold);
        }

        // Stats (health)
        if (stats != null)
        {
            stats.LoadHealth(data.currentHealth);
        }

        // Abilities
        player.LoadAbilities(data.unlockedAbilities);

        // Equipment
        if (equipment != null)
        {
            equipment.ClearAll();
            foreach (SaveEquipmentEntry entry in data.equippedItems)
            {
                EquipmentItemData itemData = FindItem(entry.itemId, (EquipmentSlotType)entry.slotType);
                if (itemData != null)
                {
                    equipment.Equip(itemData);
                }
            }
        }

        // Inventory
        if (inventory != null)
        {
            inventory.ClearAll();
            foreach (string itemId in data.inventoryItemIds)
            {
                EquipmentItemData itemData = FindItem(itemId);
                if (itemData != null)
                {
                    inventory.AddItem(itemData);
                }
            }
        }

        // Discovered areas
        UndiscoveredAreaManager areaManager = FindObjectOfType<UndiscoveredAreaManager>();
        if (areaManager != null && data.discoveredAreas != null)
        {
            areaManager.LoadDiscoveredAreas(data.discoveredAreas);
        }

        RestoreReward2State(player, inventory, equipment, data);
        ApplyRemovedSceneObjects(data.removedSceneObjectIds);
        MigrateLegacyPickupState(data, player, inventory, equipment);
    }

    public void MarkSceneObjectRemoved(Component component)
    {
        string sceneObjectId = GetSceneObjectId(component);
        if (!string.IsNullOrEmpty(sceneObjectId))
        {
            removedSceneObjectIds.Add(sceneObjectId);
        }
    }

    private string GetItemSaveKey(EquipmentItemData itemData)
    {
        if (itemData == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(itemData.itemId))
        {
            return "id:" + itemData.itemId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(itemData.name))
        {
            return "name:" + itemData.name.Trim();
        }

        return !string.IsNullOrWhiteSpace(itemData.itemName)
            ? "display:" + itemData.itemName.Trim()
            : string.Empty;
    }

    private EquipmentItemData FindItem(string savedKey, EquipmentSlotType? expectedSlotType = null)
    {
        RebuildItemLookup();

        if (!string.IsNullOrWhiteSpace(savedKey) && itemLookup.TryGetValue(savedKey, out EquipmentItemData itemData))
        {
            return itemData;
        }

        if (expectedSlotType.HasValue)
        {
            EquipmentItemData fallback = null;

            foreach (EquipmentItemData candidate in itemLookup.Values)
            {
                if (candidate == null || candidate.slotType != expectedSlotType.Value)
                {
                    continue;
                }

                if (fallback != null)
                {
                    return null;
                }

                fallback = candidate;
            }

            return fallback;
        }

        return null;
    }

    private void RebuildItemLookup()
    {
        itemLookup.Clear();

        RegisterItems(allEquipmentItems);
        RegisterItems(Resources.FindObjectsOfTypeAll<EquipmentItemData>());
    }

    private void RegisterItems(IReadOnlyList<EquipmentItemData> items)
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            EquipmentItemData itemData = items[i];
            if (itemData == null)
            {
                continue;
            }

            RegisterItemKey("id:" + itemData.itemId, itemData);
            RegisterItemKey("name:" + itemData.name, itemData);
            RegisterItemKey("display:" + itemData.itemName, itemData);
        }
    }

    private void RegisterItemKey(string key, EquipmentItemData itemData)
    {
        if (itemData == null || string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        key = key.Trim();
        if (!itemLookup.ContainsKey(key))
        {
            itemLookup.Add(key, itemData);
        }
    }

    private void ApplyRemovedSceneObjects(List<string> sceneObjectIds)
    {
        removedSceneObjectIds.Clear();

        if (sceneObjectIds == null)
        {
            return;
        }

        for (int i = 0; i < sceneObjectIds.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(sceneObjectIds[i]))
            {
                removedSceneObjectIds.Add(sceneObjectIds[i]);
            }
        }

        ApplyRemovedStateToObjects(FindObjectsOfType<Enemy>(true));
        ApplyRemovedStateToAbilityPickups(FindObjectsOfType<AbilityPickup>(true));
    }

    private void MigrateLegacyPickupState(SaveData data, Player player, PlayerInventory inventory, PlayerEquipment equipment)
    {
        if (data != null && data.removedSceneObjectIds != null && data.removedSceneObjectIds.Count > 0)
        {
            return;
        }

        AbilityPickup[] abilityPickups = FindObjectsOfType<AbilityPickup>(true);
        for (int i = 0; i < abilityPickups.Length; i++)
        {
            AbilityPickup pickup = abilityPickups[i];
            if (pickup == null || player == null)
            {
                continue;
            }

            bool shouldHide = player.GetUnlockedAbilities().Contains(pickup.GetAbilityToUnlock()) ||
                IsSiblingEquipmentAlreadyOwned(pickup, inventory, equipment);

            if (shouldHide)
            {
                pickup.ApplyCollectedState();
            }
        }
    }

    private void ApplyRemovedStateToObjects<T>(T[] objects) where T : Component
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            T obj = objects[i];
            if (obj == null)
            {
                continue;
            }

            string sceneObjectId = GetSceneObjectId(obj);
            bool shouldBeRemoved = !string.IsNullOrEmpty(sceneObjectId) && removedSceneObjectIds.Contains(sceneObjectId);
            obj.gameObject.SetActive(!shouldBeRemoved);
        }
    }

    private void ApplyRemovedStateToAbilityPickups(AbilityPickup[] pickups)
    {
        if (pickups == null)
        {
            return;
        }

        for (int i = 0; i < pickups.Length; i++)
        {
            AbilityPickup pickup = pickups[i];
            if (pickup == null)
            {
                continue;
            }

            string sceneObjectId = GetSceneObjectId(pickup);
            bool shouldBeRemoved = !string.IsNullOrEmpty(sceneObjectId) && removedSceneObjectIds.Contains(sceneObjectId);
            if (shouldBeRemoved)
            {
                pickup.ApplyCollectedState();
            }
        }
    }

    private void ReconcileRewardState(Player player, PlayerInventory inventory, PlayerEquipment equipment, SaveData data)
    {
        if (player == null)
        {
            return;
        }

        AbilityPickup[] abilityPickups = FindObjectsOfType<AbilityPickup>(true);
        for (int i = 0; i < abilityPickups.Length; i++)
        {
            AbilityPickup pickup = abilityPickups[i];
            if (pickup == null)
            {
                continue;
            }

            bool removedBySave = IsSceneObjectMarkedRemoved(data, pickup);
            bool relatedEquipmentOwned = IsSiblingEquipmentAlreadyOwned(pickup, inventory, equipment);
            bool shouldOwnAbility = removedBySave || relatedEquipmentOwned;

            if (shouldOwnAbility && !player.HasAbility(pickup.GetAbilityToUnlock()))
            {
                player.UnlockAbility(pickup.GetAbilityToUnlock());
            }

            if (player.HasAbility(pickup.GetAbilityToUnlock()) || relatedEquipmentOwned)
            {
                pickup.ApplyCollectedState();
            }
        }
    }

    private void RestoreReward2State(Player player, PlayerInventory inventory, PlayerEquipment equipment, SaveData data)
    {
        if (player == null || data == null)
        {
            return;
        }

        GameObject reward2Object = FindReward2Object();
        AbilityPickup reward2AbilityPickup = reward2Object != null ? reward2Object.GetComponent<AbilityPickup>() : null;
        EquipmentPickup reward2EquipmentPickup = reward2Object != null ? reward2Object.GetComponent<EquipmentPickup>() : null;

        bool reward2Collected = data.reward2Collected;
        bool reward2AbilityUnlocked = data.reward2AbilityUnlocked;

        EquipmentItemData reward2Item = reward2EquipmentPickup != null ? reward2EquipmentPickup.GetEquipmentItem() : null;
        if (reward2Item != null)
        {
            bool inInventory = inventory != null && inventory.Contains(reward2Item);
            bool equippedNow = equipment != null && equipment.GetEquippedItem(reward2Item.slotType) == reward2Item;
            reward2Collected = reward2Collected || inInventory || equippedNow;
        }

        reward2AbilityUnlocked = reward2AbilityUnlocked || player.HasAbility(Reward2Ability) || reward2Collected;

        if (reward2AbilityUnlocked && !player.HasAbility(Reward2Ability))
        {
            player.UnlockAbility(Reward2Ability);
        }

        if (reward2AbilityPickup != null && reward2AbilityUnlocked)
        {
            reward2AbilityPickup.ApplyCollectedState();
        }

        if (reward2Object != null && reward2Collected)
        {
            reward2Object.SetActive(false);
        }
    }

    private bool IsSceneObjectMarkedRemoved(SaveData data, Component component)
    {
        if (data == null || data.removedSceneObjectIds == null || component == null)
        {
            return false;
        }

        string sceneObjectId = GetSceneObjectId(component);
        return !string.IsNullOrEmpty(sceneObjectId) && data.removedSceneObjectIds.Contains(sceneObjectId);
    }

    private bool IsSiblingEquipmentAlreadyOwned(AbilityPickup pickup, PlayerInventory inventory, PlayerEquipment equipment)
    {
        if (pickup == null)
        {
            return false;
        }

        EquipmentPickup equipmentPickup = pickup.GetComponent<EquipmentPickup>();
        EquipmentItemData itemData = equipmentPickup != null ? equipmentPickup.GetEquipmentItem() : null;
        if (itemData == null)
        {
            return false;
        }

        bool inInventory = inventory != null && inventory.Contains(itemData);
        bool equippedNow = equipment != null && equipment.GetEquippedItem(itemData.slotType) == itemData;
        return inInventory || equippedNow;
    }

    private bool IsReward2Collected(Player player)
    {
        if (player == null)
        {
            return false;
        }

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
        GameObject reward2Object = FindReward2Object();
        EquipmentPickup reward2EquipmentPickup = reward2Object != null ? reward2Object.GetComponent<EquipmentPickup>() : null;
        EquipmentItemData reward2Item = reward2EquipmentPickup != null ? reward2EquipmentPickup.GetEquipmentItem() : null;

        if (reward2Item == null)
        {
            return reward2Object != null && !reward2Object.activeSelf;
        }

        bool inInventory = inventory != null && inventory.Contains(reward2Item);
        bool equippedNow = equipment != null && equipment.GetEquippedItem(reward2Item.slotType) == reward2Item;
        return inInventory || equippedNow || (reward2Object != null && !reward2Object.activeSelf);
    }

    private GameObject FindReward2Object()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate == null || candidate.scene.name != SceneManager.GetActiveScene().name)
            {
                continue;
            }

            if (candidate.name == Reward2ObjectName)
            {
                return candidate;
            }
        }

        return null;
    }

    private string GetSceneObjectId(Component component)
    {
        if (component == null || component.gameObject.scene.name != SceneManager.GetActiveScene().name)
        {
            return null;
        }

        return component.gameObject.scene.name + ":" + GetTransformPath(component.transform) + "#" + component.GetType().Name;
    }

    private string GetTransformPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = GetTransformSegment(target);
        Transform current = target.parent;

        while (current != null)
        {
            path = GetTransformSegment(current) + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private string GetTransformSegment(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        int siblingIndex = target.GetSiblingIndex();
        return target.name + "[" + siblingIndex + "]";
    }

    private static string GetSlotPathStatic(int slot)
    {
        return Path.Combine(SaveDirectory, $"save_{slot}.json");
    }
}

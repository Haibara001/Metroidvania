using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public float playerX;
    public float playerY;

    public float currentHealth;
    public int level;
    public int currentExperience;
    public int gold;

    public List<string> inventoryItemIds = new List<string>();
    public List<SaveEquipmentEntry> equippedItems = new List<SaveEquipmentEntry>();
    public List<int> unlockedAbilities = new List<int>();
    public List<string> discoveredAreas = new List<string>();
    public List<string> removedSceneObjectIds = new List<string>();
    public bool reward2Collected;
    public bool reward2AbilityUnlocked;

    public string saveTime;
}

[Serializable]
public class SaveEquipmentEntry
{
    public int slotType;
    public string itemId;
}

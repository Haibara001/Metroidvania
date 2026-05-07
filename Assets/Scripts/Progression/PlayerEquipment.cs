using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Serializable]
    public class EquipmentEntry
    {
        public EquipmentSlotType slotType;
        public EquipmentItemData itemData;
    }

    [SerializeField] private List<EquipmentEntry> startingEquipment = new List<EquipmentEntry>();
    private readonly Dictionary<EquipmentSlotType, EquipmentItemData> equippedItems = new Dictionary<EquipmentSlotType, EquipmentItemData>();

    public event Action EquipmentChanged;

    private void Awake()
    {
        equippedItems.Clear();

        for (int i = 0; i < startingEquipment.Count; i++)
        {
            EquipmentEntry entry = startingEquipment[i];

            if (entry == null || entry.itemData == null)
            {
                continue;
            }

            equippedItems[entry.slotType] = entry.itemData;
        }
    }

    public EquipmentItemData GetEquippedItem(EquipmentSlotType slotType)
    {
        equippedItems.TryGetValue(slotType, out EquipmentItemData itemData);
        return itemData;
    }

    public EquipmentItemData Equip(EquipmentItemData itemData)
    {
        if (itemData == null)
        {
            return null;
        }

        EquipmentItemData previousItem = GetEquippedItem(itemData.slotType);
        equippedItems[itemData.slotType] = itemData;
        EquipmentChanged?.Invoke();
        return previousItem;
    }

    public EquipmentItemData Unequip(EquipmentSlotType slotType)
    {
        EquipmentItemData previousItem = GetEquippedItem(slotType);

        if (previousItem == null)
        {
            return null;
        }

        equippedItems.Remove(slotType);
        EquipmentChanged?.Invoke();
        return previousItem;
    }

    public List<EquipmentEntry> GetEquippedEntries()
    {
        List<EquipmentEntry> entries = new List<EquipmentEntry>();

        foreach (KeyValuePair<EquipmentSlotType, EquipmentItemData> pair in equippedItems)
        {
            entries.Add(new EquipmentEntry
            {
                slotType = pair.Key,
                itemData = pair.Value
            });
        }

        return entries;
    }

    public StatModifierData GetTotalBonuses()
    {
        StatModifierData bonuses = default;

        foreach (EquipmentItemData itemData in equippedItems.Values)
        {
            bonuses.maxHealth += itemData.statBonuses.maxHealth;
            bonuses.attackPower += itemData.statBonuses.attackPower;
            bonuses.moveSpeed += itemData.statBonuses.moveSpeed;
            bonuses.jumpForce += itemData.statBonuses.jumpForce;
            bonuses.dashSpeed += itemData.statBonuses.dashSpeed;
        }

        return bonuses;
    }

    public void ClearAll()
    {
        equippedItems.Clear();
        EquipmentChanged?.Invoke();
    }
}

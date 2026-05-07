using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<EquipmentItemData> startingItems = new List<EquipmentItemData>();
    [SerializeField] private List<EquipmentItemData> items = new List<EquipmentItemData>();

    public event Action InventoryChanged;

    public IReadOnlyList<EquipmentItemData> Items => items;

    private void Awake()
    {
        items.Clear();

        for (int i = 0; i < startingItems.Count; i++)
        {
            if (startingItems[i] == null)
            {
                continue;
            }

            items.Add(startingItems[i]);
        }
    }

    public bool AddItem(EquipmentItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        items.Add(itemData);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(EquipmentItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        bool removed = items.Remove(itemData);

        if (removed)
        {
            InventoryChanged?.Invoke();
        }

        return removed;
    }

    public bool Contains(EquipmentItemData itemData)
    {
        return itemData != null && items.Contains(itemData);
    }

    public void ClearAll()
    {
        items.Clear();
        InventoryChanged?.Invoke();
    }
}

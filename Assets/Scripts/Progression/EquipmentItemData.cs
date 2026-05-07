using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "RpgTest/Equipment Item")]
public class EquipmentItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public EquipmentSlotType slotType;
    public int goldValue;
    public StatModifierData statBonuses;
}

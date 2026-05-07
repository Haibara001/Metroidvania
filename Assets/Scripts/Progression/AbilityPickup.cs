using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField] private PlayerAbilityType abilityToUnlock;
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private GameObject pickupVisual;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected)
        {
            return;
        }

        Player player = collision.GetComponent<Player>();

        if (player == null)
        {
            player = collision.GetComponentInParent<Player>();
        }

        if (player == null || !player.UnlockAbility(abilityToUnlock))
        {
            return;
        }

        collected = true;

        if (pickupVisual != null)
        {
            pickupVisual.SetActive(false);
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkSceneObjectRemoved(this);
        }

        if (destroyAfterPickup)
        {
            if (GetComponent<EquipmentPickup>() != null)
            {
                enabled = false;
                return;
            }

            Destroy(gameObject);
            return;
        }

        if (GetComponent<EquipmentPickup>() != null)
        {
            enabled = false;
            return;
        }

        gameObject.SetActive(false);
    }

    public PlayerAbilityType GetAbilityToUnlock()
    {
        return abilityToUnlock;
    }

    public void ApplyCollectedState()
    {
        collected = true;

        if (pickupVisual != null)
        {
            pickupVisual.SetActive(false);
        }

        if (GetComponent<EquipmentPickup>() != null)
        {
            enabled = false;
            return;
        }

        gameObject.SetActive(false);
    }
}

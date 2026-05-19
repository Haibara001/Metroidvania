using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField] private PlayerAbilityType abilityToUnlock;
    [SerializeField] private string abilityDescription;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private AudioClip pickupSFX;
    [SerializeField] private GameObject pickupVisual;

    private bool collected;
    private Player nearbyPlayer;

    private void Update()
    {
        if (collected || nearbyPlayer == null)
        {
            return;
        }

        if (Input.GetKeyDown(interactKey))
        {
            Collect();
        }
    }

    public void CollectFromEquipment(Player player)
    {
        if (collected)
        {
            return;
        }

        nearbyPlayer = player;
        Collect();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected)
        {
            return;
        }

        Debug.Log($"AbilityPickup: OnTriggerEnter2D with {collision.name}");

        Player player = collision.GetComponent<Player>();

        if (player == null)
        {
            player = collision.GetComponentInParent<Player>();
        }

        if (player != null)
        {
            nearbyPlayer = player;
            Debug.Log("AbilityPickup: Player detected");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player == null)
        {
            player = collision.GetComponentInParent<Player>();
        }

        if (player != null && player == nearbyPlayer)
        {
            nearbyPlayer = null;
            Debug.Log("AbilityPickup: Player exited trigger");
        }
    }

    private void Collect()
    {
        if (nearbyPlayer == null)
        {
            Debug.Log("AbilityPickup: nearbyPlayer is null");
            return;
        }

        bool unlocked = nearbyPlayer.UnlockAbility(abilityToUnlock);
        Debug.Log($"AbilityPickup: UnlockAbility({abilityToUnlock}) returned {unlocked}");

        if (!unlocked)
        {
            return;
        }

        collected = true;

        if (SFXManager.instance != null)
            SFXManager.instance.PlaySFX(pickupSFX);

        UI_AbilityNotification.Show(abilityToUnlock, abilityDescription);
        Debug.Log("AbilityPickup: Notification shown");

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

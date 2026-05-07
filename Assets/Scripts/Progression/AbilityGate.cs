using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AbilityGate : MonoBehaviour
{
    [SerializeField] private PlayerAbilityType requiredAbility;
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private bool disableObjectWhenUnlocked;

    private bool isUnlocked;

    private void Reset()
    {
        blockingCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        if (blockingCollider == null)
        {
            blockingCollider = GetComponent<Collider2D>();
        }

        ApplyLockedState();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryUnlock(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryUnlock(collision.collider);
    }

    private void ApplyLockedState()
    {
        if (blockingCollider != null)
        {
            blockingCollider.enabled = true;
        }

        if (lockedVisual != null)
        {
            lockedVisual.SetActive(true);
        }

        if (unlockedVisual != null)
        {
            unlockedVisual.SetActive(false);
        }
    }

    private void UnlockGate()
    {
        if (isUnlocked)
        {
            return;
        }

        isUnlocked = true;

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        if (lockedVisual != null)
        {
            lockedVisual.SetActive(false);
        }

        if (unlockedVisual != null)
        {
            unlockedVisual.SetActive(true);
        }

        if (disableObjectWhenUnlocked)
        {
            gameObject.SetActive(false);
        }
    }

    private void TryUnlock(Collider2D other)
    {
        if (isUnlocked || other == null)
        {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player == null)
        {
            player = other.GetComponentInParent<Player>();
        }

        if (player == null || !player.HasAbility(requiredAbility))
        {
            return;
        }

        UnlockGate();
    }
}

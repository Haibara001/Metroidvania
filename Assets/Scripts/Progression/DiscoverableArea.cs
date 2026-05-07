using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DiscoverableArea : MonoBehaviour
{
    [SerializeField] private string areaId;
    [SerializeField] private UndiscoveredAreaManager areaManager;
    [SerializeField] private Collider2D revealCollider;
    [SerializeField] private GameObject[] hiddenObjects;
    [SerializeField] private bool revealOnStartIfAlreadyDiscovered = true;

    private void Awake()
    {
        if (revealCollider == null)
        {
            revealCollider = GetComponent<Collider2D>();
        }

        if (areaManager == null)
        {
            areaManager = FindObjectOfType<UndiscoveredAreaManager>();
        }
    }

    private void Start()
    {
        bool alreadyDiscovered = areaManager != null && areaManager.IsDiscovered(areaId);
        SetHiddenObjectsVisible(alreadyDiscovered);
        SetRevealEnabled(!alreadyDiscovered);

        if (alreadyDiscovered && areaManager != null)
        {
            areaManager.ClearAreaTiles(GetRevealBounds());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player == null)
        {
            player = collision.GetComponentInParent<Player>();
        }

        if (player == null || areaManager == null || string.IsNullOrWhiteSpace(areaId))
        {
            return;
        }

        areaManager.RevealArea(areaId, GetRevealBounds());
        SetHiddenObjectsVisible(true);
        SetRevealEnabled(false);
    }

    private BoundsInt GetRevealBounds()
    {
        if (areaManager == null || revealCollider == null)
        {
            return new BoundsInt();
        }

        return areaManager.WorldBoundsToCellBounds(revealCollider.bounds);
    }

    public string GetAreaId()
    {
        return areaId;
    }

    public void ForceReveal()
    {
        if (areaManager != null)
        {
            areaManager.RevealArea(areaId, GetRevealBounds());
        }

        SetHiddenObjectsVisible(true);
        SetRevealEnabled(false);
    }

    private void SetHiddenObjectsVisible(bool isVisible)
    {
        for (int i = 0; i < hiddenObjects.Length; i++)
        {
            if (hiddenObjects[i] != null)
            {
                hiddenObjects[i].SetActive(isVisible);
            }
        }
    }

    private void SetRevealEnabled(bool isEnabled)
    {
        if (revealCollider != null)
        {
            revealCollider.enabled = isEnabled;
        }
    }
}

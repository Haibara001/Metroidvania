using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    [SerializeField] private int experienceAmount = 10;
    [SerializeField] private int goldAmount;
    [SerializeField] private bool destroyAfterPickup = true;
    [SerializeField] private float autoCollectDelay = 0.2f;
    [SerializeField] private float attractionRange = 100f;
    [SerializeField] private float absorbDistance = 0.35f;
    [SerializeField] private float minFlySpeed = 4f;
    [SerializeField] private float maxFlySpeed = 12f;
    [SerializeField] private float acceleration = 18f;

    private PlayerProgression targetProgression;
    private float currentFlySpeed;
    private float delayTimer;

    private void Awake()
    {
        delayTimer = autoCollectDelay;
        currentFlySpeed = minFlySpeed;
    }

    private void Update()
    {
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        if (targetProgression == null)
        {
            targetProgression = FindClosestPlayerInRange();
        }

        if (targetProgression == null)
        {
            return;
        }

        currentFlySpeed = Mathf.MoveTowards(currentFlySpeed, maxFlySpeed, acceleration * Time.deltaTime);
        Transform targetTransform = targetProgression.transform;
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, currentFlySpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetTransform.position) <= absorbDistance)
        {
            Absorb(targetProgression);
        }
    }

    public void Configure(int experience, int gold, bool destroyOnPickup = true)
    {
        experienceAmount = Mathf.Max(0, experience);
        goldAmount = Mathf.Max(0, gold);
        destroyAfterPickup = destroyOnPickup;
        delayTimer = autoCollectDelay;
        currentFlySpeed = minFlySpeed;
    }

    private void Absorb(PlayerProgression progression)
    {
        if (progression == null)
        {
            return;
        }

        progression.GainExperience(experienceAmount);

        if (goldAmount > 0)
        {
            progression.AddGold(goldAmount);
        }

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }

    private PlayerProgression FindClosestPlayerInRange()
    {
        PlayerProgression[] players = FindObjectsOfType<PlayerProgression>();
        PlayerProgression closestPlayer = null;
        float closestDistance = attractionRange;

        for (int i = 0; i < players.Length; i++)
        {
            float distance = Vector2.Distance(transform.position, players[i].transform.position);

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestPlayer = players[i];
            }
        }

        return closestPlayer;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, attractionRange);
    }
}

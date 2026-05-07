using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WitchEcho : MonoBehaviour, IDamageable, IAggroTarget
{
    [SerializeField] private float duration = 4f;
    [SerializeField] private float burstRadius = 2.5f;
    [SerializeField] private float burstDamage = 20f;
    [SerializeField] private int aggroPriority = 10;
    [SerializeField] private Color echoTint = new Color(1f, 0.75f, 0.9f, 0.7f);

    private Player owner;
    private float timer;
    private bool burstTriggered;
    private SpriteRenderer spriteRenderer;

    public Transform AimPoint => transform;
    public int AggroPriority => aggroPriority;
    public bool CanBeTargeted => !burstTriggered;

    public void Initialize(Player playerOwner, float lifeTime, float damageRadius, float damageAmount)
    {
        owner = playerOwner;
        duration = lifeTime;
        burstRadius = damageRadius;
        burstDamage = damageAmount;
        timer = duration;

        gameObject.layer = owner.gameObject.layer;

        CircleCollider2D triggerCollider = GetComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = Mathf.Max(0.3f, damageRadius * 0.35f);

        SpriteRenderer ownerRenderer = owner.GetComponentInChildren<SpriteRenderer>();
        if (ownerRenderer != null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ownerRenderer.sprite;
            spriteRenderer.flipX = ownerRenderer.flipX;
            spriteRenderer.sortingLayerID = ownerRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = ownerRenderer.sortingOrder - 1;
            spriteRenderer.color = echoTint;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerPetalBurst();
        }
    }

    public bool TakeDamage(float damage)
    {
        TriggerPetalBurst();
        return true;
    }

    public Vector3 GetSwapPosition()
    {
        return transform.position;
    }

    public void TriggerPetalBurst()
    {
        if (burstTriggered)
        {
            return;
        }

        burstTriggered = true;
        DealBurstDamage();

        if (owner != null)
        {
            owner.NotifyEchoDestroyed(this);
        }

        Destroy(gameObject);
    }

    private void DealBurstDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, burstRadius);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = hits[i].GetComponentInParent<IDamageable>();
            }

            if (damageable == null || damagedTargets.Contains(damageable))
            {
                continue;
            }

            if (damageable == this)
            {
                continue;
            }

            if (damageable == owner)
            {
                continue;
            }

            damagedTargets.Add(damageable);
            damageable.TakeDamage(burstDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.55f, 0.8f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, burstRadius);
    }
}

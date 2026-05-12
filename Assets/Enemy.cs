using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity, IDamageable
{
    [Header("Move info")]
    public float moveSpeed;
    public float idleTime;

    [Header("Stats")]
    [SerializeField] protected float maxHealth = 400f;
    protected float currentHealth;
    [SerializeField] protected float damagedStateDuration = 0.2f;
    [SerializeField] protected float deadStateDuration = 0.6f;
    protected bool isDead;

    [Header("Rewards")]
    [SerializeField] protected int experienceReward = 10;
    [SerializeField] protected int goldReward = 2;
    [SerializeField] protected GameObject rewardPickupPrefab;
    [SerializeField] protected Vector3 rewardSpawnOffset = new Vector3(0f, 0.35f, 0f);
    public event Action<Enemy> DeathFinalized;
    public EnemyStateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
        currentHealth = maxHealth;
        isDead = false;
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }

    public virtual bool TakeDamage(float damage)
    {
        if (isDead)
        {
            return false;
        }

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }

        return true;
    }

    protected virtual void Die()
    {
        isDead = true;
    }

    public float GetDamagedStateDuration()
    {
        return damagedStateDuration;
    }

    public float GetDeadStateDuration()
    {
        return deadStateDuration;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;

    public virtual void FinalizeDeath()
    {
        DeathFinalized?.Invoke(this);
        SpawnDeathRewards();
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkSceneObjectRemoved(this);
        }
        Destroy(gameObject);
    }

    protected virtual void SpawnDeathRewards()
    {
        if (experienceReward <= 0 && goldReward <= 0)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + rewardSpawnOffset;
        GameObject rewardObject = rewardPickupPrefab != null
            ? Instantiate(rewardPickupPrefab, spawnPosition, Quaternion.identity)
            : CreateDefaultRewardPickup(spawnPosition);

        ExperiencePickup pickup = rewardObject.GetComponent<ExperiencePickup>();

        if (pickup == null)
        {
            pickup = rewardObject.GetComponentInChildren<ExperiencePickup>();
        }

        if (pickup != null)
        {
            pickup.Configure(experienceReward, goldReward);
        }
    }

    private GameObject CreateDefaultRewardPickup(Vector3 spawnPosition)
    {
        GameObject rewardObject = new GameObject(name + "_RewardPickup");
        rewardObject.transform.position = spawnPosition;

        rewardObject.AddComponent<ExperiencePickup>();
        return rewardObject;
    }

}

using System;
using UnityEngine;

[RequireComponent(typeof(PlayerProgression))]
[RequireComponent(typeof(PlayerEquipment))]
public class PlayerStats : MonoBehaviour
{
    [Header("Base stats")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseAttackPower = 15f;
    [SerializeField] private float baseMoveSpeed = 12f;
    [SerializeField] private float baseJumpForce = 12f;
    [SerializeField] private float baseDashSpeed = 18f;

    private PlayerProgression progression;
    private PlayerEquipment equipment;
    private float currentHealth;
    private float lastMaxHealth;

    public event Action StatsChanged;

    public float MaxHealth => GetFinalStats().maxHealth;
    public float AttackPower => GetFinalStats().attackPower;
    public float MoveSpeed => GetFinalStats().moveSpeed;
    public float JumpForce => GetFinalStats().jumpForce;
    public float DashSpeed => GetFinalStats().dashSpeed;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        progression = GetComponent<PlayerProgression>();
        equipment = GetComponent<PlayerEquipment>();
        currentHealth = MaxHealth;
    }

    private void Start()
    {
        currentHealth = MaxHealth;
        lastMaxHealth = MaxHealth;
        StatsChanged?.Invoke();
    }

    private void OnEnable()
    {
        if (progression != null)
        {
            progression.ProgressionChanged += RecalculateAndClamp;
        }

        if (equipment != null)
        {
            equipment.EquipmentChanged += RecalculateAndClamp;
        }
    }

    private void OnDisable()
    {
        if (progression != null)
        {
            progression.ProgressionChanged -= RecalculateAndClamp;
        }

        if (equipment != null)
        {
            equipment.EquipmentChanged -= RecalculateAndClamp;
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
        StatsChanged?.Invoke();
    }

    public void ApplyDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0f, amount), 0f, MaxHealth);
        StatsChanged?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0f, amount), 0f, MaxHealth);
        StatsChanged?.Invoke();
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public void LoadHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, MaxHealth);
        StatsChanged?.Invoke();
    }

    public StatModifierData GetFinalStats()
    {
        StatModifierData levelBonuses = progression != null ? progression.GetLevelBonuses() : default;
        StatModifierData equipmentBonuses = equipment != null ? equipment.GetTotalBonuses() : default;

        return new StatModifierData
        {
            maxHealth = baseMaxHealth + levelBonuses.maxHealth + equipmentBonuses.maxHealth,
            attackPower = baseAttackPower + levelBonuses.attackPower + equipmentBonuses.attackPower,
            moveSpeed = baseMoveSpeed + levelBonuses.moveSpeed + equipmentBonuses.moveSpeed,
            jumpForce = baseJumpForce + levelBonuses.jumpForce + equipmentBonuses.jumpForce,
            dashSpeed = baseDashSpeed + levelBonuses.dashSpeed + equipmentBonuses.dashSpeed
        };
    }

    private void RecalculateAndClamp()
    {
        float newMax = MaxHealth;
        float delta = newMax - lastMaxHealth;

        if (delta > 0f)
        {
            currentHealth += delta;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0f, newMax);
        lastMaxHealth = newMax;
        StatsChanged?.Invoke();
    }
}

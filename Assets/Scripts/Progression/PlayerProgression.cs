using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExperience;
    [SerializeField] private int baseExperienceToLevel = 100;
    [SerializeField] private int experienceGrowthPerLevel = 35;

    [Header("Currency")]
    [SerializeField] private int gold;

    [Header("Growth per level")]
    [SerializeField] private float maxHealthPerLevel = 10f;
    [SerializeField] private float attackPowerPerLevel = 2f;
    [SerializeField] private float moveSpeedPerLevel = 0.15f;
    [SerializeField] private float jumpForcePerLevel = 0.2f;
    [SerializeField] private float dashSpeedPerLevel = 0.3f;

    public event Action ProgressionChanged;

    public int Level => level;
    public int CurrentExperience => currentExperience;
    public int Gold => gold;

    public void GainExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        while (currentExperience >= GetRequiredExperienceForNextLevel())
        {
            currentExperience -= GetRequiredExperienceForNextLevel();
            level++;
        }

        ProgressionChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        gold += Mathf.Max(0, amount);
        ProgressionChanged?.Invoke();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (gold < amount)
        {
            return false;
        }

        gold -= amount;
        ProgressionChanged?.Invoke();
        return true;
    }

    public int GetRequiredExperienceForNextLevel()
    {
        return baseExperienceToLevel + (level - 1) * experienceGrowthPerLevel;
    }

    public StatModifierData GetLevelBonuses()
    {
        int bonusLevels = Mathf.Max(0, level - 1);

        return new StatModifierData
        {
            maxHealth = bonusLevels * maxHealthPerLevel,
            attackPower = bonusLevels * attackPowerPerLevel,
            moveSpeed = bonusLevels * moveSpeedPerLevel,
            jumpForce = bonusLevels * jumpForcePerLevel,
            dashSpeed = bonusLevels * dashSpeedPerLevel
        };
    }

    public void LoadState(int savedLevel, int savedExperience, int savedGold)
    {
        level = Mathf.Max(1, savedLevel);
        currentExperience = Mathf.Max(0, savedExperience);
        gold = Mathf.Max(0, savedGold);
        ProgressionChanged?.Invoke();
    }
}

using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAbilityUnlockOnDeath : MonoBehaviour
{
    [SerializeField] private PlayerAbilityType abilityToUnlock = PlayerAbilityType.WallJump;

    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.DeathFinalized += HandleDeathFinalized;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.DeathFinalized -= HandleDeathFinalized;
        }
    }

    private void HandleDeathFinalized(Enemy deadEnemy)
    {
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            return;
        }

        if (player.UnlockAbility(abilityToUnlock))
        {
            UI_AbilityNotification.Show(abilityToUnlock);
        }
    }
}

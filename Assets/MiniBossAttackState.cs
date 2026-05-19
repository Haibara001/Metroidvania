using System.Collections.Generic;
using UnityEngine;

public class MiniBossAttackState : EnemyState
{
    private readonly Enemy_MiniBoss enemy;
    private float elapsedTime;

    public MiniBossAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_MiniBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        elapsedTime = 0f;

        Transform player = enemy.GetPlayerTransform();

        if (enemy.ShouldTeleportForCurrentAttack() && player != null)
        {
            float desiredFacing = player.position.x >= enemy.transform.position.x ? 1f : -1f;
            float teleportOffset = enemy.GetCurrentAttackTeleportOffset();
            Vector3 newPosition = enemy.transform.position;
            newPosition.x = player.position.x - desiredFacing * teleportOffset;
            enemy.transform.position = newPosition;
            enemy.SetFacingDirection((int)desiredFacing);
        }
        else if (player != null && (player.position.x - enemy.transform.position.x) * enemy.facingDir < 0)
        {
            enemy.Flip();
        }

        enemy.CommitQueuedAttack();
        enemy.StartAttackCooldown();
        enemy.PlayCurrentAttackSFX();
        enemy.PlayAttackAnimation();
        stateTimer = enemy.GetCurrentAttackDuration();
    }

    public override void Update()
    {
        base.Update();

        elapsedTime += Time.deltaTime;

        if (!triggerCalled && elapsedTime >= enemy.GetCurrentAttackTriggerDelay())
        {
            DealDamage();
            triggerCalled = true;
        }

        if (!enemy.IsPlayerInAttackRange() && triggerCalled && !enemy.ShouldTeleportForCurrentAttack())
        {
            stateMachine.ChangeStates(enemy.moveState);
            return;
        }

        if (stateTimer < 0f)
        {
            stateMachine.ChangeStates(enemy.idleState);
        }
    }

    private void DealDamage()
    {
        if (enemy.IsCurrentAttackProjectile())
        {
            enemy.SpawnProjectile();
            return;
        }

        Collider2D[] hitObjects;
        Vector2 attackCenter = enemy.GetCurrentAttackCenter();

        if (enemy.GetCurrentAttackHitShape() == Enemy_MiniBoss.AttackHitShape.Box)
        {
            hitObjects = Physics2D.OverlapBoxAll(attackCenter, enemy.GetCurrentAttackBoxSize(), 0f, enemy.whatCanBeHit);
        }
        else
        {
            hitObjects = Physics2D.OverlapCircleAll(attackCenter, enemy.GetCurrentAttackRadius(), enemy.whatCanBeHit);
        }

        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider2D hit in hitObjects)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = hit.GetComponentInParent<IDamageable>();
            }

            if (damageable == null || damagedTargets.Contains(damageable))
            {
                continue;
            }

            damagedTargets.Add(damageable);
            damageable.TakeDamage(enemy.GetCurrentAttackDamage());
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class NormalEnemyAttackState : EnemyState
{
    protected readonly Enemy_Normal enemy;

    public NormalEnemyAttackState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Normal _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        enemy.StartAttackCooldown();

        Transform player = enemy.GetPlayerTransform();
        if (player != null && (player.position.x - enemy.transform.position.x) * enemy.facingDir < 0)
        {
            enemy.Flip();
        }

        stateTimer = enemy.attackStateDuration;
    }

    public override void Update()
    {
        base.Update();

        if (!enemy.IsPlayerInAttackRange())
        {
            stateMachine.ChangeStates(enemy.moveState);
            return;
        }

        if (!triggerCalled)
        {
            DealDamage();
            triggerCalled = true;
        }

        if (stateTimer < 0f)
        {
            stateMachine.ChangeStates(enemy.idleState);
        }
    }

    protected virtual void DealDamage()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(
            enemy.transform.position + Vector3.right * enemy.attackRange * enemy.facingDir,
            enemy.attackRange,
            enemy.whatCanBeHit
        );

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
            damageable.TakeDamage(enemy.damage);
        }
    }
}

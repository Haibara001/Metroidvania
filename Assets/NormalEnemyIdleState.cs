using UnityEngine;

public class NormalEnemyIdleState : EnemyState
{
    protected readonly Enemy_Normal enemy;

    public NormalEnemyIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Normal _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.idleTime;
    }

    public override void Update()
    {
        base.Update();
        enemy.ZeroVelocity();

        if (enemy.CanStartAttack())
        {
            stateMachine.ChangeStates(enemy.attackState);
            return;
        }

        if (enemy.IsPlayerInAttackRange() && enemy.IsAttackOnCooldown())
        {
            return;
        }

        if (stateTimer < 0f)
        {
            stateMachine.ChangeStates(enemy.moveState);
        }
    }
}

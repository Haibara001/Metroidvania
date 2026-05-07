using UnityEngine;

public class MiniBossIdleState : EnemyState
{
    private readonly Enemy_MiniBoss enemy;

    public MiniBossIdleState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_MiniBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.PlayIdleAnimation();
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

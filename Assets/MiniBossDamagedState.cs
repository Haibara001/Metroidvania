using UnityEngine;

public class MiniBossDamagedState : EnemyState
{
    private readonly Enemy_MiniBoss enemy;

    public MiniBossDamagedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_MiniBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        enemy.PlayDamagedAnimation();
        stateTimer = enemy.GetDamagedStateDuration();
    }

    public override void Update()
    {
        base.Update();
        enemy.ZeroVelocity();

        if (triggerCalled || stateTimer < 0f)
        {
            if (enemy.CanStartAttack())
            {
                stateMachine.ChangeStates(enemy.attackState);
            }
            else
            {
                stateMachine.ChangeStates(enemy.idleState);
            }
        }
    }
}

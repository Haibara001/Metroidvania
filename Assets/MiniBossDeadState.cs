using UnityEngine;

public class MiniBossDeadState : EnemyState
{
    private readonly Enemy_MiniBoss enemy;

    public MiniBossDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_MiniBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        enemy.PlayDeadAnimation();
        stateTimer = enemy.GetDeadStateDuration();
    }

    public override void Update()
    {
        base.Update();
        enemy.ZeroVelocity();

        if (triggerCalled || stateTimer < 0f)
        {
            enemy.FinalizeDeath();
        }
    }
}

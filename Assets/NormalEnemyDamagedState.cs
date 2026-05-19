public class NormalEnemyDamagedState : EnemyState
{
    protected readonly Enemy_Normal enemy;

    public NormalEnemyDamagedState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Normal _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        enemy.PlayDamagedSFX();
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

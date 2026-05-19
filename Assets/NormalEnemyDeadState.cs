public class NormalEnemyDeadState : EnemyState
{
    protected readonly Enemy_Normal enemy;

    public NormalEnemyDeadState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Normal _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.ZeroVelocity();
        enemy.PlayDeadSFX();
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

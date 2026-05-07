using UnityEngine;

public class NormalEnemyMoveState : EnemyState
{
    protected readonly Enemy_Normal enemy;

    public NormalEnemyMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_Normal _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Update()
    {
        base.Update();
        enemy.SetVelocity(enemy.moveSpeed * enemy.facingDir, enemy.rb.velocity.y);

        if (enemy.CanStartAttack())
        {
            stateMachine.ChangeStates(enemy.attackState);
            return;
        }

        if (!enemy.IsGroundDetected() || enemy.IsWallDetected())
        {
            enemy.Flip();
            enemy.ZeroVelocity();
            stateMachine.ChangeStates(enemy.idleState);
        }
    }
}

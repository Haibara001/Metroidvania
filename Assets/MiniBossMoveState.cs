using UnityEngine;

public class MiniBossMoveState : EnemyState
{
    private readonly Enemy_MiniBoss enemy;

    public MiniBossMoveState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName, Enemy_MiniBoss _enemy) : base(_enemyBase, _stateMachine, _animBoolName)
    {
        enemy = _enemy;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.PlayMoveAnimation();
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

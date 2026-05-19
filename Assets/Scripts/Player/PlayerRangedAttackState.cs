using UnityEngine;

public class PlayerRangedAttackState : PlayerState
{
    private bool projectileFired;

    public PlayerRangedAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        projectileFired = false;
        player.ZeroVelocity();
        player.PlayRangedAttackSFX();
        stateTimer = 1f;
    }

    public override void Update()
    {
        base.Update();

        if (triggerCalled || stateTimer < 0f)
        {
            if (!projectileFired)
            {
                player.PerformRangedAttack();
                projectileFired = true;
            }

            stateMachine.changeState(player.idleState);
        }
    }

    public override void AnimationFinishTrigger()
    {
        if (!projectileFired)
        {
            player.PerformRangedAttack();
            projectileFired = true;
        }

        base.AnimationFinishTrigger();
    }
}

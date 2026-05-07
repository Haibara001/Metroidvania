using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (player.ShouldBlockGameplayInput())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && player.CanUseAirJump())
        {
            player.ConsumeAirJump();
            stateMachine.changeState(player.jumpState);
            return;
        }

        if (player.CanUseWallJump() && player.IsWallDetected())
        {
            stateMachine.changeState(player.wallSlideState);
            return;
        }

        if (player.IsGroundDetected())
        {
            stateMachine.changeState(player.idleState);
            return;
        }

        if (xInput != 0)
        {
            player.SetVelocity(player.moveSpeed * .8f * xInput, rb.velocity.y);
        }
    }
}

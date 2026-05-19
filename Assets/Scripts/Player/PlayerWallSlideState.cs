using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.PlayWallSlideSFX();
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

        if (!player.CanUseWallJump())
        {
            stateMachine.changeState(player.airState);
            return;
        }

        if (!player.IsWallDetected())
        {
            stateMachine.changeState(player.airState);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.changeState(player.wallJump);
            return;
        }

        if (xInput != 0 && player.facingDir != xInput)
        {
            stateMachine.changeState(player.idleState);
        }

        if (yInput < 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y * .7f);
        }

        if (player.IsGroundDetected())
        {
            stateMachine.changeState(player.idleState);
        }
    }
}

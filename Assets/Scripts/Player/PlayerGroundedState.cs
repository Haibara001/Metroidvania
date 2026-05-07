using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetAirActions();
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

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            stateMachine.changeState(player.primaryAttack);
        }

        if(player.IsGroundDetected() == false )
        {
            stateMachine.changeState(player.airState);
        }

        if(Input.GetKeyDown(KeyCode.Space) && player.IsGroundDetected())
        {
            stateMachine.changeState(player.jumpState);
        }
    }
}

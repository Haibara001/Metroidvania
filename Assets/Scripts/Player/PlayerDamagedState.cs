using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamagedState : PlayerState
{
    private float knockbackForce = 5f;
    private float knockbackDuration = 0.2f;
    private int facingDirectionBeforeDamage;

    public PlayerDamagedState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        facingDirectionBeforeDamage = player.facingDir;
        player.ZeroVelocity();
        player.SetFacingDirection(facingDirectionBeforeDamage);
        stateTimer = knockbackDuration;
    }

    public override void Exit()
    {
        player.SetFacingDirection(facingDirectionBeforeDamage);
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.SetFacingDirection(facingDirectionBeforeDamage);

        if (stateTimer < 0)
        {
            if (xInput != 0)
            {
                stateMachine.changeState(player.moveState);
            }
            else
            {
                stateMachine.changeState(player.idleState);
            }
        }
    }
}

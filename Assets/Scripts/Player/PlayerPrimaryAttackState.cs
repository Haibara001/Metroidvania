using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    private int combCounter;
    private bool damageTriggered;
    private float lastTimeAttacked;
    private float combWindow = 1f;

    public PlayerPrimaryAttackState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        damageTriggered = false;

        if (combCounter > 2 || Time.time >= lastTimeAttacked + combWindow)
        {
            combCounter = 0;
        }

        player.anim.SetInteger("CombCounter", combCounter);

        float attackDir = player.facingDir;

        if (xInput != 0)
        {
            attackDir = xInput;
        }

        player.SetVelocity(player.attackMovement[combCounter] * attackDir, rb.velocity.y);
        stateTimer = .1f;
    }

    public override void Exit()
    {
        base.Exit();

        player.StartCoroutine("BusyFor", .1f);
        combCounter++;
        lastTimeAttacked = Time.time;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            player.ZeroVelocity();
        }

        if (triggerCalled)
        {
            if (player.HasBufferedDamage())
            {
                player.ConsumeBufferedDamage();
            }
            else
            {
                stateMachine.changeState(player.idleState);
            }
        }
    }

    public override void AnimationFinishTrigger()
    {
        if (!damageTriggered)
        {
            player.PerformPrimaryAttack();
            damageTriggered = true;
        }

        base.AnimationFinishTrigger();
    }
}

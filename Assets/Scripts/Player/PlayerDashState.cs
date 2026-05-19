using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float invincibleTimer;

    public PlayerDashState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = player.dashDuration;
        invincibleTimer = player.dashInvincibleDuration;
        player.isInvincible = true;
        player.SetDashPassThrough(true);
        player.PlayDashSFX();
    }

    public override void Exit()
    {
        base.Exit();
        player.isInvincible = false;
        player.SetVelocity(0, rb.velocity.y);
        player.StartCoroutine(RestoreCollisionAfterDelay());
    }

    public override void Update()
    {
        base.Update();

        if (player.isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                player.isInvincible = false;
            }
        }

        if (!player.IsGroundDetected() && player.IsWallDetected())
        {
            if (player.CanUseWallJump())
            {
                stateMachine.changeState(player.wallSlideState);
            }
            else
            {
                stateMachine.changeState(player.airState);
            }

            return;
        }

        player.SetVelocity(player.dashSpeed * player.dashDir, 0);

        if (stateTimer < 0)
        {
            if (player.IsGroundDetected())
            {
                stateMachine.changeState(player.idleState);
            }
            else
            {
                stateMachine.changeState(player.airState);
            }
        }

    }

    private System.Collections.IEnumerator RestoreCollisionAfterDelay()
    {
        yield return new WaitForSeconds(0.15f);
        player.SetDashPassThrough(false);
    }
}

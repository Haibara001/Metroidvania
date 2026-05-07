using UnityEngine;

public class PlayerEchoSwapState : PlayerState
{
    public PlayerEchoSwapState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ZeroVelocity();
    }

    public override void Update()
    {
        base.Update();
        player.ZeroVelocity();

        if (triggerCalled)
        {
            player.SwapWithEchoFromAnimation();
            stateMachine.changeState(player.GetDefaultLocomotionState());
        }
    }
}

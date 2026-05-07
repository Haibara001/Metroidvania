using UnityEngine;

public class PlayerEchoCastState : PlayerState
{
    public PlayerEchoCastState(Player _player, PlayerStateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName)
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
            player.CreateEchoFromAnimation();
            stateMachine.changeState(player.GetDefaultLocomotionState());
        }
    }
}

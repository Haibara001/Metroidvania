using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemyBase;

    protected bool triggerCalled;
    private string animBoolName;

    protected float stateTimer;

    public EnemyState(Enemy _enemyBase, EnemyStateMachine _stateMachine, string _animBoolName)
    {
        this.stateMachine = _stateMachine;
        this.enemyBase = _enemyBase;
        this.animBoolName = _animBoolName;
    }
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Enter()
    {
        triggerCalled = false;

        if (HasAnimatorBool(animBoolName))
        {
            enemyBase.anim.SetBool(animBoolName, true);
        }
    }

    public virtual void Exit()
    {
        if (HasAnimatorBool(animBoolName))
        {
            enemyBase.anim.SetBool(animBoolName, false);
        }
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }

    private bool HasAnimatorBool(string parameterName)
    {
        if (enemyBase.anim == null || string.IsNullOrEmpty(parameterName))
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = enemyBase.anim.parameters;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Bool && parameters[i].name == parameterName)
            {
                return true;
            }
        }

        return false;
    }

}

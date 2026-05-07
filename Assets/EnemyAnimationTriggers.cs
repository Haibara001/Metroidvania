using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy => GetComponentInParent<Enemy>();

    public void AnimationTrigger()
    {
        if (enemy == null || enemy.stateMachine.currentState == null)
        {
            return;
        }

        enemy.stateMachine.currentState.AnimationFinishTrigger();
    }
}

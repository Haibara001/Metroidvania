using UnityEngine;

public class Enemy_Normal : Enemy
{
    [Header("Targeting")]
    [SerializeField] private int defaultAggroPriority;
    [SerializeField] private float detectionRange = 5f;

    #region States
    public NormalEnemyIdleState idleState { get; private set; }
    public NormalEnemyMoveState moveState { get; private set; }
    public NormalEnemyAttackState attackState { get; private set; }
    public NormalEnemyDamagedState damagedState { get; private set; }
    public NormalEnemyDeadState deadState { get; private set; }
    #endregion

    [Header("Attack Info")]
    public float attackRange = 3f;
    public float attackStateDuration = 0.25f;
    public float attackCooldown = 2f;
    public float damage = 10f;
    public LayerMask whatCanBeHit;

    private Transform playerTransform;
    private IAggroTarget currentTarget;
    private float attackCooldownTimer;

    protected override void Awake()
    {
        base.Awake();

        idleState = new NormalEnemyIdleState(this, stateMachine, "Idle", this);
        moveState = new NormalEnemyMoveState(this, stateMachine, "Move", this);
        attackState = new NormalEnemyAttackState(this, stateMachine, "Attack", this);
        damagedState = new NormalEnemyDamagedState(this, stateMachine, "Damaged", this);
        deadState = new NormalEnemyDeadState(this, stateMachine, "Dead", this);
    }

    protected override void Start()
    {
        base.Start();

        rb.isKinematic = true;
        attackCooldownTimer = 0f;
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        attackCooldownTimer -= Time.deltaTime;

        if (stateMachine.currentState == damagedState || stateMachine.currentState == deadState)
        {
            return;
        }

        DetectPlayer();
    }

    private void DetectPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, whatCanBeHit);
        IAggroTarget bestTarget = null;
        int bestPriority = int.MinValue;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            IAggroTarget target = hits[i].GetComponent<IAggroTarget>();

            if (target == null)
            {
                target = hits[i].GetComponentInParent<IAggroTarget>();
            }

            if (target == null || !target.CanBeTargeted)
            {
                continue;
            }

            int priority = target.AggroPriority + defaultAggroPriority;
            float distance = Vector2.Distance(transform.position, target.AimPoint.position);

            if (bestTarget == null || priority > bestPriority || (priority == bestPriority && distance < closestDistance))
            {
                bestTarget = target;
                bestPriority = priority;
                closestDistance = distance;
            }
        }

        currentTarget = bestTarget;
        playerTransform = currentTarget != null ? currentTarget.AimPoint : null;

        if (CanStartAttack() && stateMachine.currentState != attackState)
        {
            stateMachine.ChangeStates(attackState);
        }
    }

    public bool IsPlayerInAttackRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    public bool CanStartAttack()
    {
        return IsPlayerInAttackRange() && attackCooldownTimer <= 0f;
    }

    public bool IsAttackOnCooldown()
    {
        return attackCooldownTimer > 0f;
    }

    public void StartAttackCooldown()
    {
        attackCooldownTimer = attackCooldown;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public IAggroTarget GetCurrentTarget()
    {
        return currentTarget;
    }

    public override bool TakeDamage(float damageTaken)
    {
        bool hit = base.TakeDamage(damageTaken);

        if (!hit)
        {
            return false;
        }

        if (IsDead())
        {
            if (stateMachine.currentState != deadState)
            {
                stateMachine.ChangeStates(deadState);
            }

            return true;
        }

        if (currentHealth > 0f && stateMachine.currentState != damagedState)
        {
            stateMachine.ChangeStates(damagedState);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Enemy_MiniBoss : Enemy
{
    public enum AttackHitShape
    {
        Circle,
        Box
    }

    [System.Serializable]
    public class BossAttackDefinition
    {
        public string attackName = "Attack";
        public string animationStateName;
        public AttackHitShape hitShape = AttackHitShape.Circle;
        public float moveSpeed;
        public float minRange;
        public float maxRange = 3f;
        [Range(0f, 1f)] public float unlockBelowHealthPercent = 1f;
        public float stateDuration = 0.5f;
        public float triggerDelay = 0.15f;
        public float cooldown = 2f;
        public float damageMultiplier = 1f;
        public Vector2 hitOffset = new Vector2(1.5f, 0f);
        public float hitRadius = 1.5f;
        public Vector2 hitBoxSize = new Vector2(2.5f, 2f);

        [Header("Positioning")]
        public bool teleportToTarget;
        public float teleportOffsetFromTarget = 2f;

        [Header("Projectile")]
        public bool isProjectile;
        public GameObject projectilePrefab;
        public Vector2 projectileSpawnOffset = new Vector2(1f, 0f);
    }

    [Header("Targeting")]
    [SerializeField] private int defaultAggroPriority;
    [SerializeField] private float detectionRange = 7f;

    #region States
    public MiniBossIdleState idleState { get; private set; }
    public MiniBossMoveState moveState { get; private set; }
    public MiniBossAttackState attackState { get; private set; }
    public MiniBossDamagedState damagedState { get; private set; }
    public MiniBossDeadState deadState { get; private set; }
    #endregion

    [Header("Base Attack Info")]
    public float attackRange = 6.5f;
    public float attackStateDuration = 0.25f;
    public float attackCooldown = 2.5f;
    public float damage = 10f;
    public LayerMask whatCanBeHit;

    [Header("Boss Animation Playback")]
    [SerializeField] private string idleAnimationStateName;
    [SerializeField] private string moveAnimationStateName;
    [SerializeField] private string damagedAnimationStateName;
    [SerializeField] private string deadAnimationStateName;

    [Header("Boss Attack Patterns")]
    [SerializeField] private BossAttackDefinition[] bossAttackPatterns;

    private Transform playerTransform;
    private IAggroTarget currentTarget;
    private float attackCooldownTimer;
    private int queuedAttackIndex = -1;
    private int attackCycleCounter;

    protected override void Awake()
    {
        base.Awake();

        idleState = new MiniBossIdleState(this, stateMachine, "Idle", this);
        moveState = new MiniBossMoveState(this, stateMachine, "Move", this);
        attackState = new MiniBossAttackState(this, stateMachine, "Attack", this);
        damagedState = new MiniBossDamagedState(this, stateMachine, "Damaged", this);
        deadState = new MiniBossDeadState(this, stateMachine, "Dead", this);
    }

    protected override void Start()
    {
        base.Start();

        rb.isKinematic = true;
        attackCooldownTimer = 0f;
        queuedAttackIndex = -1;
        attackCycleCounter = 0;
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

        if (playerTransform == null)
        {
            queuedAttackIndex = -1;
            return;
        }

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

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        BossAttackDefinition attack = GetQueuedAttackDefinition();

        if (attack != null)
        {
            float maxAllowedRange = attack.maxRange > 0f ? attack.maxRange : attackRange;
            return distanceToPlayer >= attack.minRange && distanceToPlayer <= maxAllowedRange;
        }

        return IsTargetWithinAnyAttackRange(distanceToPlayer);
    }

    public bool CanStartAttack()
    {
        if (attackCooldownTimer > 0f)
        {
            return false;
        }

        return TryPrepareAttack();
    }

    public bool IsAttackOnCooldown()
    {
        return attackCooldownTimer > 0f;
    }

    public void StartAttackCooldown()
    {
        attackCooldownTimer = GetCurrentAttackCooldown();
    }

    public void CommitQueuedAttack()
    {
        if (queuedAttackIndex >= 0)
        {
            attackCycleCounter++;
        }
    }

    public float GetCurrentAttackDuration()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.stateDuration : attackStateDuration;
    }

    public float GetCurrentAttackTriggerDelay()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? Mathf.Clamp(attack.triggerDelay, 0f, GetCurrentAttackDuration()) : 0f;
    }

    public float GetCurrentAttackDamage()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? damage * attack.damageMultiplier : damage;
    }

    public float GetCurrentAttackMoveSpeed()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.moveSpeed : 0f;
    }

    public bool ShouldTeleportForCurrentAttack()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null && attack.teleportToTarget;
    }

    public float GetCurrentAttackTeleportOffset()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? Mathf.Max(0f, attack.teleportOffsetFromTarget) : 0f;
    }

    public Vector2 GetCurrentAttackCenter()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();

        if (attack == null)
        {
            return (Vector2)transform.position + Vector2.right * attackRange * facingDir;
        }

        Vector2 offset = attack.hitOffset;
        offset.x *= facingDir;
        return (Vector2)transform.position + offset;
    }

    public float GetCurrentAttackRadius()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.hitRadius : attackRange;
    }

    public Vector2 GetCurrentAttackBoxSize()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.hitBoxSize : Vector2.one * attackRange;
    }

    public AttackHitShape GetCurrentAttackHitShape()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.hitShape : AttackHitShape.Circle;
    }

    public bool IsCurrentAttackProjectile()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null && attack.isProjectile && attack.projectilePrefab != null;
    }

    public void SpawnProjectile()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();

        if (attack == null || attack.projectilePrefab == null)
        {
            return;
        }

        Vector2 spawnOffset = attack.projectileSpawnOffset;
        spawnOffset.x *= facingDir;
        Vector3 spawnPos = (Vector2)transform.position + spawnOffset;

        GameObject proj = Instantiate(attack.projectilePrefab, spawnPos, Quaternion.identity);
        WitchThrowing bp = proj.GetComponent<WitchThrowing>();

        if (bp != null)
        {
            Vector2 dir = Vector2.right * facingDir;
            bp.Setup(dir, GetCurrentAttackDamage(), whatCanBeHit);
        }
    }

    public void PlayAttackAnimation()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();

        if (attack != null)
        {
            PlayAnimationState(attack.animationStateName);
        }
    }

    public void PlayIdleAnimation()
    {
        PlayAnimationState(idleAnimationStateName);
    }

    public void PlayMoveAnimation()
    {
        PlayAnimationState(moveAnimationStateName);
    }

    public void PlayDamagedAnimation()
    {
        PlayAnimationState(damagedAnimationStateName);
    }

    public void PlayDeadAnimation()
    {
        PlayAnimationState(deadAnimationStateName);
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

        if (bossAttackPatterns != null && bossAttackPatterns.Length > 0)
        {
            for (int i = 0; i < bossAttackPatterns.Length; i++)
            {
                BossAttackDefinition attack = bossAttackPatterns[i];

                if (attack == null)
                {
                    continue;
                }

                Vector2 center = (Vector2)transform.position + attack.hitOffset;
                Gizmos.color = Color.HSVToRGB((float)i / bossAttackPatterns.Length, 0.8f, 1f);

                if (attack.hitShape == AttackHitShape.Box)
                {
                    Gizmos.DrawWireCube(center, attack.hitBoxSize);
                }
                else
                {
                    Gizmos.DrawWireSphere(center, attack.hitRadius);
                }
            }
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    private bool TryPrepareAttack()
    {
        return TrySelectAttackIndex(out queuedAttackIndex);
    }

    private bool TrySelectAttackIndex(out int selectedIndex)
    {
        selectedIndex = -1;

        if (playerTransform == null)
        {
            return false;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (bossAttackPatterns == null || bossAttackPatterns.Length == 0)
        {
            if (distanceToPlayer <= attackRange)
            {
                selectedIndex = -1;
                return true;
            }

            return false;
        }

        List<int> eligibleIndices = new List<int>();
        float healthPercent = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        for (int i = 0; i < bossAttackPatterns.Length; i++)
        {
            BossAttackDefinition attack = bossAttackPatterns[i];

            if (attack == null)
            {
                continue;
            }

            float maxAllowedRange = attack.maxRange > 0f ? attack.maxRange : attackRange;
            bool isHealthUnlocked = healthPercent <= attack.unlockBelowHealthPercent;

            if (isHealthUnlocked && distanceToPlayer >= attack.minRange && distanceToPlayer <= maxAllowedRange)
            {
                eligibleIndices.Add(i);
            }
        }

        if (eligibleIndices.Count == 0)
        {
            return false;
        }

        // Weighted random: closer attacks get higher weight, with a cycle-based boost to avoid repetition
        float totalWeight = 0f;
        float[] weights = new float[eligibleIndices.Count];

        for (int i = 0; i < eligibleIndices.Count; i++)
        {
            BossAttackDefinition atk = bossAttackPatterns[eligibleIndices[i]];
            float maxAllowedRange = atk.maxRange > 0f ? atk.maxRange : attackRange;
            float midRange = (atk.minRange + maxAllowedRange) * 0.5f;
            float distanceFactor = 1f / (1f + Mathf.Abs(distanceToPlayer - midRange));

            // Boost attacks that haven't been used recently
            float cycleBoost = (attackCycleCounter % bossAttackPatterns.Length == eligibleIndices[i]) ? 1.5f : 1f;

            weights[i] = distanceFactor * cycleBoost;
            totalWeight += weights[i];
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < eligibleIndices.Count; i++)
        {
            cumulative += weights[i];

            if (roll <= cumulative)
            {
                selectedIndex = eligibleIndices[i];
                return true;
            }
        }

        selectedIndex = eligibleIndices[eligibleIndices.Count - 1];
        return true;
    }

    private BossAttackDefinition GetQueuedAttackDefinition()
    {
        if (bossAttackPatterns == null || queuedAttackIndex < 0 || queuedAttackIndex >= bossAttackPatterns.Length)
        {
            return null;
        }

        return bossAttackPatterns[queuedAttackIndex];
    }

    private float GetCurrentAttackCooldown()
    {
        BossAttackDefinition attack = GetQueuedAttackDefinition();
        return attack != null ? attack.cooldown : attackCooldown;
    }

    private bool IsTargetWithinAnyAttackRange(float distanceToPlayer)
    {
        if (bossAttackPatterns == null || bossAttackPatterns.Length == 0)
        {
            return distanceToPlayer <= attackRange;
        }

        float healthPercent = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        for (int i = 0; i < bossAttackPatterns.Length; i++)
        {
            BossAttackDefinition attack = bossAttackPatterns[i];

            if (attack == null || healthPercent > attack.unlockBelowHealthPercent)
            {
                continue;
            }

            float maxAllowedRange = attack.maxRange > 0f ? attack.maxRange : attackRange;

            if (distanceToPlayer >= attack.minRange && distanceToPlayer <= maxAllowedRange)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayAnimationState(string stateName)
    {
        if (anim == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        int stateHash = Animator.StringToHash(stateName);

        if (anim.HasState(0, stateHash))
        {
            anim.Play(stateHash, 0, 0f);
        }
    }
}

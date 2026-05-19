using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class Player : Entity, IDamageable, IAggroTarget
{
    private PlayerStats stats;
    private PlayerProgression progression;
    private PlayerEquipment equipment;

    [Header("Ability progression")]
    [SerializeField] private List<PlayerAbilityType> startingAbilities = new List<PlayerAbilityType>();
    [SerializeField] private int extraAirJumpsWhenUnlocked = 1;
    [SerializeField] private int extraAirDashesWhenUnlocked = 1;
    private readonly HashSet<PlayerAbilityType> unlockedAbilities = new HashSet<PlayerAbilityType>();
    private bool abilitiesLoadedFromSave;
    private int remainingAirJumps;
    private int remainingAirDashes;


    [Header("Attack details")]
    public float[] attackMovement;
    [SerializeField] private Transform attackCheck;
    [SerializeField] private float attackCheckRadius = 1f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private LayerMask whatCanBeHit;
    public bool isBusy { get; private set; }

    [Header("Damage response")]
    [SerializeField] private bool applyHitStunOnDamage = true;
    [SerializeField] private LayerMask contactEnemyLayer;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactDamageCooldown = 0.5f;
    private float contactDamageTimer;

    [Header("Move info")]
    public float moveSpeed = 12f;
    public float jumpForce;

    [Header("Dash info")]
    [SerializeField] private float dashCoolDown;
    private float dashUsageTimer;
    public float dashSpeed;
    public float dashDuration;
    public float dashInvincibleDuration = 0.2f;
    [SerializeField] private LayerMask dashPassThroughLayer;
    public float dashDir { get; private set; }
    public bool isInvincible { get; set; }

    [Header("Ranged attack info")]
    [SerializeField] private KeyCode rangedAttackKey = KeyCode.Mouse1;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpawnOffset = 1f;
    [SerializeField] private float rangedAttackCooldown = 0.5f;
    private float rangedAttackTimer;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxDash;
    [SerializeField] private AudioClip sfxAttack1;
    [SerializeField] private AudioClip sfxAttack2;
    [SerializeField] private AudioClip sfxAttack3;
    [SerializeField] private AudioClip sfxRangedAttack;
    [SerializeField] private AudioClip sfxHit;
    [SerializeField] private AudioClip sfxWallSlide;
    [SerializeField] private AudioClip sfxWallJump;
    [SerializeField] private AudioClip sfxEchoCast;
    [SerializeField] private AudioClip sfxEchoSwap;

    [Header("Echo swap info")]
    [SerializeField] private KeyCode echoSwapKey = KeyCode.Q;
    [SerializeField] private Vector2 echoSpawnOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float echoDuration = 4f;
    [SerializeField] private float echoBurstRadius = 2.5f;
    [SerializeField] private float echoBurstDamage = 20f;
    [SerializeField] private float echoSwapCooldown = 0.35f;
    private float echoSwapTimer;
    private WitchEcho activeEcho;
    private EchoPassage nearbyEchoPassage;
    private bool bufferedDamagePending;
    private float bufferedDamageAmount;









    #region States
    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerIdleState idleState { get; private set; }
    public PlayerMoveState moveState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerAirState airState { get; private set; }
    public PlayerDashState dashState { get; private set; }
    public PlayerWallSlideState wallSlideState { get; private set; }
    public PlayerWallJumpState wallJump { get; private set; }
    public PlayerPrimaryAttackState primaryAttack { get; private set; }
    public PlayerRangedAttackState rangedAttack { get; private set; }
    public PlayerDamagedState damagedState { get; private set; }
    public PlayerEchoCastState echoCastState { get; private set; }
    public PlayerEchoSwapState echoSwapState { get; private set; }

    #endregion

    public Transform AimPoint => transform;
    public int AggroPriority => 0;
    public bool CanBeTargeted => true;


    protected override void Awake()
    {
        base.Awake();

        stats = GetComponent<PlayerStats>();
        progression = GetComponent<PlayerProgression>();
        equipment = GetComponent<PlayerEquipment>();

        stateMachine = new PlayerStateMachine();
        idleState = new PlayerIdleState(this, stateMachine, "Idle");
        moveState = new PlayerMoveState(this, stateMachine, "Move");
        jumpState = new PlayerJumpState(this, stateMachine, "Jump");
        airState = new PlayerAirState(this, stateMachine, "Jump");
        dashState = new PlayerDashState(this, stateMachine, "Dash");
        wallSlideState = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        wallJump = new PlayerWallJumpState(this, stateMachine, "Jump");

        primaryAttack = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        rangedAttack = new PlayerRangedAttackState(this, stateMachine, "RangedAttack");
        damagedState = new PlayerDamagedState(this, stateMachine, "Damaged");
        echoCastState = new PlayerEchoCastState(this, stateMachine, "EchoCast");
        echoSwapState = new PlayerEchoSwapState(this, stateMachine, "EchoSwap");
    }

    protected override void Start()
    {
        base.Start();

        if (!abilitiesLoadedFromSave)
        {
            InitializeAbilities();
        }

        ResetAirActions();
        ApplyDerivedStats();

        if (stats != null)
        {
            stats.StatsChanged += ApplyDerivedStats;
        }

        stateMachine.Initialize(idleState);
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.StatsChanged -= ApplyDerivedStats;
        }
    }

    protected override void Update()
    {
        base.Update();

        contactDamageTimer -= Time.deltaTime;
        stateMachine.currentState.Update();

        if (ShouldBlockGameplayInput())
        {
            return;
        }

        checkForDashInput();
        CheckForRangedAttackInput();
        CheckForEchoSwapInput();
    }

    public IEnumerator BusyFor(float seconds)
    {
        isBusy = true;
        yield return new WaitForSeconds(seconds);

        isBusy = false;
    }

    public bool TakeDamage(float damage)
    {
        if (isInvincible)
        {
            return false;
        }

        if (stats != null)
        {
            stats.ApplyDamage(damage);
        }

        if (stateMachine.currentState == primaryAttack || stateMachine.currentState == rangedAttack)
        {
            bufferedDamagePending = true;
            bufferedDamageAmount = damage;
            return true;
        }

        Debug.Log("Player damaged: " + damage);

        if (applyHitStunOnDamage)
        {
            stateMachine.changeState(damagedState);
        }

        return true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (contactDamageTimer > 0f || isInvincible)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & contactEnemyLayer) == 0)
        {
            return;
        }

        contactDamageTimer = contactDamageCooldown;
        TakeDamage(contactDamage);
    }

    public void SetDashPassThrough(bool ignore)
    {
        int playerLayer = gameObject.layer;
        int mask = dashPassThroughLayer.value;

        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                Physics2D.IgnoreLayerCollision(playerLayer, i, ignore);
            }
        }
    }

    public void AnimationTrigger() => stateMachine.currentState.AnimationFinishTrigger();

    public bool HasAbility(PlayerAbilityType ability) => unlockedAbilities.Contains(ability);

    public bool UnlockAbility(PlayerAbilityType ability)
    {
        Debug.Log($"Player.UnlockAbility called: {ability}, already has it: {unlockedAbilities.Contains(ability)}");

        if (!unlockedAbilities.Add(ability))
        {
            return false;
        }

        ResetAirActions();
        Debug.Log($"Player.UnlockAbility: {ability} unlocked successfully");
        return true;
    }

    public void ResetAirActions()
    {
        remainingAirJumps = HasAbility(PlayerAbilityType.DoubleJump) ? extraAirJumpsWhenUnlocked : 0;
        remainingAirDashes = HasAbility(PlayerAbilityType.AirDash) ? extraAirDashesWhenUnlocked : 0;
    }

    public bool CanUseAirJump() => remainingAirJumps > 0;

    public bool CanUseWallJump() => HasAbility(PlayerAbilityType.WallJump);

    public void ConsumeAirJump()
    {
        if (remainingAirJumps > 0)
        {
            remainingAirJumps--;
        }
    }

    public bool CanDash()
    {
        if (IsGroundDetected())
        {
            return HasAbility(PlayerAbilityType.Dash);
        }

        return HasAbility(PlayerAbilityType.AirDash) && remainingAirDashes > 0;
    }

    public void ConsumeDash()
    {
        if (!IsGroundDetected() && remainingAirDashes > 0)
        {
            remainingAirDashes--;
        }
    }

    private void checkForDashInput()
    {
        if (!HasAbility(PlayerAbilityType.Dash) && !HasAbility(PlayerAbilityType.AirDash))
        {
            return;
        }

        //if(IsWallDetected())
        //{
        //    return;
        //}

        dashUsageTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && dashUsageTimer < 0)
        {
            if (!CanDash())
            {
                return;
            }

            dashUsageTimer = dashCoolDown;
            dashDir = Input.GetAxisRaw("Horizontal");

            if (dashDir == 0)
            {
                dashDir = facingDir;
            }
            ConsumeDash();
            stateMachine.changeState(dashState);
        }
    }

    private void CheckForRangedAttackInput()
    {
        if (!HasAbility(PlayerAbilityType.RangedAttack) || projectilePrefab == null)
        {
            return;
        }

        rangedAttackTimer -= Time.deltaTime;

        if (Input.GetKeyDown(rangedAttackKey) && rangedAttackTimer < 0f)
        {
            rangedAttackTimer = rangedAttackCooldown;
            stateMachine.changeState(rangedAttack);
        }
    }

    private void CheckForEchoSwapInput()
    {
        if (!HasAbility(PlayerAbilityType.EchoSwap))
        {
            return;
        }

        echoSwapTimer -= Time.deltaTime;

        if (!Input.GetKeyDown(echoSwapKey) || echoSwapTimer > 0f)
        {
            return;
        }

        echoSwapTimer = echoSwapCooldown;

        if (activeEcho == null)
        {
            stateMachine.changeState(echoCastState);
            return;
        }

        stateMachine.changeState(echoSwapState);
    }

    private void InitializeAbilities()
    {
        abilitiesLoadedFromSave = false;
        unlockedAbilities.Clear();

        for (int i = 0; i < startingAbilities.Count; i++)
        {
            unlockedAbilities.Add(startingAbilities[i]);
        }
    }

    public void NotifyEchoDestroyed(WitchEcho echo)
    {
        if (activeEcho == echo)
        {
            activeEcho = null;
        }
    }

    public void GainExperience(int amount)
    {
        if (progression == null)
        {
            return;
        }

        int previousLevel = progression.Level;
        progression.GainExperience(amount);

        if (progression.Level != previousLevel)
        {
            ApplyDerivedStats();

            if (stats != null)
            {
                stats.RestoreFullHealth();
            }
        }
    }

    public void AddGold(int amount)
    {
        progression?.AddGold(amount);
    }

    public bool EquipItem(EquipmentItemData itemData)
    {
        if (equipment == null)
        {
            return false;
        }

        equipment.Equip(itemData);
        ApplyDerivedStats();
        return true;
    }

    public bool HasBufferedDamage()
    {
        return bufferedDamagePending;
    }

    public void ConsumeBufferedDamage()
    {
        if (!bufferedDamagePending)
        {
            return;
        }

        bufferedDamagePending = false;
        float damage = bufferedDamageAmount;
        bufferedDamageAmount = 0f;

        Debug.Log("Player damaged: " + damage);

        if (applyHitStunOnDamage)
        {
            stateMachine.changeState(damagedState);
        }
    }

    public void PerformRangedAttack()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.right * facingDir * projectileSpawnOffset;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        WitchThrowing wt = proj.GetComponent<WitchThrowing>();

        if (wt != null)
        {
            Vector2 dir = Vector2.right * facingDir;
            wt.Setup(dir, GetAttackDamage(), whatCanBeHit);
        }
    }

    public void PerformPrimaryAttack()
    {
        Vector3 attackOrigin = attackCheck != null
            ? attackCheck.position
            : transform.position + Vector3.right * facingDir;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackOrigin, attackCheckRadius, whatCanBeHit);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        for (int i = 0; i < hitObjects.Length; i++)
        {
            IDamageable damageable = hitObjects[i].GetComponent<IDamageable>();

            if (damageable == null)
            {
                damageable = hitObjects[i].GetComponentInParent<IDamageable>();
            }

            if (damageable == null || damageable == this || damagedTargets.Contains(damageable))
            {
                continue;
            }

            damagedTargets.Add(damageable);
            damageable.TakeDamage(GetAttackDamage());
        }
    }

    public void CreateEchoFromAnimation()
    {
        Vector3 spawnPosition = transform.position + (Vector3)echoSpawnOffset;

        if (nearbyEchoPassage != null && nearbyEchoPassage.TryGetDestination(out Vector3 passageDestination))
        {
            spawnPosition = passageDestination;
        }

        GameObject echoObject = new GameObject("WitchEcho");
        echoObject.transform.position = spawnPosition;

        activeEcho = echoObject.AddComponent<WitchEcho>();
        activeEcho.Initialize(this, echoDuration, echoBurstRadius, echoBurstDamage);
    }

    public void SwapWithEchoFromAnimation()
    {
        if (activeEcho == null)
        {
            return;
        }

        Vector3 echoPosition = activeEcho.GetSwapPosition();
        transform.position = echoPosition;
        ZeroVelocity();
        activeEcho.TriggerPetalBurst();
    }

    public HashSet<PlayerAbilityType> GetUnlockedAbilities()
    {
        return new HashSet<PlayerAbilityType>(unlockedAbilities);
    }

    public void LoadAbilities(List<int> abilityIds)
    {
        unlockedAbilities.Clear();
        abilitiesLoadedFromSave = true;

        if (abilityIds != null)
        {
            for (int i = 0; i < abilityIds.Count; i++)
            {
                unlockedAbilities.Add((PlayerAbilityType)abilityIds[i]);
            }
        }

        for (int i = 0; i < startingAbilities.Count; i++)
        {
            unlockedAbilities.Add(startingAbilities[i]);
        }

        ResetAirActions();
    }

    public PlayerState GetDefaultLocomotionState()
    {
        return IsGroundDetected() ? idleState : airState;
    }

    public bool ShouldBlockGameplayInput()
    {
        return UI_Inventory.ShouldBlockGameplayInput();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EchoPassage passage = collision.GetComponent<EchoPassage>();

        if (passage == null)
        {
            passage = collision.GetComponentInParent<EchoPassage>();
        }

        if (passage != null)
        {
            nearbyEchoPassage = passage;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EchoPassage passage = collision.GetComponent<EchoPassage>();

        if (passage == null)
        {
            passage = collision.GetComponentInParent<EchoPassage>();
        }

        if (passage != null && nearbyEchoPassage == passage)
        {
            nearbyEchoPassage = null;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXManager.instance != null)
            SFXManager.instance.PlaySFX(clip);
    }

    public void PlayJumpSFX() => PlaySFX(sfxJump);
    public void PlayDashSFX() => PlaySFX(sfxDash);
    public void PlayAttackSFX(int comboIndex)
    {
        switch (comboIndex)
        {
            case 0: PlaySFX(sfxAttack1); break;
            case 1: PlaySFX(sfxAttack2); break;
            case 2: PlaySFX(sfxAttack3); break;
        }
    }
    public void PlayRangedAttackSFX() => PlaySFX(sfxRangedAttack);
    public void PlayHitSFX() => PlaySFX(sfxHit);
    public void PlayWallSlideSFX() => PlaySFX(sfxWallSlide);
    public void PlayWallJumpSFX() => PlaySFX(sfxWallJump);
    public void PlayEchoCastSFX() => PlaySFX(sfxEchoCast);
    public void PlayEchoSwapSFX() => PlaySFX(sfxEchoSwap);

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Vector3 attackOrigin = attackCheck != null
            ? attackCheck.position
            : transform.position + Vector3.right * facingDir;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackOrigin, attackCheckRadius);
    }

    private void ApplyDerivedStats()
    {
        if (stats == null)
        {
            return;
        }

        moveSpeed = stats.MoveSpeed;
        jumpForce = stats.JumpForce;
        dashSpeed = stats.DashSpeed;
    }

    private float GetAttackDamage()
    {
        return stats != null ? stats.AttackPower : attackDamage;
    }

}

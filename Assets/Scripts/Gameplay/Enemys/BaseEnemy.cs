using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Base class for all enemy types - Cuphead/Kirby inspired movement
/// Unity 6 compatible with modular attack system
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    #region Serialized Fields

    [Header("Enemy Settings")]
    [SerializeField] private EnemyData enemyData;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float waitTimeAtWaypoint = 2f;

    [Header("Attack Points")]
    [SerializeField] private Transform[] attackPoints;

    #endregion

    #region Private Fields

    private int currentHealth;
    private PlayerMovement player;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private int currentWaypointIndex = 0;
    private bool isDead = false;
    private bool isPlayerDetected = false;
    private int currentAttackIndex = 0;
    private bool isAttacking = false;
    private float facingDirection = 1f;

    #endregion

    #region Events

    public event Action OnDeath;
    public event Action<AttackData> OnAttackStart;
    public event Action OnAttackEnd;

    #endregion

    #region Public Properties

    public int CurrentHealth => currentHealth;
    public EnemyData EnemyData => enemyData;
    public Rigidbody2D Rb => rb;
    public Animator Animator => animator;
    public Transform PlayerTransform => playerTransform;
    public bool IsDead => isDead;
    public bool IsAttacking => isAttacking;
    public float FacingDirection => facingDirection;

    public bool IsPlayerDetected
    {
        get => isPlayerDetected;
        set => isPlayerDetected = value;
    }

    public Transform[] PatrolWaypoints => patrolWaypoints;
    public float WaitTimeAtWaypoint => waitTimeAtWaypoint;
    public int CurrentWaypointIndex
    {
        get => currentWaypointIndex;
        set => currentWaypointIndex = value;
    }

    public Transform[] AttackPoints => attackPoints;

    public EnemyStatemachine StateMachine { get; private set; }

    public EnemyState.IdleState IdleState { get; private set; }
    public EnemyState.MoveState MoveState { get; private set; }
    public EnemyState.ChaseState ChaseState { get; private set; }
    public EnemyState.AttackState AttackState { get; private set; }
    public EnemyState.DieState DieState { get; private set; }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        InitializeHealth();
        InitializeStateMachine();
    }

    private void Start()
    {
        FindPlayer();
        ApplyAnimatorController();
    }

    private void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        StateMachine?.Update();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        StateMachine?.FixedUpdate();
    }

    private void OnDestroy()
    {
        OnDeath = null;
        OnAttackStart = null;
        OnAttackEnd = null;
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void InitializeHealth()
    {
        currentHealth = enemyData != null ? enemyData.maxHealth : 10;
    }

    private void InitializeStateMachine()
    {
        StateMachine = new EnemyStatemachine();

        IdleState = new EnemyState.IdleState(StateMachine, this);
        MoveState = new EnemyState.MoveState(StateMachine, this);
        ChaseState = new EnemyState.ChaseState(StateMachine, this);
        AttackState = new EnemyState.AttackState(StateMachine, this);
        DieState = new EnemyState.DieState(StateMachine, this);

        StateMachine.Initialize(IdleState);
    }

    private void FindPlayer()
    {
        if (player != null) return;
        player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) playerTransform = player.transform;
    }

    private void ApplyAnimatorController()
    {
        if (animator != null && enemyData != null && enemyData.animator != null)
        {
            animator.runtimeAnimatorController = enemyData.animator;
        }
    }

    #endregion

    #region IDamageable

    public void TakeDamage(int amount, DamageSource source)
    {
        if (isDead || amount < 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        TriggerAnimation("Hit");

        if (currentHealth <= 0) Die();
    }

    #endregion

    #region Movement (Cuphead/Kirby Style)

    public void SetFacingDirection(float direction)
    {
        if (Mathf.Abs(direction) < 0.1f) return;

        facingDirection = Mathf.Sign(direction);
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facingDirection;
        transform.localScale = scale;
    }

    public Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null) return Vector2.zero;
        return ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
    }

    public void MoveTowards(Vector2 targetPosition, float speed, bool smoothing = true)
    {
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPosition - currentPos).normalized;

        if (enemyData.canFly)
        {
            rb.gravityScale = 0f;
            Vector2 targetVelocity = direction * speed;

            if (smoothing)
            {
                // Smooth sine-wave movement (Cuphead-style)
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 8f);
            }
            else
            {
                rb.linearVelocity = targetVelocity;
            }
        }
        else if (enemyData.canWalk)
        {
            rb.gravityScale = 1f;
            float targetVelocityX = direction.x * speed;

            if (smoothing)
            {
                float currentVelocityX = rb.linearVelocity.x;
                float smoothedVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, Time.fixedDeltaTime * 10f);
                rb.linearVelocity = new Vector2(smoothedVelocityX, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
            }
        }

        if (Mathf.Abs(direction.x) > 0.1f) SetFacingDirection(direction.x);
    }

    public void StopMovement(bool immediate = false)
    {
        if (rb == null) return;

        if (immediate)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 12f);
        }
    }

    #endregion

    #region Attack System

    public AttackData GetAvailableAttack()
    {
        if (enemyData == null || enemyData.attacks == null || enemyData.attacks.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No attacks available in EnemyData!", this);
            return null;
        }

        float distanceToPlayer = GetDistanceToPlayer();
        List<AttackData> validAttacks = new List<AttackData>();

        foreach (var attack in enemyData.attacks)
        {
            if (attack == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Null attack in attacks list!", this);
                continue;
            }

            bool inRange = distanceToPlayer <= attack.range;
            bool hasLOS = !attack.requiresLineOfSight || HasLineOfSightToPlayer();

            Debug.Log($"[{gameObject.name}] Checking attack '{attack.attackName}': Range={attack.range}, Distance={distanceToPlayer:F2}, InRange={inRange}, HasLOS={hasLOS}", this);

            if (inRange && hasLOS)
            {
                validAttacks.Add(attack);
            }
        }

        if (validAttacks.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] No valid attacks found at distance {distanceToPlayer:F2}", this);
            return null;
        }

        currentAttackIndex = (currentAttackIndex + 1) % validAttacks.Count;
        AttackData selectedAttack = validAttacks[currentAttackIndex];

        Debug.Log($"[{gameObject.name}] Selected attack: {selectedAttack.attackName}", this);
        return selectedAttack;
    }

    public bool HasLineOfSightToPlayer()
    {
        if (playerTransform == null) return false;

        Vector2 direction = GetDirectionToPlayer();
        float distance = GetDistanceToPlayer();

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            distance,
            LayerMask.GetMask("Ground", "Obstacles")
        );

        return hit.collider == null;
    }

    public void PerformAttack(AttackData attackData)
    {
        if (attackData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PerformAttack called with null AttackData!", this);
            return;
        }

        if (isAttacking)
        {
            Debug.Log($"[{gameObject.name}] Already attacking, ignoring new attack request.", this);
            return;
        }

        Debug.Log($"[{gameObject.name}] Performing attack: {attackData.attackName} (Type: {attackData.attackType})", this);

        isAttacking = true;
        OnAttackStart?.Invoke(attackData);

        if (!string.IsNullOrEmpty(attackData.attackName))
        {
            TriggerAnimation(attackData.attackName);
        }

        switch (attackData.attackType)
        {
            case EnemyAttackType.Projectile:
                StartCoroutine(ProjectileAttackCoroutine(attackData));
                break;

            case EnemyAttackType.Slap:
            case EnemyAttackType.Poke:
                StartCoroutine(MeleeAttackCoroutine(attackData));
                break;

            case EnemyAttackType.Laser:
            case EnemyAttackType.DualLaser:
                StartCoroutine(LaserAttackCoroutine(attackData));
                break;

            default:
                Debug.LogWarning($"[{gameObject.name}] Unknown attack type: {attackData.attackType}", this);
                EndAttack();
                break;
        }
    }

    private System.Collections.IEnumerator ProjectileAttackCoroutine(AttackData attackData)
    {
        yield return new WaitForSeconds(attackData.windupTime);

        if (attackData.attackEffectPrefab != null)
        {
            Transform spawnPoint = GetAttackPoint(attackData);
            Vector2 direction = GetDirectionToPlayer();

            GameObject projectile = Instantiate(
                attackData.attackEffectPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

            // Setup EnemyProjectileController
            var enemyProjectile = projectile.GetComponent<EnemyProjectileController>();
            if (enemyProjectile != null)
            {
                enemyProjectile.SetProjectileType(EnemyProjectileController.ProjectileType.Bullet);

                // Get speed from BulletData
                var bulletData = enemyProjectile.GetBulletData();
                float speed = bulletData != null ? bulletData.projectileSpeed : 10f;

                // Set velocity
                Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
                if (projectileRb != null)
                {
                    projectileRb.linearVelocity = direction * speed;
                }

                // Rotate projectile to face direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Projectile prefab missing EnemyProjectileController!", this);
            }
        }

        yield return new WaitForSeconds(attackData.attackDuration);
        EndAttack();
    }

    private System.Collections.IEnumerator MeleeAttackCoroutine(AttackData attackData)
    {
        // Special behavior for Poke attack on flying enemies
        bool isDiveAttack = (attackData.attackType == EnemyAttackType.Poke) && enemyData.canFly;

        Vector2 originalPosition = transform.position;
        Vector2 playerPosition = playerTransform != null ? playerTransform.position : transform.position;

        if (isDiveAttack)
        {
            // DIVE PHASE: Sturzflug zum Spieler
            Debug.Log($"[{gameObject.name}] Starting dive attack!", this);

            // Calculate dive target (slightly above player)
            Vector2 diveTarget = playerPosition + Vector2.up * 0.5f;

            // Dive down quickly
            float diveSpeed = enemyData.moveSpeed * 2.5f; // Schneller Sturzflug
            float diveTime = 0f;
            float maxDiveTime = attackData.windupTime;

            while (diveTime < maxDiveTime)
            {
                diveTime += Time.deltaTime;

                // Move towards player
                Vector2 currentPos = transform.position;
                Vector2 direction = (diveTarget - currentPos).normalized;

                rb.gravityScale = 0f;
                rb.linearVelocity = direction * diveSpeed;

                // Update facing direction
                SetFacingDirection(direction.x);

                yield return null;
            }

            // Stop at dive position
            StopMovement(immediate: true);
        }
        else
        {
            // Normal melee windup
            yield return new WaitForSeconds(attackData.windupTime);
        }

        // DAMAGE PHASE: Deal damage if in range
        bool dealedDamage = false;
        if (GetDistanceToPlayer() <= attackData.range)
        {
            var damageable = playerTransform?.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackData.damage, DamageSource.Enemy);
                dealedDamage = true;
                Debug.Log($"[{gameObject.name}] Dive attack hit! Damage: {attackData.damage}", this);
            }

            // Spawn effect
            if (attackData.attackEffectPrefab != null)
            {
                Transform spawnPoint = GetAttackPoint(attackData);
                Instantiate(attackData.attackEffectPrefab, spawnPoint.position, Quaternion.identity);
            }
        }

        // Attack duration (recovery)
        yield return new WaitForSeconds(attackData.attackDuration);

        if (isDiveAttack && dealedDamage)
        {
            // RETREAT PHASE: Fliege zurück nach oben
            Debug.Log($"[{gameObject.name}] Retreating after dive!", this);

            // Calculate retreat position (above original position)
            Vector2 retreatTarget = originalPosition + Vector2.up * 1.5f;

            float retreatSpeed = enemyData.moveSpeed * 1.5f;
            float retreatTime = 0f;
            float maxRetreatTime = 1.0f;

            while (retreatTime < maxRetreatTime)
            {
                retreatTime += Time.deltaTime;

                Vector2 currentPos = transform.position;
                Vector2 direction = (retreatTarget - currentPos).normalized;

                rb.gravityScale = 0f;
                rb.linearVelocity = direction * retreatSpeed;

                // Check if reached retreat position
                if (Vector2.Distance(currentPos, retreatTarget) < 0.5f)
                {
                    break;
                }

                yield return null;
            }

            // Smooth stop
            StopMovement(immediate: false);
        }

        EndAttack();
    }

    private System.Collections.IEnumerator LaserAttackCoroutine(AttackData attackData)
    {
        yield return new WaitForSeconds(attackData.windupTime);

        GameObject laser = null;
        if (attackData.attackEffectPrefab != null)
        {
            Transform spawnPoint = GetAttackPoint(attackData);
            laser = Instantiate(attackData.attackEffectPrefab, spawnPoint.position, Quaternion.identity, transform);

            // Setup EnemyProjectileController for laser
            var enemyProjectile = laser.GetComponent<EnemyProjectileController>();
            if (enemyProjectile != null)
            {
                enemyProjectile.SetProjectileType(EnemyProjectileController.ProjectileType.Laser);
            }

            // Point laser at player
            Vector2 direction = GetDirectionToPlayer();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            laser.transform.rotation = Quaternion.Euler(0, 0, angle);

            // For dual laser, spawn second laser
            if (attackData.attackType == EnemyAttackType.DualLaser && attackPoints != null && attackPoints.Length > 1)
            {
                GameObject laser2 = Instantiate(attackData.attackEffectPrefab, attackPoints[1].position, Quaternion.identity, transform);
                var enemyProjectile2 = laser2.GetComponent<EnemyProjectileController>();
                if (enemyProjectile2 != null)
                {
                    enemyProjectile2.SetProjectileType(EnemyProjectileController.ProjectileType.Laser);
                }
                laser2.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Destroy second laser after duration
                Destroy(laser2, attackData.attackDuration);
            }
        }

        yield return new WaitForSeconds(attackData.attackDuration);

        if (laser != null) Destroy(laser);
        EndAttack();
    }

    private void EndAttack()
    {
        Debug.Log($"[{gameObject.name}] Attack ended.", this);
        isAttacking = false;
        OnAttackEnd?.Invoke();
    }

    private Transform GetAttackPoint(AttackData attackData)
    {
        if (attackData.attackPoints != null && attackData.attackPoints.Length > 0)
            return attackData.attackPoints[0];

        if (attackPoints != null && attackPoints.Length > 0)
            return attackPoints[0];

        return transform;
    }

    #endregion

    #region Waypoint Management

    public Transform GetCurrentWaypoint()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return null;
        if (currentWaypointIndex < 0 || currentWaypointIndex >= patrolWaypoints.Length)
            currentWaypointIndex = 0;
        return patrolWaypoints[currentWaypointIndex];
    }

    public void NextWaypoint()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
    }

    #endregion

    #region Player Detection

    public bool IsPlayerInDetectionRange()
    {
        if (playerTransform == null || enemyData == null) return false;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= enemyData.detectionRange;

        if (inRange && !isPlayerDetected) isPlayerDetected = true;
        return inRange;
    }

    public bool IsPlayerInChaseRange()
    {
        if (playerTransform == null || enemyData == null || !isPlayerDetected) return false;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= enemyData.chaseRange;

        if (!inRange) isPlayerDetected = false;
        return inRange;
    }

    public bool IsPlayerInAttackRange(float range)
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= range;
    }

    public float GetDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;
        return Vector2.Distance(transform.position, playerTransform.position);
    }

    #endregion

    #region Animation

    public void TriggerAnimation(string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
            animator.SetTrigger(triggerName);
    }

    public void SetAnimationBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
            animator.SetBool(paramName, value);
    }

    public void SetAnimationFloat(string paramName, float value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName))
            animator.SetFloat(paramName, value);
    }

    #endregion

    #region Death

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        StateMachine?.ChangeState(DieState);
        OnDeath?.Invoke();

        if (player != null) player.OnEnemyKilled();
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.detectionRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, enemyData.chaseRange);

        if (enemyData.attacks != null)
        {
            Gizmos.color = Color.red;
            foreach (var attack in enemyData.attacks)
            {
                if (attack != null)
                    Gizmos.DrawWireSphere(transform.position, attack.range);
            }
        }

        if (patrolWaypoints != null && patrolWaypoints.Length > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                if (patrolWaypoints[i] == null) continue;
                Gizmos.DrawWireSphere(patrolWaypoints[i].position, 0.3f);

                int nextIndex = (i + 1) % patrolWaypoints.Length;
                if (patrolWaypoints[nextIndex] != null)
                    Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[nextIndex].position);
            }
        }
    }

    #endregion
}
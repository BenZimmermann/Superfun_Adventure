using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cuphead-inspired Boss Controller — Unity 6, 2D Jump & Run Shooter.
///
/// Attack Pattern System:
///  • CodeRain, Fireballs and Spikes each have a List of AttackPatterns.
///  • Each pattern defines WHICH spawn points fire simultaneously.
///  • Per-phase, new patterns are unlocked and added to the pool.
///  • One random pattern is picked each time that attack fires.
///  • Attacks stack across phases (all previous attacks remain).
///  • Attack interval decreases per phase.
///  • Walls shrink exactly once (Phase 3).
/// </summary>
public class BossController : MonoBehaviour, IDamageable
{
    // ═════════════════════════════════════════════
    //  INSPECTOR
    // ═════════════════════════════════════════════

    [Header("=== Health & Phases ===")]
    [SerializeField] private int maxHealth = 400;
    [SerializeField] private float[] phaseThresholds = { 0.75f, 0.50f, 0.25f };

    [Header("=== Boss Arms ===")]
    [Tooltip("4 arms removed one-per-phase (index 0 = first removed).")]
    [SerializeField] private List<GameObject> bossArms = new List<GameObject>();

    // ─── Prefabs ──────────────────────────────────
    [Header("=== Prefabs ===")]
    [SerializeField] private GameObject codeRainProjectilePrefab;
    [SerializeField] private GameObject fireballProjectilePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject enemyPrefab;

    // ─────────────────────────────────────────────────────────────────
    //  CODE RAIN PATTERNS
    //  Each pattern = a named set of spawn points that fire together.
    //  Add patterns freely. Per-phase patterns are unlocked progressively.
    //
    //  Example setup:
    //    Phase1CodeRainPatterns[0] = "Narrow"   → 2 center points
    //    Phase1CodeRainPatterns[1] = "Wide"      → 5 spread points
    //    Phase2CodeRainPatterns[0] = "Dense"     → 7 points, short delay
    //    etc.
    // ─────────────────────────────────────────────────────────────────
    [Header("=== Code Rain Patterns ===")]
    [Tooltip("Global default bursts (can be overridden per pattern).")]
    [SerializeField] private int codeRainDefaultBursts = 5;
    [SerializeField] private float codeRainDefaultBurstDelay = 0.25f;

    [Tooltip("Patterns available from Phase 1.")]
    [SerializeField] private List<AttackPattern> codeRainPatternsPhase1 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 2.")]
    [SerializeField] private List<AttackPattern> codeRainPatternsPhase2 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 3.")]
    [SerializeField] private List<AttackPattern> codeRainPatternsPhase3 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 4.")]
    [SerializeField] private List<AttackPattern> codeRainPatternsPhase4 = new List<AttackPattern>();

    // ─────────────────────────────────────────────────────────────────
    //  FIREBALL PATTERNS
    // ─────────────────────────────────────────────────────────────────
    [Header("=== Fireball Patterns ===")]
    [Tooltip("Patterns available from Phase 1 (e.g. left+right, single side).")]
    [SerializeField] private List<AttackPattern> fireballPatternsPhase1 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 2.")]
    [SerializeField] private List<AttackPattern> fireballPatternsPhase2 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 3.")]
    [SerializeField] private List<AttackPattern> fireballPatternsPhase3 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 4.")]
    [SerializeField] private List<AttackPattern> fireballPatternsPhase4 = new List<AttackPattern>();

    // ─────────────────────────────────────────────────────────────────
    //  SPIKE PATTERNS
    // ─────────────────────────────────────────────────────────────────
    [Header("=== Spike Patterns ===")]
    [SerializeField] private float spikeDuration = 3f;

    [Tooltip("Patterns available from Phase 3.")]
    [SerializeField] private List<AttackPattern> spikePatternsPhase3 = new List<AttackPattern>();
    [Tooltip("Additional patterns unlocked in Phase 4.")]
    [SerializeField] private List<AttackPattern> spikePatternsPhase4 = new List<AttackPattern>();

    // ─── Laser ────────────────────────────────────
    [Header("=== Laser ===")]
    [SerializeField] private Transform laserOrigin;
    [SerializeField] private float laserDuration = 2.5f;

    // ─── Enemies ──────────────────────────────────
    [Header("=== Enemy Spawning ===")]
    [SerializeField] private EnemySpawnController enemySpawnController;
    [SerializeField] private Transform[] enemySpawnPoints;

    // ─── Random Object Spawner ────────────────────
    [Header("=== Random Object Spawner ===")]
    [Tooltip("A random prefab from this list spawns at a random position within spawnRadius.")]
    [SerializeField] private List<GameObject> randomSpawnObjects = new List<GameObject>();
    [SerializeField] private Transform randomSpawnCenter;
    [SerializeField] private float randomSpawnRadius = 5f;
    [SerializeField] private float randomSpawnFrequency = 4f;
    [Tooltip("spawnFrequency multiplied by this per phase (e.g. 0.8 = 20% faster).")]
    [SerializeField] private float randomSpawnPhaseMultiplier = 0.8f;
    [SerializeField] private bool randomSpawnerActive = false;

    // ─── Attack Timing ────────────────────────────
    [Header("=== Attack Timing ===")]
    [Tooltip("Seconds between attacks in Phase 1.")]
    [SerializeField] private float baseAttackInterval = 3.5f;
    [Tooltip("Multiplied per phase (e.g. 0.75 = 25% faster each phase).")]
    [SerializeField] private float attackIntervalPhaseMultiplier = 0.75f;
    [SerializeField] private float minAttackInterval = 0.8f;

    // ─── Boss Dodge ───────────────────────────────
    [Header("=== Boss Dodge ===")]
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeInterval = 6f;
    [SerializeField] private SpriteRenderer bossSpriteRenderer;

    // ─── Environment & UI ─────────────────────────
    [Header("=== Room Environment ===")]
    [SerializeField] private BossEnvironment bossEnvironment;

    [Header("=== UI ===")]
    [SerializeField] private BossHealthBar bossHealthBar;

    [Header("=== Bug Effects ===")]
    [Tooltip("Assign the BossBugEffectController component on this same GameObject.")]
    [SerializeField] private BossBugEffectController bugEffectController;

    // ═════════════════════════════════════════════
    //  PRIVATE STATE
    // ═════════════════════════════════════════════

    private int currentHealth;
    private int currentPhase = 0;
    private bool isTransitioning = false;
    private bool isDodging = false;
    private bool wallsAlreadyShrunk = false;

    private float currentAttackInterval;
    private float currentSpawnFrequency;

    private Coroutine attackLoopCoroutine;
    private Coroutine dodgeLoopCoroutine;
    private Coroutine randomSpawnCoroutine;

    /// <summary>
    /// When true the boss idles silently — no attacks, no dodge, invulnerable.
    /// Set to false by calling Activate() (done by BossRoomTrigger).
    /// </summary>
    private bool isDormant = true;

    // ═════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═════════════════════════════════════════════

    private void Start()
    {
        currentHealth = maxHealth;
        currentAttackInterval = baseAttackInterval;
        currentSpawnFrequency = randomSpawnFrequency;

        bossHealthBar?.Initialize(maxHealth);
        bossEnvironment?.SetPhase(1);

        // Boss waits for Activate() — loops start paused via isDormant check
        attackLoopCoroutine = StartCoroutine(AttackLoop());
        dodgeLoopCoroutine = StartCoroutine(DodgeLoop());

        if (randomSpawnerActive)
            randomSpawnCoroutine = StartCoroutine(RandomSpawnLoop());
    }

    // ═════════════════════════════════════════════
    //  ACTIVATION — called by BossRoomTrigger
    // ═════════════════════════════════════════════

    /// <summary>
    /// Called by BossRoomTrigger after the UI has faded in.
    /// Releases the boss from its dormant state so it starts attacking.
    /// </summary>
    public void Activate()
    {
        if (!isDormant) return;
        isDormant = false;
        Debug.Log("[Boss] Activated — starting attacks.");
    }

    // ═════════════════════════════════════════════
    //  IDAMAGEABLE
    // ═════════════════════════════════════════════

    public void TakeDamage(int amount, DamageSource source)
    {
        if (isDormant) return;
        if (source != DamageSource.Player) return;
        if (IsInvulnerable()) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        bossHealthBar?.UpdateHealth(currentHealth);

        // Reset player psychosis on every hit — same as killing any enemy
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        playerGO?.GetComponent<PlayerMovement>()?.OnEnemyKilled();

        CheckPhaseTransition();
        if (currentHealth <= 0) Die();
    }

    // ═════════════════════════════════════════════
    //  PHASE TRANSITIONS
    // ═════════════════════════════════════════════

    private void CheckPhaseTransition()
    {
        float ratio = (float)currentHealth / maxHealth;
        if (currentPhase < phaseThresholds.Length && ratio <= phaseThresholds[currentPhase])
            StartCoroutine(TransitionToPhase(currentPhase + 2));
    }

    private IEnumerator TransitionToPhase(int newPhase)
    {
        isTransitioning = true;
        if (attackLoopCoroutine != null) StopCoroutine(attackLoopCoroutine);

        yield return StartCoroutine(PlayTransitionEffect());

        RemoveArm(newPhase - 2);
        currentPhase = newPhase - 1; // store 0-based

        // Scale timings
        currentAttackInterval = Mathf.Max(
            minAttackInterval,
            baseAttackInterval * Mathf.Pow(attackIntervalPhaseMultiplier, currentPhase));

        currentSpawnFrequency = randomSpawnFrequency
            * Mathf.Pow(randomSpawnPhaseMultiplier, currentPhase);

        bossEnvironment?.SetPhase(newPhase);
        bugEffectController?.OnPhaseChanged(newPhase);

        // Walls shrink exactly once at Phase 3
        if (newPhase == 3 && !wallsAlreadyShrunk)
        {
            bossEnvironment?.ShrinkWalls();
            wallsAlreadyShrunk = true;
        }

        // Activate random spawner from Phase 3 onward
        if (currentPhase >= 2 && !randomSpawnerActive)
        {
            randomSpawnerActive = true;
            randomSpawnCoroutine = StartCoroutine(RandomSpawnLoop());
        }

        Debug.Log($"[Boss] → Phase {newPhase} | interval={currentAttackInterval:F2}s");

        isTransitioning = false;
        attackLoopCoroutine = StartCoroutine(AttackLoop());
    }

    // ═════════════════════════════════════════════
    //  ARM MANAGEMENT
    // ═════════════════════════════════════════════

    private void RemoveArm(int index)
    {
        if (index < 0 || index >= bossArms.Count || bossArms[index] == null) return;
        Destroy(bossArms[index]);
        bossArms[index] = null;
        Debug.Log($"[Boss] Arm {index} removed.");
    }

    // ═════════════════════════════════════════════
    //  ATTACK LOOP  (cumulative pool per phase)
    // ═════════════════════════════════════════════

    private IEnumerator AttackLoop()
    {
        // Wait until Activate() is called
        yield return new WaitUntil(() => !isDormant);

        while (true)
        {
            yield return new WaitForSeconds(currentAttackInterval);
            if (isTransitioning || isDodging) continue;
            yield return StartCoroutine(PickAndExecuteAttack());
        }
    }

    private IEnumerator PickAndExecuteAttack()
    {
        var pool = new List<System.Func<IEnumerator>>();

        // Phase 1 always available
        pool.Add(AttackCodeRain);
        pool.Add(AttackFireballs);

        if (currentPhase >= 1) pool.Add(AttackLaser);
        if (currentPhase >= 2) pool.Add(AttackSpikes);
        if (currentPhase >= 3) pool.Add(AttackSpawnEnemies);

        yield return StartCoroutine(pool[Random.Range(0, pool.Count)]());
    }

    // ═════════════════════════════════════════════
    //  PATTERN HELPERS
    // ═════════════════════════════════════════════

    /// Builds the cumulative pattern pool up to the current phase.
    private List<AttackPattern> GetPatternPool(
        List<AttackPattern> p1,
        List<AttackPattern> p2,
        List<AttackPattern> p3,
        List<AttackPattern> p4)
    {
        var pool = new List<AttackPattern>(p1);
        if (currentPhase >= 1) pool.AddRange(p2);
        if (currentPhase >= 2) pool.AddRange(p3);
        if (currentPhase >= 3) pool.AddRange(p4);
        return pool;
    }

    /// Picks a random pattern from the pool. Returns null if pool is empty.
    private AttackPattern PickPattern(List<AttackPattern> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    // ═════════════════════════════════════════════
    //  ATTACK MODULES
    // ═════════════════════════════════════════════

    /// Code Rain — picks a random pattern, fires all its points simultaneously,
    /// repeats for burst count, then waits burst delay between each burst.
    private IEnumerator AttackCodeRain()
    {
        var pool = GetPatternPool(codeRainPatternsPhase1, codeRainPatternsPhase2,
                                     codeRainPatternsPhase3, codeRainPatternsPhase4);
        var pattern = PickPattern(pool);

        if (pattern == null || codeRainProjectilePrefab == null)
        { yield return new WaitForSeconds(1f); yield break; }

        int bursts = pattern.burstCountOverride > 0
            ? pattern.burstCountOverride : codeRainDefaultBursts;
        float delay = pattern.burstDelayOverride > 0f
            ? pattern.burstDelayOverride : codeRainDefaultBurstDelay;

        Debug.Log($"[Boss] Code Rain: '{pattern.patternName}' ({pattern.spawnPoints.Count} points, {bursts} bursts)");

        for (int i = 0; i < bursts; i++)
        {
            // All spawn points in the pattern fire at the same time
            foreach (var sp in pattern.spawnPoints)
                if (sp != null)
                    Instantiate(codeRainProjectilePrefab, sp.position, Quaternion.identity);

            yield return new WaitForSeconds(delay);
        }
    }

    /// Fireballs — picks a random pattern, fires all points simultaneously.
    private IEnumerator AttackFireballs()
    {
        var pool = GetPatternPool(fireballPatternsPhase1, fireballPatternsPhase2,
                                     fireballPatternsPhase3, fireballPatternsPhase4);
        var pattern = PickPattern(pool);

        if (pattern == null || fireballProjectilePrefab == null)
        { yield return new WaitForSeconds(1f); yield break; }

        Debug.Log($"[Boss] Fireballs: '{pattern.patternName}' ({pattern.spawnPoints.Count} points)");

        foreach (var sp in pattern.spawnPoints)
            if (sp != null)
                Instantiate(fireballProjectilePrefab, sp.position, Quaternion.identity);

        yield return new WaitForSeconds(1f);
    }

    /// Laser — spawned at laserOrigin, auto-tracks via LaserBeam script.
    private IEnumerator AttackLaser()
    {
        Debug.Log("[Boss] Attack: Laser");
        if (laserPrefab != null && laserOrigin != null)
        {
            GameObject laser = Instantiate(laserPrefab, laserOrigin.position, Quaternion.identity);
            laser.transform.SetParent(laserOrigin);
            yield return new WaitForSeconds(laserDuration);
            Destroy(laser);
        }
        else
            yield return new WaitForSeconds(laserDuration);
    }

    /// Spikes — picks a random pattern, all points rise simultaneously,
    /// then despawn after spikeDuration.
    private IEnumerator AttackSpikes()
    {
        var pool = GetPatternPool(new List<AttackPattern>(), new List<AttackPattern>(),
                                     spikePatternsPhase3, spikePatternsPhase4);
        var pattern = PickPattern(pool);

        if (pattern == null || spikePrefab == null)
        { yield return new WaitForSeconds(spikeDuration); yield break; }

        Debug.Log($"[Boss] Spikes: '{pattern.patternName}' ({pattern.spawnPoints.Count} points)");

        var spawned = new List<GameObject>();
        foreach (var sp in pattern.spawnPoints)
            if (sp != null)
                spawned.Add(Instantiate(spikePrefab, sp.position, Quaternion.identity));

        yield return new WaitForSeconds(spikeDuration);

        foreach (var s in spawned) if (s != null) Destroy(s);
    }

    /// Enemies — respects EnemySpawnController cap (max 2).
    private IEnumerator AttackSpawnEnemies()
    {
        Debug.Log("[Boss] Attack: Spawn Enemies");
        foreach (var sp in enemySpawnPoints)
        {
            if (enemyPrefab == null) continue;
            if (enemySpawnController != null)
                enemySpawnController.TrySpawnEnemy(enemyPrefab, sp.position);
            else
                Instantiate(enemyPrefab, sp.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(2f);
    }

    // ═════════════════════════════════════════════
    //  RANDOM OBJECT SPAWNER
    // ═════════════════════════════════════════════

    private IEnumerator RandomSpawnLoop()
    {
        yield return new WaitUntil(() => !isDormant);

        while (true)
        {
            yield return new WaitForSeconds(currentSpawnFrequency);
            if (isTransitioning || randomSpawnObjects.Count == 0) continue;

            GameObject prefab = randomSpawnObjects[Random.Range(0, randomSpawnObjects.Count)];
            if (prefab == null) continue;

            Vector3 center = randomSpawnCenter != null ? randomSpawnCenter.position : transform.position;
            Vector2 offset = Random.insideUnitCircle * randomSpawnRadius;
            Vector3 spawnPos = center + new Vector3(offset.x, offset.y, 0f);

            Instantiate(prefab, spawnPos, Quaternion.identity);
            Debug.Log($"[RandomSpawner] '{prefab.name}' at {spawnPos}");
        }
    }

    // ═════════════════════════════════════════════
    //  BOSS DODGE
    // ═════════════════════════════════════════════

    private IEnumerator DodgeLoop()
    {
        yield return new WaitUntil(() => !isDormant);

        while (true)
        {
            yield return new WaitForSeconds(dodgeInterval);
            if (!isTransitioning)
                yield return StartCoroutine(DodgeForegroundBackground());
        }
    }

    private IEnumerator DodgeForegroundBackground()
    {
        isDodging = true;
        yield return StartCoroutine(ScaleBoss(0.75f, dodgeDuration));
        SetBossInvulnerable(true);
        yield return new WaitForSeconds(dodgeDuration + 0.5f);
        SetBossInvulnerable(false);
        yield return StartCoroutine(ScaleBoss(1f, dodgeDuration));
        isDodging = false;

        // Fire a random bug effect as the boss returns — feels reactive and chaotic
        bugEffectController?.TriggerRandomBugEffect();
    }

    private IEnumerator ScaleBoss(float targetScale, float duration)
    {
        Vector3 start = transform.localScale;
        Vector3 end = new Vector3(Mathf.Sign(start.x) * targetScale, targetScale, targetScale);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        transform.localScale = end;
        if (bossSpriteRenderer != null)
        {
            Color c = bossSpriteRenderer.color;
            c.a = targetScale < 1f ? 0.4f : 1f;
            bossSpriteRenderer.color = c;
        }
    }

    // ═════════════════════════════════════════════
    //  INVULNERABILITY
    // ═════════════════════════════════════════════

    private bool isInvulnerable = false;
    private void SetBossInvulnerable(bool v) => isInvulnerable = v;
    public bool IsInvulnerable() => isInvulnerable || isTransitioning;

    // ═════════════════════════════════════════════
    //  TRANSITION EFFECT HOOK
    // ═════════════════════════════════════════════

    private IEnumerator PlayTransitionEffect()
    {
        Debug.Log("[Boss] Phase transition...");
        yield return new WaitForSeconds(1.5f);
    }

    // ═════════════════════════════════════════════
    //  DEATH
    // ═════════════════════════════════════════════

    private void Die()
    {
        Debug.Log("[Boss] Defeated!");
        StopAllCoroutines();
        bossEnvironment?.OnBossDefeated();

        // Reset player psychosis — same as killing any enemy
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        playerGO?.GetComponent<PlayerMovement>()?.OnEnemyKilled();

        // Mark level as finished
        if (SaveManager.Instance != null)
        {
            SaveData saveData = new SaveData();
            saveData.isfinished = true;
            SaveManager.Instance.SaveGame(saveData);
            Debug.Log("[Boss] Defeated — isFinished saved.");
        }

        gameObject.SetActive(false);
        GameManager.Instance.PauseGame();
        GameStateManager.Instance.SetState(GameState.LevelEnd);

    }
}
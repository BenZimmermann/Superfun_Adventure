using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cuphead-inspired Boss Controller for a 2D Jump & Run Shooter in Unity 6.
/// Attach this to the Boss GameObject (centered in the room).
/// </summary>
public class BossController : MonoBehaviour, IDamageable
{
    // ─────────────────────────────────────────────
    //  Inspector References
    // ─────────────────────────────────────────────

    [Header("=== Health & Phases ===")]
    [SerializeField] private int maxHealth = 400;
    [SerializeField] private float[] phaseThresholds = { 0.75f, 0.50f, 0.25f }; // 75 %, 50 %, 25 %

    [Header("=== Boss Arms (removed per phase) ===")]
    [Tooltip("4 arm GameObjects — removed one per phase transition (index 0 = first removed).")]
    [SerializeField] private List<GameObject> bossArms = new List<GameObject>();

    [Header("=== Projectile Prefabs ===")]
    [SerializeField] private GameObject codeRainProjectilePrefab;
    [SerializeField] private GameObject fireballProjectilePrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("=== Spawn Points ===")]
    [SerializeField] private Transform[] codeRainSpawnPoints;   // top of screen
    [SerializeField] private Transform[] fireballSpawnPoints;   // sides of screen
    [SerializeField] private Transform[] enemySpawnPoints;      // fixed positions on ground
    [SerializeField] private Transform[] spikeSpawnPoints;      // ground positions

    [Header("=== Laser ===")]
    [SerializeField] private Transform laserOrigin;             // center of boss
    [SerializeField] private float laserDuration = 2f;

    [Header("=== Boss Movement (foreground / background dodge) ===")]
    [SerializeField] private float dodgeDistance = 3f;          // how far boss moves on Z-axis (sprite sort)
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeInterval = 6f;          // seconds between dodges
    [SerializeField] private SpriteRenderer bossSpriteRenderer;

    [Header("=== Room Environment ===")]
    [SerializeField] private BossEnvironment bossEnvironment;   // separate component

    [Header("=== Enemy Spawning ===")]
    [SerializeField] private EnemySpawnController enemySpawnController;

    [Header("=== UI ===")]
    [SerializeField] private BossHealthBar bossHealthBar;

    [Header("=== Attack Timings ===")]
    [SerializeField] private float attackInterval = 3f;

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    private int currentHealth;
    private int currentPhase = 0; // 0 = phase 1, goes up to 3
    private bool isTransitioning = false;
    private bool isDodging = false;
    private bool isAttacking = false;

    private Coroutine attackLoopCoroutine;
    private Coroutine dodgeLoopCoroutine;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Start()
    {
        currentHealth = maxHealth;
        bossHealthBar?.Initialize(maxHealth);
        bossEnvironment?.SetPhase(1);

        attackLoopCoroutine = StartCoroutine(AttackLoop());
        dodgeLoopCoroutine = StartCoroutine(DodgeLoop());
    }

    // ─────────────────────────────────────────────
    //  Damage & Phase Transitions
    // ─────────────────────────────────────────────

    // ─────────────────────────────────────────────
    //  IDamageable — Damage & Phase Transitions
    // ─────────────────────────────────────────────

    /// <summary>
    /// Called by player bullets hitting the boss.
    /// Only DamageSource.Player should actually damage the boss;
    /// other sources are ignored to prevent self-damage edge cases.
    /// </summary>
    public void TakeDamage(int amount, DamageSource source)
    {
        // Boss can only be hurt by the player
        if (source != DamageSource.Player) return;
        if (IsInvulnerable()) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        bossHealthBar?.UpdateHealth(currentHealth);

        CheckPhaseTransition();

        if (currentHealth <= 0) Die();
    }

    private void CheckPhaseTransition()
    {
        float ratio = (float)currentHealth / maxHealth;

        // Phase thresholds: index 0 triggers phase 2, 1 → phase 3, 2 → phase 4
        if (currentPhase < phaseThresholds.Length && ratio <= phaseThresholds[currentPhase])
        {
            StartCoroutine(TransitionToPhase(currentPhase + 2)); // +2 because phase is 1-based
        }
    }

    private IEnumerator TransitionToPhase(int newPhase)
    {
        isTransitioning = true;

        // Stop current attack loop
        if (attackLoopCoroutine != null) StopCoroutine(attackLoopCoroutine);

        // Play transition animation / flash
        yield return StartCoroutine(PlayTransitionEffect());

        // Remove arm (arms list index matches phase - 1, i.e. phase 2 removes arm[0])
        int armIndex = newPhase - 2;
        RemoveArm(armIndex);

        // Change environment
        currentPhase = newPhase - 1; // internal 0-based index
        bossEnvironment?.SetPhase(newPhase);

        Debug.Log($"[Boss] Transitioned to Phase {newPhase}");

        isTransitioning = false;

        // Restart attack loop with new phase
        attackLoopCoroutine = StartCoroutine(AttackLoop());
    }

    // ─────────────────────────────────────────────
    //  Arm Management
    // ─────────────────────────────────────────────

    /// <summary>Removes the arm at the given index (0–3).</summary>
    private void RemoveArm(int index)
    {
        if (index < 0 || index >= bossArms.Count) return;
        if (bossArms[index] == null) return;

        // Play a little pop / destroy effect here if desired
        Destroy(bossArms[index]);
        bossArms[index] = null;
        Debug.Log($"[Boss] Arm {index} destroyed.");
    }

    // ─────────────────────────────────────────────
    //  Attack Loop (phase-aware)
    // ─────────────────────────────────────────────

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            if (isTransitioning || isDodging) continue;

            yield return StartCoroutine(ExecutePhaseAttack());
        }
    }

    private IEnumerator ExecutePhaseAttack()
    {
        isAttacking = true;

        switch (currentPhase)
        {
            case 0: yield return StartCoroutine(Phase1Attack()); break;
            case 1: yield return StartCoroutine(Phase2Attack()); break;
            case 2: yield return StartCoroutine(Phase3Attack()); break;
            case 3: yield return StartCoroutine(Phase4Attack()); break;
        }

        isAttacking = false;
    }

    // ─────────────────────────────────────────────
    //  Phase Attacks
    // ─────────────────────────────────────────────

    /// Phase 1 — Code Rain + Fireballs
    private IEnumerator Phase1Attack()
    {
        int choice = Random.Range(0, 2);
        if (choice == 0)
            yield return StartCoroutine(AttackCodeRain());
        else
            yield return StartCoroutine(AttackFireballs());
    }

    /// Phase 2 — Code Rain + Laser
    private IEnumerator Phase2Attack()
    {
        int choice = Random.Range(0, 2);
        if (choice == 0)
            yield return StartCoroutine(AttackCodeRain());
        else
            yield return StartCoroutine(AttackLaser());
    }

    /// Phase 3 — Spikes + Smaller Room Walls
    private IEnumerator Phase3Attack()
    {
        int choice = Random.Range(0, 2);
        if (choice == 0)
            yield return StartCoroutine(AttackSpikes());
        else
        {
            bossEnvironment?.ShrinkWalls();
            yield return new WaitForSeconds(1f);
        }
    }

    /// Phase 4 — Spikes + Enemy Spawns
    private IEnumerator Phase4Attack()
    {
        int choice = Random.Range(0, 2);
        if (choice == 0)
            yield return StartCoroutine(AttackSpikes());
        else
            yield return StartCoroutine(AttackSpawnEnemies());
    }

    // ─────────────────────────────────────────────
    //  Individual Attack Modules
    // ─────────────────────────────────────────────

    /// Spawns projectiles from the top (code rain effect)
    private IEnumerator AttackCodeRain()
    {
        Debug.Log("[Boss] Attack: Code Rain");
        int bursts = 5;
        for (int i = 0; i < bursts; i++)
        {
            foreach (var spawnPoint in codeRainSpawnPoints)
            {
                if (codeRainProjectilePrefab != null)
                    Instantiate(codeRainProjectilePrefab, spawnPoint.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// Shoots fireballs from the sides
    private IEnumerator AttackFireballs()
    {
        Debug.Log("[Boss] Attack: Fireballs");
        foreach (var spawnPoint in fireballSpawnPoints)
        {
            if (fireballProjectilePrefab != null)
                Instantiate(fireballProjectilePrefab, spawnPoint.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(1f);
    }

    /// Fires a laser from the boss center
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
        {
            yield return new WaitForSeconds(laserDuration);
        }
    }

    /// Spawns spikes on the ground
    private IEnumerator AttackSpikes()
    {
        Debug.Log("[Boss] Attack: Spikes");
        List<GameObject> spawnedSpikes = new List<GameObject>();
        foreach (var spawnPoint in spikeSpawnPoints)
        {
            if (spikePrefab != null)
                spawnedSpikes.Add(Instantiate(spikePrefab, spawnPoint.position, Quaternion.identity));
        }
        yield return new WaitForSeconds(3f);
        foreach (var spike in spawnedSpikes) if (spike != null) Destroy(spike);
    }

    /// Spawns enemies at fixed positions — max 2 at a time enforced by EnemySpawnController
    private IEnumerator AttackSpawnEnemies()
    {
        Debug.Log("[Boss] Attack: Spawn Enemies");
        foreach (var spawnPoint in enemySpawnPoints)
        {
            if (enemyPrefab == null) continue;

            if (enemySpawnController != null)
                enemySpawnController.TrySpawnEnemy(enemyPrefab, spawnPoint.position);
            else
                Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity); // fallback
        }
        yield return new WaitForSeconds(2f);
    }

    // ─────────────────────────────────────────────
    //  Boss Dodge — foreground / background movement
    // ─────────────────────────────────────────────

    private IEnumerator DodgeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(dodgeInterval);
            if (!isTransitioning)
                yield return StartCoroutine(DodgeForegroundBackground());
        }
    }

    /// Moves boss "into background" (scale/alpha change to simulate depth),
    /// making him temporarily invulnerable.
    private IEnumerator DodgeForegroundBackground()
    {
        isDodging = true;
        Debug.Log("[Boss] Dodge: stepping into background.");

        // Simulate going into background — scale down & reduce alpha
        yield return StartCoroutine(ScaleBoss(0.75f, dodgeDuration));
        SetBossInvulnerable(true);

        yield return new WaitForSeconds(dodgeDuration + 0.5f);

        // Return to foreground
        SetBossInvulnerable(false);
        yield return StartCoroutine(ScaleBoss(1f, dodgeDuration));
        Debug.Log("[Boss] Dodge: back in foreground.");

        isDodging = false;
    }

    private IEnumerator ScaleBoss(float targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(
            Mathf.Sign(startScale.x) * targetScale,
            targetScale,
            targetScale);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endScale;

        // Also adjust sprite alpha for the "depth" effect
        if (bossSpriteRenderer != null)
        {
            Color c = bossSpriteRenderer.color;
            c.a = (targetScale < 1f) ? 0.4f : 1f;
            bossSpriteRenderer.color = c;
        }
    }

    // ─────────────────────────────────────────────
    //  Invulnerability
    // ─────────────────────────────────────────────

    private bool isInvulnerable = false;

    private void SetBossInvulnerable(bool value)
    {
        isInvulnerable = value;
    }

    public bool IsInvulnerable() => isInvulnerable || isTransitioning;

    // ─────────────────────────────────────────────
    //  Transition Effect
    // ─────────────────────────────────────────────

    private IEnumerator PlayTransitionEffect()
    {
        // TODO: trigger animation, screen flash, sound — hook in here
        Debug.Log("[Boss] Playing phase transition effect...");
        yield return new WaitForSeconds(1.5f);
    }

    // ─────────────────────────────────────────────
    //  Death
    // ─────────────────────────────────────────────

    private void Die()
    {
        Debug.Log("[Boss] Boss defeated!");
        StopAllCoroutines();
        bossEnvironment?.OnBossDefeated();
        // TODO: play death animation, trigger level clear, etc.
        gameObject.SetActive(false);
    }
}
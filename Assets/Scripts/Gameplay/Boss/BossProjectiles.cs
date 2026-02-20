using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  DamageHelper — shared static utility, keeps projectiles DRY
// ═══════════════════════════════════════════════════════════════
internal static class DamageHelper
{
    /// <summary>Deals damage to any IDamageable on the collider's GameObject.</summary>
    public static bool TryDamage(Collider2D other, int amount, DamageSource source)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null) return false;
        target.TakeDamage(amount, source);
        return true;
    }

    /// <summary>Overload — rounds float to int before dealing damage.</summary>
    public static bool TryDamage(Collider2D other, float amount, DamageSource source)
        => TryDamage(other, Mathf.RoundToInt(amount), source);

    // ─────────────────────────────────────────────
    //  90-degree direction snap toward player
    // ─────────────────────────────────────────────

    /// <summary>
    /// Snaps a direction to the nearest cardinal axis (right / left / up / down).
    /// Whichever component has the larger absolute value wins its axis.
    /// </summary>
    public static Vector2 SnapTo90(Vector2 rawDirection)
    {
        if (Mathf.Abs(rawDirection.x) >= Mathf.Abs(rawDirection.y))
            return rawDirection.x >= 0f ? Vector2.right : Vector2.left;
        else
            return rawDirection.y >= 0f ? Vector2.up : Vector2.down;
    }

    /// <summary>
    /// Returns the 90°-snapped direction from <paramref name="from"/> toward the Player tag.
    /// Falls back to Vector2.down if no player is found.
    /// </summary>
    public static Vector2 DirectionToPlayerSnapped(Vector2 from)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Vector2.down;
        Vector2 raw = (Vector2)player.transform.position - from;
        return SnapTo90(raw);
    }
}

// ═══════════════════════════════════════════════════════════════
//  CodeRainProjectile
//  Spawned above the screen. Snaps direction toward player on Start.
// ═══════════════════════════════════════════════════════════════
public class CodeRainProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;

    private Vector2 direction;

    private void Start()
    {
        direction = DamageHelper.DirectionToPlayerSnapped(transform.position);
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DamageHelper.TryDamage(other, damage, DamageSource.Boss);
        Destroy(gameObject);
    }
}

// ═══════════════════════════════════════════════════════════════
//  FireballProjectile
//  Spawned from the sides. Direction snapped to 90° toward player.
// ═══════════════════════════════════════════════════════════════
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 6f;

    private Vector2 direction;

    private void Start()
    {
        direction = DamageHelper.DirectionToPlayerSnapped(transform.position);

        // Flip sprite when moving right
        if (direction.x > 0f)
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DamageHelper.TryDamage(other, damage, DamageSource.Boss);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}

// ═══════════════════════════════════════════════════════════════
//  LaserBeam
//  LineRenderer + Physics2D.Raycast — tracks player, no Collider needed.
//
//  Prefab setup:
//    • Add LineRenderer (positionCount=2, useWorldSpace=true)
//    • No physical trigger collider needed — damage via Raycast hit
// ═══════════════════════════════════════════════════════════════
public class LaserBeam : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damagePerSecond = 8;
    [SerializeField] private float maxLength = 20f;

    [Header("Line Renderer")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float beamWidth = 0.15f;
    [SerializeField] private Color beamColorStart = Color.red;
    [SerializeField] private Color beamColorEnd = new Color(1f, 0.3f, 0f, 0.6f);

    [Header("Warm-up telegraph")]
    [SerializeField] private float warmupDuration = 0.5f;

    [Header("Tracking")]
    [SerializeField] private float trackingSpeed = 90f;   // degrees / second
    [SerializeField] private bool instantAim = false;

    private Transform playerTransform;
    private bool isWarmedUp = false;
    private float currentAngle;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = beamWidth;
            lineRenderer.endWidth = beamWidth * 0.5f;
            lineRenderer.startColor = beamColorStart;
            lineRenderer.endColor = beamColorEnd;
            lineRenderer.useWorldSpace = true;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        StartCoroutine(Warmup());
    }

    private IEnumerator Warmup()
    {
        SetAlpha(0.3f);
        yield return new WaitForSeconds(warmupDuration);
        SetAlpha(1f);
        isWarmedUp = true;
    }

    private void Update()
    {
        if (playerTransform == null || lineRenderer == null) return;

        // ── Rotate toward player ──────────────────
        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        currentAngle = instantAim
            ? targetAngle
            : Mathf.MoveTowardsAngle(currentAngle, targetAngle, trackingSpeed * Time.deltaTime);

        // ── Cast ray & draw beam ──────────────────
        Vector2 beamDir = new Vector2(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        Vector3 origin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, beamDir, maxLength,
                                  LayerMask.GetMask("Default", "Ground", "Wall"));

        Vector3 endpoint = (hit.collider != null)
            ? (Vector3)hit.point
            : origin + (Vector3)(beamDir * maxLength);

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endpoint);

        // ── Damage player if warmed up ────────────
        if (isWarmedUp && hit.collider != null && hit.collider.CompareTag("Player"))
            DamageHelper.TryDamage(hit.collider, damagePerSecond * Time.deltaTime, DamageSource.Boss);
    }

    private void SetAlpha(float alpha)
    {
        if (lineRenderer == null) return;
        Color s = lineRenderer.startColor; s.a = alpha; lineRenderer.startColor = s;
        Color e = lineRenderer.endColor; e.a = alpha; lineRenderer.endColor = e;
    }
}

// ═══════════════════════════════════════════════════════════════
//  Spike — rises from ground, damages player on contact
// ═══════════════════════════════════════════════════════════════
public class Spike : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float riseSpeed = 3f;
    [SerializeField] private float riseHeight = 1.5f;

    private Vector3 startPos;
    private bool rising = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!rising) return;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        if (transform.position.y >= startPos.y + riseHeight)
            rising = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DamageHelper.TryDamage(other, damage, DamageSource.Boss);
    }
}

// ═══════════════════════════════════════════════════════════════
//  EnemySpawnController — enforces max 2 live enemies at a time
//  Attach to the Boss or a dedicated Manager GameObject.
// ═══════════════════════════════════════════════════════════════
public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private int maxEnemies = 2;

    private readonly List<GameObject> _liveEnemies = new List<GameObject>();

    /// <summary>
    /// Spawns a prefab at position if under the cap. Returns null if cap reached.
    /// Destroyed enemies are pruned automatically.
    /// </summary>
    public GameObject TrySpawnEnemy(GameObject prefab, Vector3 position)
    {
        _liveEnemies.RemoveAll(e => e == null);

        if (_liveEnemies.Count >= maxEnemies)
        {
            Debug.Log($"[EnemySpawn] Cap reached ({maxEnemies}). Skipping spawn.");
            return null;
        }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        _liveEnemies.Add(enemy);
        Debug.Log($"[EnemySpawn] Spawned ({_liveEnemies.Count}/{maxEnemies}).");
        return enemy;
    }

    /// <summary>Current live enemy count (null entries pruned automatically).</summary>
    public int LiveCount
    {
        get { _liveEnemies.RemoveAll(e => e == null); return _liveEnemies.Count; }
    }
}
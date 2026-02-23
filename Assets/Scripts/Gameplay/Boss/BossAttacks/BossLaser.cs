using System.Collections;
using UnityEngine;

// LaserBeam.cs
// ─────────────────────────────────────────────────────────────
//  Prefab setup:
//    • LineRenderer    positionCount=2, useWorldSpace=ON
//                      Assign a bright Additive/Glow material
//    • LaserBeam       (this script)
//    • NO Rigidbody, NO Collider — damage fully via Physics2D.Raycast
//
//  FIX: Raycast now hits the Player layer directly.
//       Make sure your Player GameObject is on a layer called "Player"
//       (Edit → Project Settings → Tags and Layers).
// ─────────────────────────────────────────────────────────────
public class LaserBeam : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damagePerSecond = 8;
    [Tooltip("Maximum world-unit length of the beam.")]
    [SerializeField] private float maxLength = 20f;

    [Header("Line Renderer")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float beamWidth = 0.15f;
    [SerializeField] private Color beamColorStart = Color.red;
    [SerializeField] private Color beamColorEnd = new Color(1f, 0.3f, 0f, 0.6f);

    [Header("Warm-up telegraph")]
    [Tooltip("Seconds the beam shows dimly before dealing damage.")]
    [SerializeField] private float warmupDuration = 0.5f;

    [Header("Tracking")]
    [Tooltip("Degrees per second the beam rotates toward the player.")]
    [SerializeField] private float trackingSpeed = 90f;
    [Tooltip("Snap to player instantly — no rotation lag.")]
    [SerializeField] private bool instantAim = false;

    // ─── Internal ────────────────────────────────
    private Transform playerTransform;
    private bool isWarmedUp = false;
    private float currentAngle;

    /// <summary>
    /// LayerMask used for the raycast.
    /// Includes "Player", "Default", "Ground", and "Wall" so the beam
    /// stops at whichever it hits first (player OR geometry).
    /// </summary>
    private int raycastMask;

    // ─────────────────────────────────────────────

    private void Awake()
    {
        // Build mask once — includes Player so we actually detect hits
        raycastMask = LayerMask.GetMask("Player", "Default", "Ground", "Wall");

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
        SetAlpha(0.3f);                          // dim = warning / telegraph
        yield return new WaitForSeconds(warmupDuration);
        SetAlpha(1f);
        isWarmedUp = true;
    }

    private void Update()
    {
        if (playerTransform == null || lineRenderer == null) return;

        // ── Track player ─────────────────────────────
        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        currentAngle = instantAim
            ? targetAngle
            : Mathf.MoveTowardsAngle(currentAngle, targetAngle, trackingSpeed * Time.deltaTime);

        // ── Cast ray ─────────────────────────────────
        Vector2 beamDir = new Vector2(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        Vector3 origin = transform.position;

        // Offset the origin slightly so it doesn't hit the boss's own collider
        RaycastHit2D hit = Physics2D.Raycast(
            origin + (Vector3)(beamDir * 0.3f),
            beamDir,
            maxLength,
            raycastMask);

        Vector3 endpoint = (hit.collider != null)
            ? (Vector3)hit.point
            : origin + (Vector3)(beamDir * maxLength);

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endpoint);

        // ── Damage ───────────────────────────────────
        if (isWarmedUp && hit.collider != null && hit.collider.CompareTag("Player"))
        {
            DamageHelper.TryDamage(hit.collider, damagePerSecond * Time.deltaTime, DamageSource.Boss);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (lineRenderer == null) return;
        Color s = lineRenderer.startColor; s.a = alpha; lineRenderer.startColor = s;
        Color e = lineRenderer.endColor; e.a = alpha; lineRenderer.endColor = e;
    }
}
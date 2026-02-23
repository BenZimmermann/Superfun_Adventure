using UnityEngine;

// CodeRainProjectile.cs
// ─────────────────────────────────────────────────────────────
//  Prefab setup:
//    • SpriteRenderer       (code/glitch sprite, facing DOWN)
//    • Rigidbody2D          Body Type: Kinematic, Gravity Scale: 0
//    • BoxCollider2D        Is Trigger: ON
//    • CodeRainProjectile   (this script)
//
//  Always falls straight down (no player-tracking).
//  Visual variety via random speed, scale, wobble, gradient + flicker.
// ─────────────────────────────────────────────────────────────
public class CodeRainProjectile : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 6f;
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float lifetime = 5f;

    [Header("Wobble (horizontal drift while falling)")]
    [SerializeField] private bool wobbleEnabled = true;
    [SerializeField] private float wobbleAmplitude = 0.3f;
    [SerializeField] private float wobbleFrequency = 3f;

    [Header("Visual")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.4f;
    [Tooltip("Color cycles through this gradient over the projectile's lifetime.")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private bool flickerEnabled = true;
    [SerializeField] private float flickerSpeed = 8f;

    // ─────────────────────────────────────────────
    private float speed;
    private float wobbleOffset;
    private float elapsed;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        speed = Random.Range(minSpeed, maxSpeed);

        float scale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(scale, scale, scale);

        wobbleOffset = Random.Range(0f, Mathf.PI * 2f);

        // Start transparent, fade in
        if (sr != null) { Color c = sr.color; c.a = 0f; sr.color = c; }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        // ── Movement — always straight down + optional wobble ──
        float wobbleDelta = wobbleEnabled
            ? Mathf.Sin((elapsed * wobbleFrequency) + wobbleOffset) * wobbleAmplitude * Time.deltaTime
            : 0f;

        transform.position += new Vector3(wobbleDelta, -speed * Time.deltaTime, 0f);

        // ── Visuals ────────────────────────────────────────────
        if (sr != null)
        {
            float fadeIn = Mathf.Clamp01(elapsed / 0.1f);
            float t = Mathf.Clamp01(elapsed / lifetime);

            Color col = (colorGradient != null && colorGradient.colorKeys.Length > 0)
                ? colorGradient.Evaluate(t)
                : sr.color;

            if (flickerEnabled)
                col *= 0.7f + 0.3f * Mathf.Sin(elapsed * flickerSpeed + wobbleOffset);

            col.a = fadeIn;
            sr.color = col;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DamageHelper.TryDamage(other, damage, DamageSource.Boss);
        Destroy(gameObject);
    }
}
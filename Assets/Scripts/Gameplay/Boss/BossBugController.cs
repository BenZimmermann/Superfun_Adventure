using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// BossBugEffectController.cs
// ─────────────────────────────────────────────────────────────
//  Attach to the Boss GameObject alongside BossController.
//  BossController calls TriggerRandomBugEffect() between attacks.
//
//  Adapts effects from BugManager as chaotic boss abilities:
//    • Screen shake / flash
//    • Sprite corruption (boss body parts)
//    • Music pitch distortion
//    • Gravity reversal
//    • Time slowdown
//    • Player control inversion
//    • Fake error messages
//    • UI hide
//    • Object flash spawning
//    • Random forces on nearby rigidbodies
//
//  Per phase, more effects become available (pool expands).
//  You can toggle individual effects on/off in the Inspector.
// ─────────────────────────────────────────────────────────────
public class BossBugEffectController : MonoBehaviour
{
    // ═════════════════════════════════════════════
    //  INSPECTOR
    // ═════════════════════════════════════════════

    [Header("=== Trigger Settings ===")]
    [Tooltip("How often (seconds) a random bug effect fires between attacks.")]
    [SerializeField] private float effectInterval = 8f;
    [Tooltip("effectInterval is multiplied by this per phase (e.g. 0.8 = faster each phase).")]
    [SerializeField] private float intervalPhaseMultiplier = 0.8f;
    [Tooltip("Minimum seconds between effects regardless of phase scaling.")]
    [SerializeField] private float minEffectInterval = 3f;
    [SerializeField] private bool showDebugLogs = true;

    // ─── Screen Shake ─────────────────────────────
    [Header("=== Screen Shake ===")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.3f;

    // ─── Screen Flash ─────────────────────────────
    [Header("=== Screen Flash ===")]
    [SerializeField] private bool enableScreenFlash = true;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] private float flashDuration = 0.3f;

    // ─── Object Flash ─────────────────────────────
    [Header("=== Object Flash ===")]
    [SerializeField] private bool enableObjectFlash = false;
    [SerializeField] private GameObject objectToFlash;
    [SerializeField] private Transform flashSpawnPoint;
    [SerializeField] private float objectFlashDuration = 0.8f;
    [SerializeField] private bool fadeOutObject = true;
    [SerializeField] private bool scaleUpDuringFlash = true;
    [SerializeField] private float flashScale = 1.8f;

    // ─── Sprite Corruption ────────────────────────
    [Header("=== Sprite Corruption ===")]
    [Tooltip("Boss sprites (arms, body etc.) that get colour-corrupted briefly.")]
    [SerializeField] private bool enableCorruptSprites = true;
    [SerializeField] private List<SpriteRenderer> spritesToCorrupt = new List<SpriteRenderer>();
    [SerializeField] private float corruptionDuration = 0.8f;

    // ─── Audio Distortion ─────────────────────────
    [Header("=== Audio Glitch ===")]
    [SerializeField] private bool enableGlitchSound = false;
    [SerializeField] private AudioClip glitchSound;

    [SerializeField] private bool enableMusicDistortion = true;
    [SerializeField] private float musicPitchChange = 0.5f;
    [SerializeField] private float musicDistortDuration = 2f;

    // ─── Physics Chaos ────────────────────────────
    [Header("=== Physics Chaos ===")]
    [SerializeField] private bool enableRandomForces = true;
    [SerializeField] private float forceRadius = 5f;
    [SerializeField] private float forceStrength = 12f;

    [SerializeField] private bool enableGravityReverse = false;   // off by default – very disruptive
    [SerializeField] private float gravityReverseDuration = 1.5f;

    // ─── Player Effects ───────────────────────────
    [Header("=== Player Effects ===")]
    [SerializeField] private bool enableInvertControls = true;
    [SerializeField] private float controlInvertDuration = 2.5f;

    // ─── Time Manipulation ────────────────────────
    [Header("=== Time Manipulation ===")]
    [SerializeField] private bool enableTimeManipulation = true;
    [Range(0.1f, 1f)]
    [SerializeField] private float timeScaleSlow = 0.4f;
    [SerializeField] private float timeManipulationDuration = 1.5f;

    // ─── UI Glitches ──────────────────────────────
    [Header("=== UI Glitches ===")]
    [SerializeField] private bool enableHideUI = true;
    [SerializeField] private float uiHideDuration = 1.5f;

    [SerializeField] private bool enableFakeError = true;
    [SerializeField] private string errorMessage = "CRITICAL ERROR: Boss.exe has stopped responding";
    [SerializeField] private float errorDisplayDuration = 2.5f;

    // ═════════════════════════════════════════════
    //  PRIVATE STATE
    // ═════════════════════════════════════════════

    private int currentPhase = 0;
    private float currentInterval;
    private AudioSource audioSource;
    private Vector2 originalGravity;
    private Coroutine effectLoopCoroutine;

    // ═════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═════════════════════════════════════════════

    private void Awake()
    {
        if (enableGlitchSound && glitchSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        currentInterval = effectInterval;
        effectLoopCoroutine = StartCoroutine(EffectLoop());
    }

    // ═════════════════════════════════════════════
    //  PUBLIC API — called by BossController
    // ═════════════════════════════════════════════

    /// <summary>
    /// Called by BossController on every phase transition.
    /// Scales the effect frequency and expands the available effect pool.
    /// </summary>
    public void OnPhaseChanged(int newPhase)
    {
        currentPhase = newPhase - 1; // convert to 0-based
        currentInterval = Mathf.Max(
            minEffectInterval,
            effectInterval * Mathf.Pow(intervalPhaseMultiplier, currentPhase));

        if (showDebugLogs)
            Debug.Log($"[BugEffect] Phase {newPhase} — effect interval={currentInterval:F2}s");
    }

    /// <summary>
    /// Fires a single random bug effect immediately.
    /// Can also be called from BossController at will.
    /// </summary>
    public void TriggerRandomBugEffect()
    {
        var pool = BuildEffectPool();
        if (pool.Count == 0) return;

        int idx = Random.Range(0, pool.Count);
        StartCoroutine(pool[idx]());

        if (showDebugLogs)
            Debug.Log($"[BugEffect] Triggered effect #{idx} from pool of {pool.Count}");
    }

    // ═════════════════════════════════════════════
    //  EFFECT LOOP
    // ═════════════════════════════════════════════

    private IEnumerator EffectLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentInterval);
            TriggerRandomBugEffect();
        }
    }

    // ═════════════════════════════════════════════
    //  EFFECT POOL — cumulative per phase
    // ═════════════════════════════════════════════

    /// <summary>
    /// Builds the available effect pool for the current phase.
    /// Phase 1: visual + audio only (less disruptive)
    /// Phase 2: + physics chaos
    /// Phase 3: + time manipulation, player effects
    /// Phase 4: + all remaining effects
    /// </summary>
    private List<System.Func<IEnumerator>> BuildEffectPool()
    {
        var pool = new List<System.Func<IEnumerator>>();

        // ── Phase 1+ (always available) ──────────
        if (enableScreenShake) pool.Add(ScreenShake);
        if (enableScreenFlash) pool.Add(ScreenFlash);
        if (enableCorruptSprites && spritesToCorrupt.Count > 0)
            pool.Add(CorruptSprites);
        if (enableMusicDistortion) pool.Add(DistortMusic);
        if (enableObjectFlash && objectToFlash != null)
            pool.Add(ObjectFlash);

        // ── Phase 2+ ─────────────────────────────
        if (currentPhase >= 1)
        {
            if (enableRandomForces) pool.Add(ApplyRandomForcesEffect);
            if (enableFakeError) pool.Add(ShowFakeError);
        }

        // ── Phase 3+ ─────────────────────────────
        if (currentPhase >= 2)
        {
            if (enableTimeManipulation) pool.Add(ManipulateTime);
            if (enableInvertControls) pool.Add(InvertPlayerControls);
            if (enableHideUI) pool.Add(HideUI);
        }

        // ── Phase 4 ──────────────────────────────
        if (currentPhase >= 3)
        {
            if (enableGravityReverse) pool.Add(ReverseGravity);
        }

        return pool;
    }

    // ═════════════════════════════════════════════
    //  VISUAL EFFECTS
    // ═════════════════════════════════════════════

    private IEnumerator ScreenShake()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 origin = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            cam.transform.position = origin + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = origin;
        if (showDebugLogs) Debug.Log("[BugEffect] Screen shake done.");
    }

    private IEnumerator ScreenFlash()
    {
        // Creates a full-screen overlay sprite and fades it out
        GameObject flashObj = new GameObject("BossScreenFlash");
        SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        sr.color = flashColor;
        sr.sortingOrder = 9999;

        Camera cam = Camera.main;
        if (cam != null)
        {
            flashObj.transform.position = cam.transform.position + Vector3.forward * 5f;
            flashObj.transform.localScale = new Vector3(100f, 100f, 1f);
        }

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            sr.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        Destroy(flashObj);
        if (showDebugLogs) Debug.Log("[BugEffect] Screen flash done.");
    }

    private IEnumerator ObjectFlash()
    {
        Vector3 spawnPos = flashSpawnPoint != null ? flashSpawnPoint.position : transform.position;
        Quaternion spawnRot = flashSpawnPoint != null ? flashSpawnPoint.rotation : Quaternion.identity;
        GameObject flashedObj = Instantiate(objectToFlash, spawnPos, spawnRot);

        Vector3 originalScale = flashedObj.transform.localScale;
        Vector3 targetScale = scaleUpDuringFlash ? originalScale * flashScale : originalScale;

        // Optional scale-up phase (30 % of duration)
        if (scaleUpDuringFlash)
        {
            float scaleTime = objectFlashDuration * 0.3f;
            for (float t = 0f; t < scaleTime; t += Time.deltaTime)
            {
                flashedObj.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / scaleTime);
                yield return null;
            }
        }

        // Fade out via sprite renderers
        if (fadeOutObject)
        {
            SpriteRenderer[] renderers = flashedObj.GetComponentsInChildren<SpriteRenderer>();
            var origColors = new Dictionary<SpriteRenderer, Color>();
            foreach (var r in renderers) origColors[r] = r.color;

            float fadeStart = objectFlashDuration * 0.5f;
            yield return new WaitForSeconds(fadeStart);

            float fadeDuration = objectFlashDuration - fadeStart;
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                foreach (var kvp in origColors)
                    if (kvp.Key != null)
                        kvp.Key.color = new Color(kvp.Value.r, kvp.Value.g, kvp.Value.b, alpha);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(objectFlashDuration);
        }

        Destroy(flashedObj);
        if (showDebugLogs) Debug.Log("[BugEffect] Object flash done.");
    }

    private IEnumerator CorruptSprites()
    {
        // Save originals, apply glitch colours + flipX
        var originals = new Dictionary<SpriteRenderer, (Sprite sprite, Color color, bool flipX)>();
        foreach (var sr in spritesToCorrupt)
        {
            if (sr == null) continue;
            originals[sr] = (sr.sprite, sr.color, sr.flipX);
            sr.color = new Color(Random.value, Random.value, Random.value, sr.color.a);
            sr.flipX = !sr.flipX;
        }

        yield return new WaitForSeconds(corruptionDuration);

        // Restore
        foreach (var kvp in originals)
        {
            if (kvp.Key == null) continue;
            kvp.Key.sprite = kvp.Value.sprite;
            kvp.Key.color = kvp.Value.color;
            kvp.Key.flipX = kvp.Value.flipX;
        }

        if (showDebugLogs) Debug.Log("[BugEffect] Sprite corruption restored.");
    }

    // ═════════════════════════════════════════════
    //  AUDIO EFFECTS
    // ═════════════════════════════════════════════

    private IEnumerator DistortMusic()
    {
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        var sources = new List<AudioSource>();
        var originalPitches = new List<float>();

        foreach (var src in allAudio)
        {
            if (src.isPlaying && src != audioSource)
            {
                sources.Add(src);
                originalPitches.Add(src.pitch);
                src.pitch = musicPitchChange;
            }
        }

        yield return new WaitForSeconds(musicDistortDuration);

        for (int i = 0; i < sources.Count; i++)
            if (sources[i] != null) sources[i].pitch = originalPitches[i];

        if (showDebugLogs) Debug.Log("[BugEffect] Music pitch restored.");
    }

    // ═════════════════════════════════════════════
    //  PHYSICS EFFECTS
    // ═════════════════════════════════════════════

    private IEnumerator ApplyRandomForcesEffect()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, forceRadius);
        foreach (var col in nearby)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;
            rb.AddForce(Random.insideUnitCircle.normalized * forceStrength, ForceMode2D.Impulse);
        }

        if (showDebugLogs) Debug.Log("[BugEffect] Random forces applied.");
        yield return null;
    }

    private IEnumerator ReverseGravity()
    {
        originalGravity = Physics2D.gravity;
        Physics2D.gravity = -originalGravity;
        if (showDebugLogs) Debug.Log("[BugEffect] Gravity reversed!");

        yield return new WaitForSeconds(gravityReverseDuration);

        Physics2D.gravity = originalGravity;
        if (showDebugLogs) Debug.Log("[BugEffect] Gravity restored.");
    }

    // ═════════════════════════════════════════════
    //  PLAYER EFFECTS
    // ═════════════════════════════════════════════

    private IEnumerator InvertPlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // Hook: call InvertControls(true) on your PlayerMovement here
        // PlayerMovement pm = player.GetComponent<PlayerMovement>();
        // if (pm != null) pm.InvertControls(true);

        if (showDebugLogs) Debug.Log("[BugEffect] Controls inverted! (hook InvertControls in PlayerMovement)");

        yield return new WaitForSeconds(controlInvertDuration);

        // if (pm != null) pm.InvertControls(false);
        if (showDebugLogs) Debug.Log("[BugEffect] Controls restored.");
    }

    // ═════════════════════════════════════════════
    //  TIME MANIPULATION
    // ═════════════════════════════════════════════

    private IEnumerator ManipulateTime()
    {
        float original = Time.timeScale;
        Time.timeScale = timeScaleSlow;
        if (showDebugLogs) Debug.Log($"[BugEffect] Time slowed to {timeScaleSlow}x");

        // WaitForSecondsRealtime so the wait isn't affected by the slow timescale
        yield return new WaitForSecondsRealtime(timeManipulationDuration);

        Time.timeScale = original;
        if (showDebugLogs) Debug.Log("[BugEffect] Time restored.");
    }

    // ═════════════════════════════════════════════
    //  UI EFFECTS
    // ═════════════════════════════════════════════

    private IEnumerator HideUI()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases) c.enabled = false;

        yield return new WaitForSeconds(uiHideDuration);

        foreach (var c in canvases) if (c != null) c.enabled = true;
        if (showDebugLogs) Debug.Log("[BugEffect] UI restored.");
    }

    private IEnumerator ShowFakeError()
    {
        // Build a full-screen error overlay at runtime
        GameObject root = new GameObject("BossFakeError");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        root.AddComponent<UnityEngine.UI.CanvasScaler>();
        root.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Dark background
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(root.transform, false);
        var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.85f);
        FullStretch(bg.GetComponent<RectTransform>());

        // Error text
        GameObject textObj = new GameObject("ErrorText");
        textObj.transform.SetParent(root.transform, false);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = errorMessage;
        tmp.fontSize = 36;
        tmp.color = Color.red;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        FullStretch(textObj.GetComponent<RectTransform>());

        if (showDebugLogs) Debug.Log($"[BugEffect] Fake error: \"{errorMessage}\"");

        yield return new WaitForSeconds(errorDisplayDuration);

        Destroy(root);
    }

    // ═════════════════════════════════════════════
    //  HELPERS
    // ═════════════════════════════════════════════

    private static void FullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (enableRandomForces)
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, forceRadius);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Unity 6 uses TextMeshPro by default

[RequireComponent(typeof(Collider2D))]
public class BugManager : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Trigger fires only once, then gets disabled")]
    public bool oneTimeOnly = false;

    [Tooltip("Requires X passes through trigger to activate")]
    [Min(1)]
    public int requiredPasses = 1;

    [Tooltip("Delay before bug effects happen (in seconds)")]
    public float delayBeforeEffect = 0f;

    private int currentPasses = 0;
    private bool hasTriggered = false;

    [Header("Visual Glitches")]
    [Tooltip("Enable screen shake effect")]
    public bool enableScreenShake = false;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 0.3f;

    [Tooltip("Flash the screen")]
    public bool enableScreenFlash = false;
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    [Tooltip("Flash an object/image briefly then remove it")]
    public bool enableObjectFlash = false;
    public GameObject objectToFlash;
    public Transform flashSpawnPoint;
    public float objectFlashDuration = 0.5f;
    [Tooltip("Fade out the object instead of instant removal")]
    public bool fadeOutObject = true;
    [Tooltip("Scale the object up during flash")]
    public bool scaleUpDuringFlash = false;
    public float flashScale = 1.5f;

    [Tooltip("Distort/corrupt sprites temporarily")]
    public bool corruptSprites = false;
    public List<SpriteRenderer> spritesToCorrupt = new List<SpriteRenderer>();
    public float corruptionDuration = 1f;

    [Header("Audio Glitches")]
    [Tooltip("Play a glitch sound")]
    public bool playGlitchSound = false;
    public AudioClip glitchSound;

    [Tooltip("Distort background music pitch")]
    public bool distortMusic = false;
    public float musicPitchChange = 0.5f;
    public float musicDistortDuration = 2f;

    [Header("Object Spawning")]
    [Tooltip("Spawn objects on trigger")]
    public bool spawnObjects = false;
    public List<GameObject> objectsToSpawn = new List<GameObject>();
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("If true, spawns at random points. If false, spawns at all points")]
    public bool spawnAtRandomPoint = true;

    [Tooltip("How many random spawn points to use (if spawnAtRandomPoint is true)")]
    [Min(1)]
    public int numberOfRandomSpawns = 1;

    [Header("Object Destruction")]
    [Tooltip("Destroy specific objects")]
    public bool destroyObjects = false;
    public List<GameObject> objectsToDestroy = new List<GameObject>();

    [Tooltip("Destroy objects with specific tag")]
    public bool destroyByTag = false;
    public string tagToDestroy = "Enemy";

    [Tooltip("Delay before destroying objects")]
    public float destroyDelay = 0f;

    [Header("Physics Chaos")]
    [Tooltip("Apply random forces to nearby rigidbodies")]
    public bool applyRandomForces = false;
    public float forceRadius = 5f;
    public float forceStrength = 10f;

    [Tooltip("Reverse gravity temporarily")]
    public bool reverseGravity = false;
    public float gravityReverseDuration = 2f;

    [Header("Player Effects")]
    [Tooltip("Teleport player to another location")]
    public bool teleportPlayer = false;
    public Transform teleportDestination;

    [Tooltip("Invert player controls temporarily")]
    public bool invertControls = false;
    public float controlInvertDuration = 3f;

    [Tooltip("Make player invincible temporarily")]
    public bool makePlayerInvincible = false;
    public float invincibilityDuration = 5f;

    [Tooltip("Add corruption to the corruption manager")]
    public bool addCorruption = false;
    [Range(0f, 100f)]
    public float corruptionAmount = 10f;

    [Header("UI Glitches")]
    [Tooltip("Hide UI temporarily")]
    public bool hideUI = false;
    public float uiHideDuration = 2f;

    [Tooltip("Show fake error message")]
    public bool showFakeError = false;
    public string errorMessage = "ERROR 404: Reality not found";
    public float errorDisplayDuration = 3f;

    [Header("Time Manipulation")]
    [Tooltip("Slow down or speed up time")]
    public bool manipulateTime = false;
    [Range(0.1f, 3f)]
    public float timeScale = 0.5f;
    public float timeManipulationDuration = 2f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Collider2D triggerCollider;
    private AudioSource audioSource;
    private Vector2 originalGravity;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        if (playGlitchSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered && oneTimeOnly) return;

        currentPasses++;

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Player passed through trigger. Pass count: {currentPasses}/{requiredPasses}");

        if (currentPasses >= requiredPasses)
        {
            if (delayBeforeEffect > 0)
            {
                StartCoroutine(TriggerBugWithDelay());
            }
            else
            {
                TriggerBug(other.gameObject);
            }

            if (oneTimeOnly)
            {
                hasTriggered = true;
                triggerCollider.enabled = false;
            }
            else
            {
                currentPasses = 0;
            }
        }
    }

    private IEnumerator TriggerBugWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeEffect);
        TriggerBug(GameObject.FindGameObjectWithTag("Player"));
    }

    private void TriggerBug(GameObject player)
    {
        if (showDebugLogs)
            Debug.LogWarning($"[BugMaker] Triggering bug effects!");

        // Visual Glitches
        if (enableScreenShake)
            StartCoroutine(ScreenShake());

        if (enableScreenFlash)
            StartCoroutine(ScreenFlash());

        if (enableObjectFlash && objectToFlash != null)
            StartCoroutine(ObjectFlash());

        if (corruptSprites)
            StartCoroutine(CorruptSprites());

        // Audio
        if (playGlitchSound && glitchSound != null)
        {
            audioSource.clip = glitchSound;
            audioSource.Play();
        }

        if (distortMusic)
            StartCoroutine(DistortMusic());

        // Spawning
        if (spawnObjects)
            SpawnObjects();

        // Destruction
        if (destroyObjects)
            StartCoroutine(DestroyObjectsWithDelay());

        if (destroyByTag)
            StartCoroutine(DestroyObjectsByTag());

        // Physics
        if (applyRandomForces)
            ApplyRandomForces();

        if (reverseGravity)
            StartCoroutine(ReverseGravity());

        // Player Effects
        if (teleportPlayer && teleportDestination != null)
            player.transform.position = teleportDestination.position;

        if (invertControls)
            StartCoroutine(InvertPlayerControls(player));

        if (makePlayerInvincible)
            StartCoroutine(MakePlayerInvincible(player));

        if (addCorruption && CorruptionManager.Instance != null)
        {
            CorruptionManager.Instance.AddCorruption(corruptionAmount);
            if (showDebugLogs)
                Debug.Log($"[BugMaker] Added {corruptionAmount} corruption");
        }

        // UI
        if (hideUI)
            StartCoroutine(HideUI());

        if (showFakeError)
            StartCoroutine(ShowFakeError());

        // Time
        if (manipulateTime)
            StartCoroutine(ManipulateTime());
    }

    #region Visual Effects

    private IEnumerator ScreenShake()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 originalPos = mainCam.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            mainCam.transform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = originalPos;
    }

    private IEnumerator ScreenFlash()
    {
        GameObject flashObj = new GameObject("ScreenFlash");
        SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
        sr.color = flashColor;
        sr.sortingOrder = 9999;

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            flashObj.transform.position = mainCam.transform.position + Vector3.forward * 5f;
            flashObj.transform.localScale = new Vector3(50, 50, 1);
        }

        float elapsed = 0f;
        Color startColor = flashColor;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / flashDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(flashObj);
    }

    private IEnumerator ObjectFlash()
    {
        // Spawn point bestimmen
        Vector3 spawnPosition = flashSpawnPoint != null
            ? flashSpawnPoint.position
            : transform.position;

        // Objekt spawnen
        GameObject flashedObj = Instantiate(objectToFlash, spawnPosition,
            flashSpawnPoint != null ? flashSpawnPoint.rotation : Quaternion.identity);

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Object flash: {objectToFlash.name} spawned at {spawnPosition}");

        Vector3 originalScale = flashedObj.transform.localScale;
        Vector3 targetScale = scaleUpDuringFlash ? originalScale * flashScale : originalScale;

        // Optional: Scale up effect
        if (scaleUpDuringFlash)
        {
            float scaleTime = objectFlashDuration * 0.3f; // 30% of duration for scale up
            float elapsed = 0f;

            while (elapsed < scaleTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scaleTime;
                flashedObj.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
        }

        // Fade out effect
        if (fadeOutObject)
        {
            SpriteRenderer[] spriteRenderers = flashedObj.GetComponentsInChildren<SpriteRenderer>();
            CanvasGroup canvasGroup = flashedObj.GetComponent<CanvasGroup>();

            // If it has a CanvasGroup (UI element), fade that
            if (canvasGroup != null)
            {
                float elapsed = 0f;
                float fadeStart = objectFlashDuration * 0.5f; // Start fading halfway through

                yield return new WaitForSeconds(fadeStart);

                while (elapsed < (objectFlashDuration - fadeStart))
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / (objectFlashDuration - fadeStart));
                    yield return null;
                }
            }
            // Otherwise fade sprite renderers
            else if (spriteRenderers.Length > 0)
            {
                Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
                foreach (var sr in spriteRenderers)
                {
                    originalColors[sr] = sr.color;
                }

                float elapsed = 0f;
                float fadeStart = objectFlashDuration * 0.5f;

                yield return new WaitForSeconds(fadeStart);

                while (elapsed < (objectFlashDuration - fadeStart))
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / (objectFlashDuration - fadeStart));

                    foreach (var kvp in originalColors)
                    {
                        if (kvp.Key != null)
                        {
                            Color newColor = kvp.Value;
                            newColor.a = alpha;
                            kvp.Key.color = newColor;
                        }
                    }
                    yield return null;
                }
            }
            else
            {
                // No fade, just wait
                yield return new WaitForSeconds(objectFlashDuration);
            }
        }
        else
        {
            // No fade, just wait full duration
            yield return new WaitForSeconds(objectFlashDuration);
        }

        Destroy(flashedObj);

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Object flash removed");
    }

    private IEnumerator CorruptSprites()
    {
        Dictionary<SpriteRenderer, Sprite> originalSprites = new Dictionary<SpriteRenderer, Sprite>();

        foreach (var sr in spritesToCorrupt)
        {
            if (sr == null) continue;
            originalSprites[sr] = sr.sprite;

            // Flip or distort sprite
            sr.flipX = !sr.flipX;
            sr.color = new Color(Random.value, Random.value, Random.value, sr.color.a);
        }

        yield return new WaitForSeconds(corruptionDuration);

        foreach (var kvp in originalSprites)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sprite = kvp.Value;
                kvp.Key.flipX = !kvp.Key.flipX;
                kvp.Key.color = Color.white;
            }
        }
    }

    #endregion

    #region Audio Effects

    private IEnumerator DistortMusic()
    {
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        List<AudioSource> musicSources = new List<AudioSource>();
        List<float> originalPitches = new List<float>();

        foreach (var audio in allAudio)
        {
            if (audio.isPlaying && audio != audioSource)
            {
                musicSources.Add(audio);
                originalPitches.Add(audio.pitch);
                audio.pitch = musicPitchChange;
            }
        }

        yield return new WaitForSeconds(musicDistortDuration);

        for (int i = 0; i < musicSources.Count; i++)
        {
            if (musicSources[i] != null)
                musicSources[i].pitch = originalPitches[i];
        }
    }

    #endregion

    #region Spawning & Destruction

    private void SpawnObjects()
    {
        if (objectsToSpawn.Count == 0 || spawnPoints.Count == 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("[BugMaker] No objects or spawn points defined!");
            return;
        }

        if (spawnAtRandomPoint)
        {
            for (int i = 0; i < numberOfRandomSpawns; i++)
            {
                GameObject randomObj = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

                Instantiate(randomObj, randomPoint.position, randomPoint.rotation);

                if (showDebugLogs)
                    Debug.Log($"[BugMaker] Spawned {randomObj.name} at {randomPoint.name}");
            }
        }
        else
        {
            foreach (var spawnPoint in spawnPoints)
            {
                GameObject objToSpawn = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
                Instantiate(objToSpawn, spawnPoint.position, spawnPoint.rotation);

                if (showDebugLogs)
                    Debug.Log($"[BugMaker] Spawned {objToSpawn.name} at {spawnPoint.name}");
            }
        }
    }

    private IEnumerator DestroyObjectsWithDelay()
    {
        if (destroyDelay > 0)
            yield return new WaitForSeconds(destroyDelay);

        foreach (var obj in objectsToDestroy)
        {
            if (obj != null)
            {
                if (showDebugLogs)
                    Debug.Log($"[BugMaker] Destroyed {obj.name}");
                Destroy(obj);
            }
        }
    }

    private IEnumerator DestroyObjectsByTag()
    {
        if (destroyDelay > 0)
            yield return new WaitForSeconds(destroyDelay);

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToDestroy);

        foreach (var obj in taggedObjects)
        {
            if (showDebugLogs)
                Debug.Log($"[BugMaker] Destroyed {obj.name} (Tag: {tagToDestroy})");
            Destroy(obj);
        }
    }

    #endregion

    #region Physics

    private void ApplyRandomForces()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, forceRadius);

        foreach (var col in nearbyColliders)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * forceStrength, ForceMode2D.Impulse);

                if (showDebugLogs)
                    Debug.Log($"[BugMaker] Applied random force to {rb.gameObject.name}");
            }
        }
    }

    private IEnumerator ReverseGravity()
    {
        originalGravity = Physics2D.gravity;
        Physics2D.gravity = -originalGravity;

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Gravity reversed!");

        yield return new WaitForSeconds(gravityReverseDuration);

        Physics2D.gravity = originalGravity;

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Gravity restored!");
    }

    #endregion

    #region Player Effects

    private IEnumerator InvertPlayerControls(GameObject player)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            // Hier müsstest du in deinem PlayerMovement Script eine Methode hinzufügen
            // pm.InvertControls(true);

            if (showDebugLogs)
                Debug.Log($"[BugMaker] Player controls inverted! (Implement InvertControls in PlayerMovement)");
        }

        yield return new WaitForSeconds(controlInvertDuration);

        if (pm != null)
        {
            // pm.InvertControls(false);
        }
    }

    private IEnumerator MakePlayerInvincible(GameObject player)
    {
        // Hier müsstest du in deinem Player Health/Damage Script invincibility setzen
        // PlayerHealth health = player.GetComponent<PlayerHealth>();
        // if (health != null) health.SetInvincible(true);

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Player is invincible! (Implement in your health system)");

        yield return new WaitForSeconds(invincibilityDuration);

        // if (health != null) health.SetInvincible(false);
    }

    #endregion

    #region UI Effects

    private IEnumerator HideUI()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach (var canvas in canvases)
        {
            canvas.enabled = false;
        }

        yield return new WaitForSeconds(uiHideDuration);

        foreach (var canvas in canvases)
        {
            canvas.enabled = true;
        }
    }

    private IEnumerator ShowFakeError()
    {
        // Erstelle ein einfaches Error UI mit TextMeshPro (Unity 6 Standard)
        GameObject errorObj = new GameObject("FakeError");
        Canvas canvas = errorObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        errorObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        errorObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textObj = new GameObject("ErrorText");
        textObj.transform.SetParent(errorObj.transform);

        // Use TextMeshPro for Unity 6
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = errorMessage;
        text.fontSize = 36;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Optional: Add a dark background panel
        GameObject bgObj = new GameObject("ErrorBackground");
        bgObj.transform.SetParent(errorObj.transform);
        bgObj.transform.SetAsFirstSibling(); // Behind text

        UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        yield return new WaitForSeconds(errorDisplayDuration);

        Destroy(errorObj);
    }

    #endregion

    #region Time Manipulation

    private IEnumerator ManipulateTime()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Time scale changed to {timeScale}");

        yield return new WaitForSecondsRealtime(timeManipulationDuration);

        Time.timeScale = originalTimeScale;

        if (showDebugLogs)
            Debug.Log($"[BugMaker] Time scale restored to {originalTimeScale}");
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw trigger area
        Gizmos.color = Color.yellow;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }

        // Draw force radius
        if (applyRandomForces)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, forceRadius);
        }

        // Draw spawn points
        if (spawnObjects && spawnPoints.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }

        // Draw teleport destination
        if (teleportPlayer && teleportDestination != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
            Gizmos.DrawLine(transform.position, teleportDestination.position);
        }

        // Draw object flash spawn point
        if (enableObjectFlash && flashSpawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(flashSpawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, flashSpawnPoint.position);
        }
    }
}
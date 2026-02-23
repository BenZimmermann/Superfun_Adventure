using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BossRoomTrigger.cs
// ─────────────────────────────────────────────────────────────
//  Attach to an empty GameObject with a BoxCollider2D (Is Trigger ON).
//  Position it at the entrance of the boss room.
//
//  When the Player walks in:
//    1. Seals the entrance (door / gate closes)
//    2. Spawns the boss at a defined spawn point
//    3. Spawns the left and right room walls
//    4. Fades in the boss health bar UI
//    5. Optionally plays an intro cutscene (camera move + music)
//    6. Fires a configurable delay before the boss becomes active
//
//  This trigger fires ONCE (oneShot = true by default).
//
//  Scene setup checklist:
//    □ Empty GO with BoxCollider2D (Is Trigger ON) at room entrance
//    □ BossRoomTrigger.cs on that GO
//    □ Boss prefab (has BossController, BossEnvironment etc.)
//    □ Left/Right wall prefabs (simple collider objects)
//    □ Boss Canvas (with BossHealthBar) — starts hidden (alpha 0 / disabled)
//    □ Entrance door/gate GO (optional — gets disabled on trigger)
//    □ Player GameObject tagged "Player"
// ─────────────────────────────────────────────────────────────
[RequireComponent(typeof(Collider2D))]
public class BossRoomTrigger : MonoBehaviour
{
    // ═════════════════════════════════════════════
    //  INSPECTOR
    // ═════════════════════════════════════════════

    [Header("=== Trigger Settings ===")]
    [Tooltip("Trigger fires only once.")]
    [SerializeField] private bool oneShot = true;
    [Tooltip("Seconds after the player enters before the boss activates.")]
    [SerializeField] private float activationDelay = 2f;

    // ─── Boss ─────────────────────────────────────
    [Header("=== Boss ===")]
    [Tooltip("Boss prefab to instantiate (must have BossController).")]
    [SerializeField] private GameObject bossPrefab;
    [Tooltip("Where the boss spawns. Place an empty GO at the center top of the room.")]
    [SerializeField] private Transform bossSpawnPoint;
    [Tooltip("If the boss already exists in the scene (not a prefab), assign it here instead.")]
    [SerializeField] private GameObject bossInstance;

    // ─── Room Walls ───────────────────────────────
    [Header("=== Room Walls ===")]
    [Tooltip("Left wall prefab or scene GO. Spawned/enabled to seal the room.")]
    [SerializeField] private GameObject leftWallPrefab;
    [SerializeField] private Transform leftWallSpawnPoint;

    [Tooltip("Right wall prefab or scene GO.")]
    [SerializeField] private GameObject rightWallPrefab;
    [SerializeField] private Transform rightWallSpawnPoint;

    [Tooltip("Already-placed wall GameObjects to simply enable (alternative to spawning prefabs).")]
    [SerializeField] private List<GameObject> wallsToEnable = new List<GameObject>();

    // ─── Entrance Gate ────────────────────────────
    [Header("=== Entrance Gate ===")]
    [Tooltip("Gate/door that closes behind the player. Set to null to skip.")]
    [SerializeField] private GameObject entranceGate;
    [Tooltip("Delay before the gate closes (let the player fully enter first).")]
    [SerializeField] private float gatecloseDelay = 0.3f;
    [Tooltip("Optional Animator on the gate — triggers 'Close' bool.")]
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private string gateCloseTrigger = "Close";

    // ─── Boss UI ──────────────────────────────────
    [Header("=== Boss UI ===")]
    [Tooltip("The Canvas that contains the boss health bar. Starts hidden.")]
    [SerializeField] private GameObject bossUICanvas;
    [Tooltip("Seconds to fade the UI in (0 = instant).")]
    [SerializeField] private float uiFadeInDuration = 0.8f;
    [Tooltip("CanvasGroup on the boss UI canvas used for fading (auto-added if missing).")]
    [SerializeField] private CanvasGroup bossUICanvasGroup;

    // ─── Camera ───────────────────────────────────
    [Header("=== Camera Intro ===")]
    [Tooltip("Move camera to this position for a brief boss reveal, then return.")]
    [SerializeField] private bool doCameraIntro = false;
    [SerializeField] private Transform cameraIntroTarget;
    [SerializeField] private float cameraIntroDuration = 1.5f;
    [SerializeField] private float cameraHoldDuration = 1f;

    // ─── Music ────────────────────────────────────
    [Header("=== Music ===")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip bossMusicClip;
    [Tooltip("Fade out current music before playing boss music.")]
    [SerializeField] private float musicFadeDuration = 1f;

    // ─── Debug ────────────────────────────────────
    [Header("=== Debug ===")]
    [SerializeField] private bool showDebugLogs = true;

    // ═════════════════════════════════════════════
    //  PRIVATE STATE
    // ═════════════════════════════════════════════

    private bool hasTriggered = false;
    private Collider2D triggerCollider;

    // ═════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═════════════════════════════════════════════

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;

        // Ensure UI starts hidden
        if (bossUICanvas != null)
        {
            if (bossUICanvasGroup == null)
                bossUICanvasGroup = bossUICanvas.GetComponent<CanvasGroup>()
                                 ?? bossUICanvas.AddComponent<CanvasGroup>();

            bossUICanvasGroup.alpha = 0f;
            bossUICanvasGroup.interactable = false;
            bossUICanvasGroup.blocksRaycasts = false;
            bossUICanvas.SetActive(false);
        }

        // Ensure walls start disabled (will be enabled/spawned on trigger)
        foreach (var wall in wallsToEnable)
            if (wall != null) wall.SetActive(false);

        // Ensure boss instance starts disabled if pre-placed
        if (bossInstance != null)
            bossInstance.SetActive(false);
    }

    // ═════════════════════════════════════════════
    //  TRIGGER
    // ═════════════════════════════════════════════

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered && oneShot) return;

        hasTriggered = true;

        if (showDebugLogs)
            Debug.Log("[BossRoomTrigger] Player entered — starting boss room sequence.");

        StartCoroutine(BossRoomSequence(other.gameObject));

        // Disable trigger so it can't fire again
        if (oneShot) triggerCollider.enabled = false;
    }

    // ═════════════════════════════════════════════
    //  MAIN SEQUENCE
    // ═════════════════════════════════════════════

    private IEnumerator BossRoomSequence(GameObject player)
    {
        // ── 1. Close entrance gate ───────────────
        yield return new WaitForSeconds(gatecloseDelay);
        CloseEntranceGate();

        // ── 2. Seal room walls ───────────────────
        SpawnOrEnableWalls();

        // ── 3. Optional camera intro ────────────
        if (doCameraIntro && cameraIntroTarget != null)
            yield return StartCoroutine(CameraIntro());

        // ── 4. Fade in boss music ────────────────
        if (bossMusicClip != null)
            StartCoroutine(TransitionMusic());

        // ── 5. Spawn / activate boss ─────────────
        GameObject boss = SpawnOrActivateBoss();

        // ── 6. Activation delay (boss is dormant)
        //      BossController.Start() is already called but we can
        //      keep the boss frozen via the BossController's isTransitioning
        //      flag — here we just wait before showing UI
        yield return new WaitForSeconds(activationDelay);

        // ── 7. Show boss UI ──────────────────────
        if (bossUICanvas != null)
            yield return StartCoroutine(FadeInUI());

        // ── 8. Activate boss — starts attacking ──
        if (boss != null)
        {
            BossController bc = boss.GetComponent<BossController>();
            bc?.Activate();
        }

        if (showDebugLogs)
            Debug.Log("[BossRoomTrigger] Sequence complete — boss is active.");
    }

    // ═════════════════════════════════════════════
    //  STEP IMPLEMENTATIONS
    // ═════════════════════════════════════════════

    // ─── Gate ────────────────────────────────────

    private void CloseEntranceGate()
    {
        if (entranceGate == null) return;

        if (gateAnimator != null)
        {
            gateAnimator.SetTrigger(gateCloseTrigger);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Gate animator triggered.");
        }
        else
        {
            // No animator — just enable the gate object (a solid wall collider)
            entranceGate.SetActive(true);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Gate enabled.");
        }
    }

    // ─── Walls ───────────────────────────────────

    private void SpawnOrEnableWalls()
    {
        // Enable pre-placed wall GameObjects
        foreach (var wall in wallsToEnable)
        {
            if (wall != null)
            {
                wall.SetActive(true);
                if (showDebugLogs) Debug.Log($"[BossRoomTrigger] Wall enabled: {wall.name}");
            }
        }

        // Spawn left wall prefab
        if (leftWallPrefab != null && leftWallSpawnPoint != null)
        {
            Instantiate(leftWallPrefab, leftWallSpawnPoint.position, leftWallSpawnPoint.rotation);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Left wall spawned.");
        }

        // Spawn right wall prefab
        if (rightWallPrefab != null && rightWallSpawnPoint != null)
        {
            Instantiate(rightWallPrefab, rightWallSpawnPoint.position, rightWallSpawnPoint.rotation);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Right wall spawned.");
        }
    }

    // ─── Boss ────────────────────────────────────

    private GameObject SpawnOrActivateBoss()
    {
        // Priority: pre-placed instance > spawn from prefab
        if (bossInstance != null)
        {
            bossInstance.SetActive(true);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Boss instance activated.");
            return bossInstance;
        }

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
            if (showDebugLogs) Debug.Log("[BossRoomTrigger] Boss prefab spawned.");
            return boss;
        }

        Debug.LogWarning("[BossRoomTrigger] No boss prefab or instance assigned!");
        return null;
    }

    // ─── UI Fade ─────────────────────────────────

    private IEnumerator FadeInUI()
    {
        bossUICanvas.SetActive(true);
        bossUICanvasGroup.alpha = 0f;
        bossUICanvasGroup.interactable = true;
        bossUICanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < uiFadeInDuration)
        {
            elapsed += Time.deltaTime;
            bossUICanvasGroup.alpha = Mathf.Clamp01(elapsed / uiFadeInDuration);
            yield return null;
        }

        bossUICanvasGroup.alpha = 1f;
        if (showDebugLogs) Debug.Log("[BossRoomTrigger] Boss UI faded in.");
    }

    // ─── Camera Intro ─────────────────────────────

    private IEnumerator CameraIntro()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        // Disable player camera follow during intro if CinemachineBrain / follow script exists
        // (hook your camera follow component here if needed)

        Vector3 originPos = cam.transform.position;
        Vector3 targetPos = new Vector3(
            cameraIntroTarget.position.x,
            cameraIntroTarget.position.y,
            originPos.z);  // keep Z for 2D

        // Pan to boss
        for (float t = 0f; t < cameraIntroDuration; t += Time.deltaTime)
        {
            cam.transform.position = Vector3.Lerp(originPos, targetPos, t / cameraIntroDuration);
            yield return null;
        }
        cam.transform.position = targetPos;

        // Hold on boss
        yield return new WaitForSeconds(cameraHoldDuration);

        // Pan back to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 returnPos = player != null
            ? new Vector3(player.transform.position.x, player.transform.position.y, originPos.z)
            : originPos;

        for (float t = 0f; t < cameraIntroDuration; t += Time.deltaTime)
        {
            cam.transform.position = Vector3.Lerp(targetPos, returnPos, t / cameraIntroDuration);
            yield return null;
        }
        cam.transform.position = returnPos;

        if (showDebugLogs) Debug.Log("[BossRoomTrigger] Camera intro done.");
    }

    // ─── Music Transition ─────────────────────────

    private IEnumerator TransitionMusic()
    {
        // Fade out any playing AudioSources
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        var playing = new List<(AudioSource src, float origVol)>();

        foreach (var src in allSources)
        {
            if (src.isPlaying && src != musicSource)
                playing.Add((src, src.volume));
        }

        // Fade out
        for (float t = 0f; t < musicFadeDuration; t += Time.deltaTime)
        {
            float ratio = 1f - (t / musicFadeDuration);
            foreach (var (src, origVol) in playing)
                if (src != null) src.volume = origVol * ratio;
            yield return null;
        }
        foreach (var (src, _) in playing)
            if (src != null) src.Stop();

        // Play boss music
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        musicSource.clip = bossMusicClip;
        musicSource.volume = 1f;
        musicSource.Play();

        if (showDebugLogs) Debug.Log("[BossRoomTrigger] Boss music started.");
    }

    // ═════════════════════════════════════════════
    //  GIZMOS
    // ═════════════════════════════════════════════

    private void OnDrawGizmosSelected()
    {
        // Trigger area
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            Gizmos.DrawCube(transform.position, col.bounds.size);

        // Boss spawn
        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 0.6f);
            Gizmos.DrawLine(transform.position, bossSpawnPoint.position);
        }

        // Wall spawns
        if (leftWallSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(leftWallSpawnPoint.position, Vector3.one);
        }
        if (rightWallSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(rightWallSpawnPoint.position, Vector3.one);
        }

        // Gate
        if (entranceGate != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(entranceGate.transform.position,
                                entranceGate.transform.localScale);
        }

        // Camera intro target
        if (doCameraIntro && cameraIntroTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(cameraIntroTarget.position, 0.5f);
            Gizmos.DrawLine(transform.position, cameraIntroTarget.position);
        }
    }
}
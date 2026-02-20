using UnityEngine;

/// <summary>
/// Handles all environment changes tied to boss phases.
/// Attach to a dedicated "BossEnvironment" GameObject in the room.
/// Connect background layers, wall colliders, and lighting here.
/// </summary>
public class BossEnvironment : MonoBehaviour
{
    [Header("=== Background Layers (one per phase) ===")]
    [SerializeField] private GameObject backgroundPhase1;
    [SerializeField] private GameObject backgroundPhase2;
    [SerializeField] private GameObject backgroundPhase3;
    [SerializeField] private GameObject backgroundPhase4;

    [Header("=== Room Walls (for ShrinkWalls) ===")]
    [Tooltip("Left wall collider / transform")]
    [SerializeField] private Transform leftWall;
    [Tooltip("Right wall collider / transform")]
    [SerializeField] private Transform rightWall;
    [SerializeField] private float wallShrinkAmount = 2f;   // units to move each wall inward

    [Header("=== Lighting ===")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight;
    [SerializeField] private Color[] phaseLightColors = new Color[4];

    [Header("=== Parallax / VFX ===")]
    [SerializeField] private ParticleSystem[] phaseParticles; // one particle system per phase

    // ─────────────────────────────────────────────
    //  Phase Switching
    // ─────────────────────────────────────────────

    /// <summary>Call this from BossController on every phase transition.</summary>
    /// <param name="phase">1–4</param>
    public void SetPhase(int phase)
    {
        DisableAllBackgrounds();
        StopAllParticles();

        switch (phase)
        {
            case 1: ApplyPhase1Environment(); break;
            case 2: ApplyPhase2Environment(); break;
            case 3: ApplyPhase3Environment(); break;
            case 4: ApplyPhase4Environment(); break;
        }

        SetLightColor(phase - 1);
        PlayParticlesForPhase(phase - 1);

        Debug.Log($"[Environment] Switched to phase {phase}.");
    }

    // ─────────────────────────────────────────────
    //  Individual Phase Environments — extend freely
    // ─────────────────────────────────────────────

    private void ApplyPhase1Environment()
    {
        if (backgroundPhase1 != null) backgroundPhase1.SetActive(true);
        // TODO: e.g. mild green tint, normal room size
    }

    private void ApplyPhase2Environment()
    {
        if (backgroundPhase2 != null) backgroundPhase2.SetActive(true);
        // TODO: e.g. electric blue flicker, scanlines VFX
    }

    private void ApplyPhase3Environment()
    {
        if (backgroundPhase3 != null) backgroundPhase3.SetActive(true);
        ShrinkWalls(); // walls get smaller in phase 3
        // TODO: e.g. red warning tint
    }

    private void ApplyPhase4Environment()
    {
        if (backgroundPhase4 != null) backgroundPhase4.SetActive(true);
        // Walls might already be shrunk from phase 3 — shrink further or keep
        // TODO: e.g. dark chaos theme, screen shake
    }

    // ─────────────────────────────────────────────
    //  Wall Shrink — smooth slide
    // ─────────────────────────────────────────────

    [Header("=== Wall Slide ===")]
    [SerializeField] private float wallSlideDuration = 1.5f;  // seconds to complete slide

    private Vector3 _leftWallOriginal;
    private Vector3 _rightWallOriginal;
    private Coroutine _leftSlideCoroutine;
    private Coroutine _rightSlideCoroutine;

    private void Awake()
    {
        if (leftWall != null) _leftWallOriginal = leftWall.position;
        if (rightWall != null) _rightWallOriginal = rightWall.position;
    }

    /// <summary>
    /// Smoothly slides both walls inward by <see cref="wallShrinkAmount"/> units.
    /// Safe to call multiple times — each call slides from the current position.
    /// </summary>
    public void ShrinkWalls()
    {
        if (leftWall != null)
        {
            if (_leftSlideCoroutine != null) StopCoroutine(_leftSlideCoroutine);
            Vector3 target = leftWall.position + new Vector3(wallShrinkAmount, 0f, 0f);
            _leftSlideCoroutine = StartCoroutine(SlideWall(leftWall, target));
        }

        if (rightWall != null)
        {
            if (_rightSlideCoroutine != null) StopCoroutine(_rightSlideCoroutine);
            Vector3 target = rightWall.position - new Vector3(wallShrinkAmount, 0f, 0f);
            _rightSlideCoroutine = StartCoroutine(SlideWall(rightWall, target));
        }

        Debug.Log("[Environment] Walls sliding inward.");
    }

    /// <summary>Smoothly resets both walls to their original positions.</summary>
    public void ResetWalls()
    {
        if (leftWall != null)
        {
            if (_leftSlideCoroutine != null) StopCoroutine(_leftSlideCoroutine);
            _leftSlideCoroutine = StartCoroutine(SlideWall(leftWall, _leftWallOriginal));
        }

        if (rightWall != null)
        {
            if (_rightSlideCoroutine != null) StopCoroutine(_rightSlideCoroutine);
            _rightSlideCoroutine = StartCoroutine(SlideWall(rightWall, _rightWallOriginal));
        }
    }

    private System.Collections.IEnumerator SlideWall(Transform wall, Vector3 targetPos)
    {
        Vector3 startPos = wall.position;
        float elapsed = 0f;

        while (elapsed < wallSlideDuration)
        {
            elapsed += Time.deltaTime;
            // SmoothStep gives a nice ease-in/ease-out feel
            float t = Mathf.SmoothStep(0f, 1f, elapsed / wallSlideDuration);
            wall.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        wall.position = targetPos;
    }

    // ─────────────────────────────────────────────
    //  Lights
    // ─────────────────────────────────────────────

    private void SetLightColor(int index)
    {
        if (globalLight == null) return;
        if (index < 0 || index >= phaseLightColors.Length) return;
        globalLight.color = phaseLightColors[index];
    }

    // ─────────────────────────────────────────────
    //  Particles
    // ─────────────────────────────────────────────

    private void StopAllParticles()
    {
        if (phaseParticles == null) return;
        foreach (var ps in phaseParticles)
            if (ps != null) ps.Stop();
    }

    private void PlayParticlesForPhase(int index)
    {
        if (phaseParticles == null || index < 0 || index >= phaseParticles.Length) return;
        if (phaseParticles[index] != null) phaseParticles[index].Play();
    }

    // ─────────────────────────────────────────────
    //  Cleanup Helpers
    // ─────────────────────────────────────────────

    private void DisableAllBackgrounds()
    {
        if (backgroundPhase1 != null) backgroundPhase1.SetActive(false);
        if (backgroundPhase2 != null) backgroundPhase2.SetActive(false);
        if (backgroundPhase3 != null) backgroundPhase3.SetActive(false);
        if (backgroundPhase4 != null) backgroundPhase4.SetActive(false);
    }

    /// <summary>Called by BossController when the boss is defeated.</summary>
    public void OnBossDefeated()
    {
        StopAllParticles();
        ResetWalls();
        // TODO: play victory fanfare, unlock door, etc.
        Debug.Log("[Environment] Boss defeated — resetting environment.");
    }
}
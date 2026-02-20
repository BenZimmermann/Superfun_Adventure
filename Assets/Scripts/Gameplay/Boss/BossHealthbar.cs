using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Cuphead-style Boss Health Bar.
/// Attach to a Canvas UI element that contains a Slider named "HealthSlider"
/// and optionally phase segment markers.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMPro.TextMeshProUGUI bossNameText;

    [Header("=== Phase Segment Markers ===")]
    [Tooltip("UI Image objects placed at 75%, 50%, 25% on the health bar.")]
    [SerializeField] private RectTransform[] phaseMarkers;

    [Header("=== Colors per Phase ===")]
    [SerializeField]
    private Color[] phaseColors = new Color[]
    {
        new Color(0.2f, 0.9f, 0.2f),   // Phase 1 — green
        new Color(0.9f, 0.7f, 0.1f),   // Phase 2 — yellow
        new Color(0.9f, 0.4f, 0.1f),   // Phase 3 — orange
        new Color(0.9f, 0.1f, 0.1f)    // Phase 4 — red
    };

    [Header("=== Hit Flash ===")]
    [SerializeField] private float hitFlashDuration = 0.1f;

    private int maxHealth;

    // ─────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────

    public void Initialize(int max)
    {
        maxHealth = max;
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = max;
        }
        SetPhaseColor(0);
    }

    public void UpdateHealth(int current)
    {
        if (healthSlider != null)
            healthSlider.value = current;

        StopAllCoroutines();
        StartCoroutine(HitFlash());
    }

    public void SetPhaseColor(int phaseIndex)
    {
        if (fillImage == null) return;
        if (phaseIndex < 0 || phaseIndex >= phaseColors.Length) return;
        fillImage.color = phaseColors[phaseIndex];
    }

    public void SetBossName(string name)
    {
        if (bossNameText != null)
            bossNameText.text = name;
    }

    // ─────────────────────────────────────────────
    //  Visual Feedback
    // ─────────────────────────────────────────────

    private IEnumerator HitFlash()
    {
        if (fillImage == null) yield break;

        Color original = fillImage.color;
        fillImage.color = Color.white;
        yield return new WaitForSeconds(hitFlashDuration);
        fillImage.color = original;
    }
}
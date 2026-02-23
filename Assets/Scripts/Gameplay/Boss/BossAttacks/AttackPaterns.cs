using System.Collections.Generic;
using UnityEngine;

// AttackPattern.cs
// ─────────────────────────────────────────────────────────────
//  Serializable data container for a single named attack pattern.
//  Used by BossController for CodeRain and Spike patterns.
//
//  Each pattern holds:
//    • A display name (shown in Inspector for clarity)
//    • A list of spawn point Transforms that fire simultaneously
//    • An optional per-pattern override for burst count / delay
//
//  No MonoBehaviour — never placed on a GameObject.
// ─────────────────────────────────────────────────────────────

[System.Serializable]
public class AttackPattern
{
    [Tooltip("Display name shown in the Inspector (e.g. 'Wide Rain', 'Left Cluster').")]
    public string patternName = "Pattern";

    [Tooltip("All spawn points in this pattern fire at the same time.")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Tooltip("Override burst count for this pattern. 0 = use global default.")]
    public int burstCountOverride = 0;

    [Tooltip("Override delay between bursts. 0 = use global default.")]
    public float burstDelayOverride = 0f;
}
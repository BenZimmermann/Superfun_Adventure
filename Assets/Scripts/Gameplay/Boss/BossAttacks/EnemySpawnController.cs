using System.Collections.Generic;
using UnityEngine;

// EnemySpawnController.cs
// ─────────────────────────────────────────────────────────────
//  Attach to the Boss GameObject (or a dedicated Manager).
//  BossController holds a reference and calls TrySpawnEnemy().
//  Enforces a live-enemy cap (default: 2).
// ─────────────────────────────────────────────────────────────
public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private int maxEnemies = 2;

    private readonly List<GameObject> _liveEnemies = new List<GameObject>();

    /// <summary>
    /// Spawns <paramref name="prefab"/> at <paramref name="position"/> if under cap.
    /// Destroyed enemies are pruned automatically before the check.
    /// Returns the new GameObject or null if the cap is reached.
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
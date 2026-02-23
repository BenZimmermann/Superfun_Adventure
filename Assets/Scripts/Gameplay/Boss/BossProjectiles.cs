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
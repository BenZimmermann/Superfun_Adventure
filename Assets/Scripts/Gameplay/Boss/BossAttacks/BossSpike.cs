using UnityEngine;

// Spike.cs
// ─────────────────────────────────────────────────────────────
//  Prefab setup:
//    • SpriteRenderer   (spike sprite, pointing UP)
//    • Rigidbody2D      Body Type: Kinematic, Gravity Scale: 0
//    • PolygonCollider2D / BoxCollider2D   Is Trigger: ON
//    • Spike            (this script)
//
//  Place the prefab at ground level — the script moves it upward.
//  BossController spawns and destroys it automatically.
// ─────────────────────────────────────────────────────────────
public class Spike : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float riseSpeed = 3f;
    [SerializeField] private float riseHeight = 1.5f;

    private Vector3 startPos;
    private bool rising = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (!rising) return;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        if (transform.position.y >= startPos.y + riseHeight)
            rising = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        DamageHelper.TryDamage(other, damage, DamageSource.Boss);
    }
}
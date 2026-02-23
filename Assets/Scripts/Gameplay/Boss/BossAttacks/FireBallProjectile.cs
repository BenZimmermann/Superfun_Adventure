using UnityEngine;

// FireballProjectile.cs
// ─────────────────────────────────────────────────────────────
//  Prefab setup:
//    • SpriteRenderer       (fireball sprite, facing RIGHT)
//    • Rigidbody2D          Body Type: Kinematic, Gravity Scale: 0
//    • CircleCollider2D     Is Trigger: ON
//    • FireballProjectile   (this script)
//    • Animator             (optional looping fire animation)
// ─────────────────────────────────────────────────────────────
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 6f;

    private Vector2 direction;

    private void Start()
    {
        // Snap direction toward the player in 90° increments at spawn time
        direction = DamageHelper.DirectionToPlayerSnapped(transform.position);

        // Flip sprite horizontally when moving right
        if (direction.x > 0f)
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DamageHelper.TryDamage(other, damage, DamageSource.Boss);
            Destroy(gameObject);
        }
        // Fix: Use IsTouchingLayers with LayerMask for "Ground"
        else if (other.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private BulletData bulletData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null && ((1 << collision.gameObject.layer) & bulletData.enemyLayer) != 0)
        {
            damageable.TakeDamage(bulletData.damage, DamageSource.Player);
            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & bulletData.floorLayer) != 0)
        {
            Destroy(gameObject);
        }
        else if (((1 << collision.gameObject.layer) & bulletData.playerLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
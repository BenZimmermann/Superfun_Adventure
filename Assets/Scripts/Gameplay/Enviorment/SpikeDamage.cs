using UnityEngine;


public class SpikeDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    // Achte auf das "2D" im Methodennamen und beim Parameter!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogWarning("Spike-Tilemap kollidiert mit: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamageToTarget(collision.gameObject);
        }
    }

    private void DealDamageToTarget(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount, DamageSource.Enemy);
        }
    }
}


using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BulletData bulletData;

    private float nextFireTime = 0f;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // Hole die PlayerMovement Komponente
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement Komponente nicht gefunden! PlayerShoot braucht PlayerMovement.");
        }
    }

    private void Update()
    {
        // Prüfe Input vom InputController (auch wenn gehalten)
        if (InputController.ShootWasHeld && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + bulletData.fireRate;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile Prefab oder Fire Point fehlt!");
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement nicht gefunden!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && bulletData.bulletSprite != null)
        {
            spriteRenderer.sprite = bulletData.bulletSprite;
        }
 
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 shootDirection = playerMovement._isFacingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = shootDirection * bulletData.projectileSpeed;
        }

        Destroy(projectile, bulletData.lifeTime);
    }
}
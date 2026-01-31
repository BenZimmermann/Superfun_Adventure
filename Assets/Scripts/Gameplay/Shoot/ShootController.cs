using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float fireRate = 0.5f;

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
            nextFireTime = Time.time + fireRate;
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

        // Erstelle das Projektil
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Hole die Rigidbody2D Komponente
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Schieße in die Richtung, in die der Spieler schaut (nutzt PlayerMovement.IsFacingRight)
            Vector2 shootDirection = playerMovement._isFacingRight ? Vector2.right : Vector2.left;
            rb.linearVelocity = shootDirection * projectileSpeed;
        }

        // Optional: Zerstöre das Projektil nach 3 Sekunden
        Destroy(projectile, 3f);
    }
}
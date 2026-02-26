using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BulletData bulletData;

    private float nextFireTime = 0f;
    private PlayerMovement playerMovement;
    private Camera mainCamera;

    private void Awake()
    {
        // Grundlegende Komponenten-Referenz holen
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement nicht gefunden! Das Skript muss auf dem Player liegen.");
        }
    }

    private void OnEnable()
    {
        // Suche Kamera beim Aktivieren (wichtig für Spawnen/Szenenwechsel)
        FindMyCamera();
    }

    private void FindMyCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = Object.FindFirstObjectByType<Camera>();
            }
        }
    }

    private void Update()
    {
        // Sicherheits-Check: Falls Kamera noch nicht da war oder bulletData fehlt
        if (mainCamera == null) FindMyCamera();
        if (bulletData == null) return;

        // Input-Abfrage über den InputController
        if (InputController.ShootWasHeld && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + bulletData.fireRate;
        }
    }

    private void Shoot()
    {
        // Abbruch, wenn wichtige Referenzen fehlen
        if (projectilePrefab == null || firePoint == null || playerMovement == null || mainCamera == null)
        {
            Debug.LogWarning("Schießen abgebrochen: Fehlende Referenzen im PlayerShoot Skript.");
            return;
        }
        // 1. MAUSPOSITION BERECHNEN
        // Wir projizieren den Mauspunkt auf die Welt-Ebene (Z-Differenz beachten)
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);

        // Vektor von der Waffe zur Maus
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)firePoint.position).normalized;

        // 2. WINKEL-LIMITIERUNG (90° Halbkreis nach oben)
        // Berechne den Winkel in Grad
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (playerMovement._isFacingRight)
        {
            // Rechts schauend: Erlaube nur 0° (vorne) bis 90° (oben)
            // Alles unter 0 (Boden) wird auf 0 gesetzt
            angle = Mathf.Clamp(angle, 0f, 90f);
        }
        else
        {
            // Links schauend: Erlaube nur 90° (oben) bis 180° (vorne links)
            // Falls der Winkel negativ ist (unten), setzen wir ihn auf die Horizontale (180)
            if (angle < 0) angle = 180f;
            angle = Mathf.Clamp(angle, 90f, 180f);
        }

        // Richtung aus dem korrigierten Winkel neu berechnen
        direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        // 3. PROJEKTIL ERZEUGEN
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Sprite aus BulletData zuweisen
        SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && bulletData.bulletSprite != null)
        {
            spriteRenderer.sprite = bulletData.bulletSprite;
        }

        // 4. PHYSIK UND ROTATION
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Geschwindigkeit aus BulletData übernehmen
            rb.linearVelocity = direction * bulletData.projectileSpeed;

            // Kugel in Flugrichtung drehen
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Zerstörung nach der Zeit aus BulletData
        Destroy(projectile, bulletData.lifeTime);
        playerMovement.TriggerShootAnimation();
    }

    private void OnDisable()
    {
        // Kamera-Referenz lösen, um Speicherlecks/Fehler bei Szenenwechsel zu vermeiden
        mainCamera = null;
    }
}
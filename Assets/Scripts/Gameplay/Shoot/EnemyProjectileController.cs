using UnityEngine;

/// <summary>
/// Handles all enemy projectiles including bullets and lasers
/// </summary>
public class EnemyProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private BulletData bulletData;
    [SerializeField] private ProjectileType projectileType = ProjectileType.Bullet;

    [Header("Laser Settings")]
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float laserWidth = 0.2f;
    [SerializeField] private LayerMask laserHitLayers;
    [SerializeField] private bool continuousDamage = true;
    [SerializeField] private float damageTickRate = 0.2f;

    private float lifeTimer = 0f;
    private bool hasHit = false;
    private LineRenderer lineRenderer;
    private BoxCollider2D laserCollider;
    private float damageTickTimer = 0f;
    private Vector2 direction;

    public enum ProjectileType
    {
        Bullet,      // Standard projectile
        Laser        // Continuous beam
    }

    private void Awake()
    {
        if (projectileType == ProjectileType.Laser)
        {
            SetupLaser();
        }
    }

    private void Start()
    {
        if (bulletData == null)
        {
            Debug.LogError($"[{gameObject.name}] BulletData is missing!", this);
            Destroy(gameObject);
            return;
        }

        // Get initial direction from velocity for bullets
        if (projectileType == ProjectileType.Bullet)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                direction = rb.linearVelocity.normalized;
            }
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (lifeTimer >= bulletData.lifeTime)
        {
            DestroyProjectile();
            return;
        }

        if (projectileType == ProjectileType.Laser)
        {
            UpdateLaser();
        }
    }

    #region Laser Logic

    private void SetupLaser()
    {
        // Setup LineRenderer for visual
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;

        // Set laser color (red for enemy)
        lineRenderer.startColor = new Color(1f, 0.2f, 0.2f, 1f);
        lineRenderer.endColor = new Color(1f, 0.5f, 0.5f, 0.8f);

        // Set material (use default)
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Setup BoxCollider2D for hit detection
        laserCollider = gameObject.AddComponent<BoxCollider2D>();
        laserCollider.isTrigger = true;

        // Set layers to hit (player + ground)
        laserHitLayers = LayerMask.GetMask("Player", "Ground");
    }

    private void UpdateLaser()
    {
        if (lineRenderer == null) return;

        Vector2 startPos = transform.position;
        Vector2 laserDirection = transform.right; // Laser points in transform's right direction

        // Raycast to find endpoint
        RaycastHit2D hit = Physics2D.Raycast(startPos, laserDirection, laserLength, laserHitLayers);

        Vector2 endPos;
        if (hit.collider != null)
        {
            endPos = hit.point;

            // Deal damage if hit player
            if (((1 << hit.collider.gameObject.layer) & LayerMask.GetMask("Player")) != 0)
            {
                if (continuousDamage)
                {
                    damageTickTimer += Time.deltaTime;
                    if (damageTickTimer >= damageTickRate)
                    {
                        DealDamageToTarget(hit.collider.gameObject);
                        damageTickTimer = 0f;
                    }
                }
                else if (!hasHit)
                {
                    DealDamageToTarget(hit.collider.gameObject);
                    hasHit = true;
                }
            }
        }
        else
        {
            endPos = startPos + laserDirection * laserLength;
        }

        // Update line renderer
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // Update collider to match laser
        UpdateLaserCollider(startPos, endPos);

        // Debug draw
        Debug.DrawLine(startPos, endPos, Color.red);
    }

    private void UpdateLaserCollider(Vector2 startPos, Vector2 endPos)
    {
        if (laserCollider == null) return;

        // Calculate center and size
        Vector2 center = (startPos + endPos) / 2f;
        float length = Vector2.Distance(startPos, endPos);

        // Set collider size and position
        laserCollider.size = new Vector2(length, laserWidth);
        laserCollider.offset = transform.InverseTransformPoint(center);
    }

    #endregion

    #region Bullet Logic

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (projectileType == ProjectileType.Laser) return; // Laser handles its own collision

        // Check if hit player
        if (((1 << collision.gameObject.layer) & bulletData.playerLayer) != 0)
        {
            DealDamageToTarget(collision.gameObject);
            SpawnHitEffect();
            DestroyProjectile();
        }
        // Check if hit ground
        else if (((1 << collision.gameObject.layer) & bulletData.floorLayer) != 0)
        {
            SpawnHitEffect();
            DestroyProjectile();
        }
    }

    #endregion

    #region Damage & Effects

    private void DealDamageToTarget(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(bulletData.damage, DamageSource.Enemy);
        }
    }

    private void SpawnHitEffect()
    {
        if (bulletData.hitEffect != null)
        {
            Instantiate(bulletData.hitEffect, transform.position, Quaternion.identity);
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the projectile type (called from BaseEnemy)
    /// </summary>
    public void SetProjectileType(ProjectileType type)
    {
        projectileType = type;

        if (type == ProjectileType.Laser && lineRenderer == null)
        {
            SetupLaser();
        }
    }

    /// <summary>
    /// Sets the bullet data (called from BaseEnemy)
    /// </summary>
    public void SetBulletData(BulletData data)
    {
        bulletData = data;
    }

    /// <summary>
    /// Gets the bullet data
    /// </summary>
    public BulletData GetBulletData()
    {
        return bulletData;
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (projectileType == ProjectileType.Laser)
        {
            Gizmos.color = Color.red;
            Vector3 direction = transform.right;
            Gizmos.DrawRay(transform.position, direction * laserLength);
        }
    }

    #endregion
}
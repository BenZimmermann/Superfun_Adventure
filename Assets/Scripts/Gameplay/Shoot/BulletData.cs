using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletData", menuName = "Bullet/Bullet Data")]
public class BulletData : ScriptableObject
{
    public int damage = 1;
    public float lifeTime = 3f;
    public float fireRate = 0.5f;
    public float projectileSpeed = 10f;

    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public LayerMask floorLayer;

    public Sprite bulletSprite;

    public GameObject hitEffect;

}

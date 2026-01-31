using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Korrekte Methode: Prüfe ob der Layer in der LayerMask enthalten ist
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            Destroy(gameObject);
            Debug.Log("Projectile destroyed on collision with: " + collision.gameObject.name);
        }
    }
}
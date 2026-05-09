using UnityEngine;

public class CannonProjectile : Projectile
{
    [Header("Explosion")]
    public float explosionRadius = 3f; // Bán kính vụ nổ
    public GameObject explosionPrefab;

    protected override void Hit()
    {
        Debug.Log("💥 Cannon HIT");

        // ===== 1. SPAWN VFX =====
        if (explosionPrefab != null)
        {
            Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius); // Tìm tất cả collider trong bán kính vụ nổ

        foreach (Collider col in hits)
        {
            ZombieHealth zh = col.GetComponentInParent<ZombieHealth>(); // Kiểm tra nếu collider thuộc về zombie

            if (zh != null)
            {
                Debug.Log("✅ Damage zombie");
                zh.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    // DEBUG EXPLOSION RANGE
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage;
    private Transform target;

    private Vector3 moveDir;

    public Vector3 offsetDir;


    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position + offsetDir) - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // 🔥 FIX QUAN TRỌNG: tránh skip target
        if (dir.magnitude <= distanceThisFrame)
        {
            Hit();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    protected virtual void Hit()
    {
        ZombieHealth zh = target.GetComponent<ZombieHealth>();

        if (zh != null)
        {
            zh.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    public void SetTargetWithOffset(Transform t, Vector3 offset)
    {
        target = t;
        offsetDir = offset;
    }
}
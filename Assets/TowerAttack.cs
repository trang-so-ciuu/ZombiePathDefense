
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    private Tower tower;
    private float timer = 0f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private TowerTargeting targeting;

    void Start()
    {
        InitAfterUpgrade(); // 🔥 bỏ Invoke luôn
    }

    void Init()
    {
        tower = GetComponent<Tower>(); 
        targeting = GetComponent<TowerTargeting>(); 
        
        if (tower == null) 
        { 
            Debug.LogError("❌ tower NULL sau Init"); 
            return; 
        }
        if (targeting == null) 
        { 
            Debug.LogError("❌ targeting NULL sau Init"); 
            return; 
        }
        tower.onLevelChanged += OnLevelChanged;
    }

    void OnDestroy()
    {
        if (tower != null)
            tower.onLevelChanged -= OnLevelChanged;
    }

    void OnLevelChanged()
    {
        timer = 0f; // Reset timer khi upgrade
    }

    void Update()
    {
        if (!GameManager.Instance.isPlaying) return;

        if (tower == null || targeting == null || targeting.target == null || firePoint == null)
            return;
        Debug.Log("Level: " + tower.currentLevel +
          " | BulletCount: " + (tower.currentLevel + 1));

        // 🔥 FIX vị trí quay
        Vector3 dir = targeting.target.position - firePoint.position;
        dir.y = 0;
        //Vector3 dir = targeting.target.position - tower.modelHolder.position; //
        //dir.y = 0;

        if (dir != Vector3.zero)
        {
            // 🔥 FIX quay model
            Transform model = tower.modelHolder;

            model.rotation = Quaternion.Slerp(
                model.rotation,
                Quaternion.LookRotation(dir),
                10f * Time.deltaTime
            );
        }

        timer += Time.deltaTime;

        if (timer >= tower.GetFireRate())
        {
            int bulletCount = tower.currentLevel + 1;

            for (int i = 0; i < bulletCount; i++)
            {
                Shoot(i, bulletCount);
            }

            PlayShootSound();
            timer = 0f;
        }
    }

    void Shoot(int index, int total)
    {
        Transform target = targeting.target;
        if (target == null) return;

        // 👉 tạo góc spread
        float angleStep = 25f; // độ tỏa (có thể chỉnh)
        float angle = (index - (total - 1) / 2f) * angleStep;

        // 👉 hướng gốc
        Vector3 dir = (target.position - firePoint.position).normalized;
        dir.y = 0;

        // 👉 xoay hướng theo góc
        Quaternion spreadRot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 offset = spreadRot * dir * 5f;

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile proj = bullet.GetComponent<Projectile>();

        if (proj != null)
        {
            //proj.SetTarget(target);
            proj.SetTargetWithOffset(target, offset);
            float totalDamage = tower.GetDamage();
            int bulletCount = tower.currentLevel + 1;

            float damagePerBullet = totalDamage / bulletCount;

            proj.damage = damagePerBullet;
        }
    }
    public void ResetAttack()
    {
        timer = 0f;
    }

    public void InitAfterUpgrade()
    {
        // 🔥 lấy lại reference
        tower = GetComponent<Tower>();
        targeting = GetComponent<TowerTargeting>();

        // reset timer
        timer = 0f;

        Debug.Log("✅ Re-init TowerAttack sau upgrade");
    }

    void PlayShootSound()
    {
        if (tower == null) return;

        switch (tower.towerType)
        {
            case Tower.TowerType.Archer:
                AudioManager.Instance.PlaySFX("arrow");
                break;

            case Tower.TowerType.Canon:
                AudioManager.Instance.PlaySFX("canon");
                break;
        }
    }

}
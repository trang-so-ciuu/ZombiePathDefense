
using UnityEngine;
using System.Collections.Generic;

public class TowerTargeting : MonoBehaviour
{
    public float range = 30f;
    public Transform target;

    private Tower tower;
    private static List<Transform> allZombies = new List<Transform>();
    private float findTimer = 0f;
    private float findRate = 0.2f;

    void Start()
    {
        tower = GetComponent<Tower>();

        if (tower != null)
        {
            tower.onLevelChanged += OnLevelChanged;
            SyncRange(); // Lấy range ngay khi start
        }
    }

    void OnDestroy()
    {
        if (tower != null)
            tower.onLevelChanged -= OnLevelChanged;
    }

    void OnLevelChanged()
    {
        SyncRange();
        target = null; // 🔥 bắt buộc
    }

    void SyncRange()
    {
        if (tower != null)
            range = tower.GetRange();
    }

    void Update()
    {
        findTimer += Time.deltaTime;
        if (findTimer >= findRate)
        {
            findTimer = 0f;
            FindNearestZombie();
        }

        if (target == null)
            FindNearestZombie();

        HandleRangeVisual();
    }

    // Tìm zombie gần nhất trong range
    void FindNearestZombie()
    {
        allZombies.RemoveAll(z => z == null);

        float nearest = Mathf.Infinity;
        target = null;

        foreach (Transform zombie in allZombies)
        {
            Vector3 a = transform.position;
            Vector3 b = zombie.position;
            a.y = 0;
            b.y = 0;

            float dist = Vector3.Distance(a, b);
            if (dist <= range && dist < nearest)
            {
                nearest = dist;
                target = zombie;
            }
        }
    }

    // Các tower gọi để đăng ký/unregister zombie khi spawn/despawn
    public static void RegisterZombie(Transform zombie)
    {
        if (!allZombies.Contains(zombie))
            allZombies.Add(zombie);
    }

    public static void UnregisterZombie(Transform zombie)
    {
        allZombies.Remove(zombie);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    void HandleRangeVisual()
    {
        if (tower == null) return;

        // nếu có target → hiện range
        if (target != null)
        {
            tower.ShowRange(true);
        }
        else
        {
            // chỉ tắt nếu không đang select
            if (!BuildManager.Instance.IsUpgradeUIOpen())
            {
                tower.ShowRange(false);
            }
        }
    }
}
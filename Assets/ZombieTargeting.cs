
using System.Collections.Generic;
using UnityEngine;

public class ZombieTargeting : MonoBehaviour
{
    public List<Transform> targets = new List<Transform>();
    private int currentIndex = 0;

    void Start()
    {
        FindAllHumans();
        AssignNearestFreeTarget();
    }

    void Update()
    {
        Transform current = GetCurrentTarget();

        if (current == null)
        {
            FindAllHumans();
            AssignNearestFreeTarget();
            return;
        }

        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.GetAttackSlot(transform, current);
        }
    }

    void OnDestroy()
    {
        // Zombie bị xóa → trả lại chỗ đã giữ
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.ReleaseByZombie(transform);
    }

    void FindAllHumans()
    {
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        targets.Clear();
        foreach (GameObject h in humans)
            targets.Add(h.transform);
    }

    // Tìm và đặt chỗ Human gần nhất còn trống
    public void AssignNearestFreeTarget()
    {
        if (targets.Count == 0)
        {
            FindAllHumans();
        }

        // 🔥 QUAN TRỌNG: bỏ target cũ
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.ReleaseByZombie(transform);
        }

        targets.RemoveAll(t => t == null);

        if (targets.Count == 0) return;

        targets.Sort((a, b) =>
            Vector3.Distance(transform.position, a.position)
            .CompareTo(Vector3.Distance(transform.position, b.position))
        );

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;

            if (ZombieManager.Instance == null || ZombieManager.Instance.TryReserve(transform, targets[i]))
            {
                currentIndex = i;
                return;
            }
        }

        // fallback: nếu tất cả Human đã có zombie giữ chỗ, vẫn chọn Human gần nhất.
        // ZombieMovement sẽ tự chia slot đứng quanh target để tránh dồn cụm.
        currentIndex = 0;
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.GetAttackSlot(transform, targets[currentIndex]);
        }
    }

    public Transform GetCurrentTarget()
    {
        if (targets.Count == 0) return null;
        if (currentIndex >= targets.Count) currentIndex = 0;

        if (targets[currentIndex] == null)
        {
            // 🔥 QUAN TRỌNG
            if (ZombieManager.Instance != null)
            {
                ZombieManager.Instance.ReleaseByZombie(transform);
            }

            targets.RemoveAt(currentIndex);
            AssignNearestFreeTarget();
            return targets.Count > 0 ? targets[currentIndex] : null;
        }

        return targets[currentIndex];
    }

    // Gọi từ HumanHealth khi Human chết
    public void SwitchToNearestFreeTarget()
    {
        FindAllHumans();
        AssignNearestFreeTarget();
    }
}

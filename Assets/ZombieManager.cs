// ZombieTargetManager.cs - Singleton quản lý target toàn cục
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance { get; private set; }

    // Key: Transform của Human | Value: Transform của Zombie đang giữ chỗ
    private Dictionary<Transform, Transform> reservations = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, Transform> zombieSlotTargets = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, int> zombieSlotIndices = new Dictionary<Transform, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Zombie xin đặt chỗ một Human
    // Trả về true nếu thành công, false nếu đã bị Zombie khác giữ
    public bool TryReserve(Transform zombie, Transform human)
    {
        // Dọn các entry null (Human đã chết)
        CleanUp();

        if (!reservations.ContainsKey(human))
        {
            // Hủy chỗ cũ của zombie này (nếu có) trước khi đặt chỗ mới
            ReleaseByZombie(zombie);
            reservations[human] = zombie;
            return true;
        }

        // Nếu chính zombie này đã giữ chỗ rồi thì OK
        if (reservations[human] == zombie) return true;

        return false; // Human đã bị Zombie khác giữ
    }

    // Zombie trả lại chỗ khi không cần nữa (bị destroy, hoặc chuyển target)
    public void ReleaseByZombie(Transform zombie)
    {
        List<Transform> toRemove = new List<Transform>();
        foreach (var pair in reservations)
        {
            if (pair.Value == zombie)
                toRemove.Add(pair.Key);
        }
        foreach (var key in toRemove)
            reservations.Remove(key);

        zombieSlotTargets.Remove(zombie);
        zombieSlotIndices.Remove(zombie);
    }

    // Giải phóng chỗ của một Human cụ thể (Human chết)
    public void ReleaseHuman(Transform human)
    {
        if (reservations.ContainsKey(human))
            reservations.Remove(human);
    }

    // Kiểm tra Human này đã bị đặt chỗ chưa
    public bool IsReserved(Transform human)
    {
        CleanUp();
        return reservations.ContainsKey(human);
    }

    public int GetAttackSlot(Transform zombie, Transform human)
    {
        CleanUp();

        if (zombie == null || human == null) return 0;

        if (zombieSlotTargets.TryGetValue(zombie, out Transform currentHuman) &&
            currentHuman == human &&
            zombieSlotIndices.TryGetValue(zombie, out int currentSlot))
        {
            return currentSlot;
        }

        zombieSlotTargets.Remove(zombie);
        zombieSlotIndices.Remove(zombie);

        HashSet<int> usedSlots = new HashSet<int>();
        foreach (var pair in zombieSlotTargets)
        {
            if (pair.Value == human && zombieSlotIndices.TryGetValue(pair.Key, out int slot))
            {
                usedSlots.Add(slot);
            }
        }

        int newSlot = 0;
        while (usedSlots.Contains(newSlot))
        {
            newSlot++;
        }

        zombieSlotTargets[zombie] = human;
        zombieSlotIndices[zombie] = newSlot;

        return newSlot;
    }

    private void CleanUp()
    {
        List<Transform> nullKeys = new List<Transform>();
        foreach (var pair in reservations)
        {
            if (pair.Key == null || pair.Value == null)
                nullKeys.Add(pair.Key);
        }
        foreach (var key in nullKeys)
            reservations.Remove(key);

        List<Transform> nullSlotZombies = new List<Transform>();
        foreach (var pair in zombieSlotTargets)
        {
            if (pair.Key == null || pair.Value == null)
            {
                nullSlotZombies.Add(pair.Key);
            }
        }

        foreach (var zombie in nullSlotZombies)
        {
            zombieSlotTargets.Remove(zombie);
            zombieSlotIndices.Remove(zombie);
        }
    }
}

using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("References")]
    public ZombieSpawner spawner;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int maxWave = 5;
    public int zombieIncreasePerWave = 3;

    [Header("Wave 1 Random")]
    public int wave1Min = 20;
    public int wave1Max = 25;

    // 🔥 Base thực tế sau khi random wave 1
    private int currentBaseZombie;

    void Awake()
    {
        Instance = this;
    }

    public void StartWave()
    {
        currentWave++;

        if (currentWave > maxWave)
        {
            Debug.Log("✅ All waves completed!");
            return;
        }

        int zombieCount = CalculateZombieCount();

        // Debug
        Debug.Log($" Wave: {currentWave}");
        Debug.Log($" Base (wave1): {currentBaseZombie}");
        Debug.Log($" Spawn Count: {zombieCount}");

        // UI
        WaveUI ui = FindFirstObjectByType<WaveUI>();
        if (ui != null)
        {
            ui.waveText.text = "Wave " + currentWave;
        }

        // Spawn
        spawner.SpawnWave(zombieCount);
    }

    
    int CalculateZombieCount()
    {
        if (currentWave == 1)
        {
            Debug.Log($"⚙️ Min: {wave1Min} | Max: {wave1Max}");

            int raw = Random.Range(wave1Min, wave1Max + 1);
            Debug.Log("🎯 Random raw: " + raw);
            currentBaseZombie = raw;
            return currentBaseZombie;
        }

        return currentBaseZombie + (currentWave - 1) * zombieIncreasePerWave; 
    }

    // ================= RESET =================
    public void ResetWave()
    {
        currentWave = 0;
        currentBaseZombie = 0;
    }

    // ================= SAFETY =================
    void OnValidate()
    {
        if (wave1Min > wave1Max)
            wave1Min = wave1Max;

        if (zombieIncreasePerWave < 0)
            zombieIncreasePerWave = 0;
    }
}
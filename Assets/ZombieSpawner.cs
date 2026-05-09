using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public MapAreaManager mapArea;

    [Header("Zombie Settings")]
    public GameObject[] zombiePrefabs;
    public int numberOfZombies = 18;

    [Header("Cluster Settings")]
    public int numberOfClusters = 4;       // số cụm
    public float clusterRadius = 8f;       // bán kính mỗi cụm
    public float minClusterSpacing = 10f;  // khoảng cách tối thiểu giữa 2 cụm

    [Header("Scaling Per Wave")]
    public float healthIncreasePerWave = 10f;
    public float damageIncreasePerWave = 2f;
    public float speedIncreasePerWave = 0.2f;

    private float minDistance = 1.5f;
    private List<int> usedCells;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Awake()
    {
        usedCells = new List<int>(mapArea.GetUsedCells());
    }

    void SpawnZombies()
    {
        List<Vector3> clusterCenters = GenerateClusterCenters();
        int perCluster = numberOfZombies / clusterCenters.Count;
        int remainder = numberOfZombies % clusterCenters.Count;

        int totalSpawned = 0;

        for (int c = 0; c < clusterCenters.Count; c++)
        {
            int toSpawn = perCluster + (c < remainder ? 1 : 0);
            int spawned = 0;
            int tries = 0;

            while (spawned < toSpawn && tries < 1000)
            {
                tries++;

                Vector2 randomCircle = Random.insideUnitCircle * clusterRadius;
                Vector3 pos = clusterCenters[c] + new Vector3(randomCircle.x, 0, randomCircle.y);

                // distance check
                bool tooClose = false;
                foreach (Vector3 p in spawnedPositions)
                {
                    if (Vector3.Distance(pos, p) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // grid check
                int x = Mathf.FloorToInt((pos.x - transform.position.x + (mapArea.gridX * mapArea.cellSize) / 2f) / mapArea.cellSize);
                int z = Mathf.FloorToInt((pos.z - transform.position.z + (mapArea.gridZ * mapArea.cellSize) / 2f) / mapArea.cellSize);

                if (x < 0 || x >= mapArea.gridX || z < 0 || z >= mapArea.gridZ) continue;

                int index = x * mapArea.gridZ + z;

                if (usedCells.Contains(index)) continue;
                if (Physics.CheckSphere(pos, 0.5f)) continue;

                SpawnZombie(pos);
                spawned++;
                totalSpawned++;
            }
            // Nếu không đủ trong cụm → spawn rải rác ở biên
            while (spawned < toSpawn)
            {
                Vector3 pos = GetRandomEdgePosition();

                SpawnZombie(pos);

                spawned++;
                totalSpawned++;
            }
        }

        Debug.Log($"Zombies Spawned: {totalSpawned} in {clusterCenters.Count} clusters");
        // 1. Nạp số lượng thực tế vào GameManager
        GameManager.Instance.SetZombie(totalSpawned);

        // 2. CHỈ KÍCH HOẠT CHƠI KHI ĐÃ CÓ ZOMBIE TRÊN SÂN
        if (totalSpawned > 0)
        {
            GameManager.Instance.isPlaying = true;
        }



    }

    void SpawnZombie(Vector3 pos)
    {
        Debug.Log("👉 numberOfZombies thực tế: " + numberOfZombies);
        GameObject prefab = zombiePrefabs[Random.Range(0, zombiePrefabs.Length)];
        GameObject zombieObj = Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));

        int wave = WaveManager.Instance.currentWave;

        ZombieHealth zh = zombieObj.GetComponent<ZombieHealth>();
        if (zh != null) zh.maxHealth += (wave - 1) * healthIncreasePerWave;

        ZombieMovement zm = zombieObj.GetComponent<ZombieMovement>();
        if (zm != null) zm.speed += (wave - 1) * speedIncreasePerWave;

        ZombieAttack za = zombieObj.GetComponent<ZombieAttack>();
        if (za != null) za.damage += (wave - 1) * damageIncreasePerWave;

        spawnedPositions.Add(pos);
    }

    List<Vector3> GenerateClusterCenters()
    {
        List<Vector3> centers = new List<Vector3>();
        int maxAttempts = 200;
        int attempts = 0;

        while (centers.Count < numberOfClusters && attempts < maxAttempts)
        {
            attempts++;
            Vector3 candidate = GetRandomEdgePosition();

            // Kiểm tra khoảng cách với cụm đã chọn
            bool tooClose = false;
            foreach (Vector3 existing in centers)
            {
                if (Vector3.Distance(candidate, existing) < minClusterSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                centers.Add(candidate);
        }

        return centers;
    }

    Vector3 GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: return mapArea.GetGridPosition(0, Random.Range(0, mapArea.gridZ));                         // trái
            case 1: return mapArea.GetGridPosition(mapArea.gridX - 1, Random.Range(0, mapArea.gridZ));         // phải
            case 2: return mapArea.GetGridPosition(Random.Range(0, mapArea.gridX), 0);                         // dưới
            default: return mapArea.GetGridPosition(Random.Range(0, mapArea.gridX), mapArea.gridZ - 1);        // trên
        }
    }

    public void SpawnWave(int zombieCount)
    {
        Debug.Log("👉 SpawnWave nhận: " + zombieCount);
        numberOfZombies = zombieCount;

        usedCells = new List<int>(mapArea.GetUsedCells());
        spawnedPositions.Clear();

        // 🔥 RESET trước
        GameManager.Instance.SetZombie(0);
        GameManager.Instance.isPlaying = false;

        SpawnZombies();
    }
}
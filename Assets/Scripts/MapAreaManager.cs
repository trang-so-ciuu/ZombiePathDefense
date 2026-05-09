using System.Collections.Generic;
using UnityEngine;

public class MapAreaManager : MonoBehaviour
{

    [Header("Prefabs")]
    public GameObject treePlatformPrefab;
    public GameObject[] humanPrefabs; // kid1, kid2

    [Header("Grid Settings")]
    public int gridX = 10;
    public int gridZ = 10;
    public float cellSize = 4f;

    [Header("Tree Settings")]
    public int numberOfTrees = 20;

    [Header("Human Settings")]
    public int minHuman = 15;
    public int maxHuman = 25;


    private List<int> usedCells = new List<int>(); // lưu ô đã dùng

    public bool[,] blocked;

    void Start()
    {
        blocked = new bool[gridX, gridZ];
        SpawnTrees();
        SpawnHumans();
    }

    // ================= TREE =================
    void SpawnTrees()
    {
        List<int> cells = GetShuffledCells();

        for (int i = 0; i < numberOfTrees; i++)
        {
            int index = cells[i];
            usedCells.Add(index); // đánh dấu đã dùng

            int x = index / gridZ;
            int z = index % gridZ;

            blocked[x, z] = true;

            Vector3 pos = GetGridPosition(x, z);

            Instantiate(treePlatformPrefab, pos, Quaternion.identity);
            blocked[x, z] = true; // đánh dấu ô này bị block
        }

    }

    // ================= HUMAN =================
    void SpawnHumans()
    {
        int humanCount = Random.Range(minHuman, maxHuman + 1);

        List<int> cells = GetShuffledCells();

        int spawned = 0;

        foreach (int index in cells)
        {
            if (spawned >= humanCount) break;

            // ❗ không spawn vào ô đã có tree
            if (usedCells.Contains(index)) continue;

            // ❗ chỉ spawn ở vùng giữa map
            int x = index / gridZ;
            int z = index % gridZ;

            if (!IsCenterArea(x, z)) continue;

            Vector3 pos = GetGridPosition(x, z);

            // chọn random kid1 hoặc kid2
            GameObject prefab = humanPrefabs[Random.Range(0, humanPrefabs.Length)];

            Instantiate(prefab, pos, Quaternion.identity);

            usedCells.Add(index);
            spawned++;
        }

        Debug.Log("Humans Spawned: " + spawned);
        GameManager.Instance.SetHuman(spawned);

    }

    // ================= GRID =================
     public Vector3 GetGridPosition(int x, int z)
    {
        float startX = -(gridX * cellSize) / 2f + cellSize / 2f;
        float startZ = -(gridZ * cellSize) / 2f + cellSize / 2f;

        float posX = startX + x * cellSize;
        float posZ = startZ + z * cellSize;

        return new Vector3(posX, 11f, posZ) + transform.position;

    }

    // ================= CENTER CHECK =================
     public bool IsCenterArea(int x, int z)
    {
        int minX = gridX / 4;
        int maxX = gridX * 3 / 4;

        int minZ = gridZ / 4;
        int maxZ = gridZ * 3 / 4;

        return (x >= minX && x <= maxX && z >= minZ && z <= maxZ);
    }

    public bool IsBlocked(int x, int z)
    {
        if (x < 0 || z < 0 || x >= gridX || z >= gridZ)
            return true;
        return blocked[x, z];
    }

    public Vector2Int WorldToGrid(Vector3 pos)
    {
        float startX = -(gridX * cellSize) / 2f + cellSize / 2f;
        float startZ = -(gridZ * cellSize) / 2f + cellSize / 2f;

        float localX = pos.x - transform.position.x;
        float localZ = pos.z - transform.position.z;

        int x = Mathf.RoundToInt((localX - startX) / cellSize);
        int z = Mathf.RoundToInt((localZ - startZ) / cellSize);

        return new Vector2Int(x, z);
    }

    // ================= SHUFFLE =================
    public List<int> GetShuffledCells()
    {
        int total = gridX * gridZ;
        List<int> list = new List<int>();

        for (int i = 0; i < total; i++)
            list.Add(i);

        for (int i = 0; i < total; i++)
        {
            int rand = Random.Range(i, total);
            int temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }

        return list;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.green;

        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                Vector3 pos = GetGridPosition(x, z);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }

    public List<int> GetUsedCells()
    {
        return new List<int>(usedCells);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public MapAreaManager map; // 👉 dùng map của bạn

    // 👉 tìm đường từ start → target
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {

        if (map == null)
        {
            map = Object.FindFirstObjectByType<MapAreaManager>();
        }
        // 👉 đổi từ world → grid
        Vector2Int start = map.WorldToGrid(startPos);
        Vector2Int target = map.WorldToGrid(targetPos);

        List<Vector2Int> openList = new List<Vector2Int>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        openList.Add(start);

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];

            // 👉 nếu tới đích
            if (current == target)
            {
                return RetracePath(cameFrom, start, target);
            }

            openList.Remove(current);
            closedList.Add(current);

            // 👉 duyệt 8 hướng (cả chéo)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    int nx = current.x + dx;
                    int nz = current.y + dz;

                    Vector2Int neighbor = new Vector2Int(nx, nz);

                    // 👉 nếu bị block hoặc đã xét rồi thì bỏ
                    if (map.IsBlocked(nx, nz) || closedList.Contains(neighbor))
                        continue;

                    // 🔥 CHẶN ĐI CHÉO (QUAN TRỌNG NHẤT)
                    if (dx != 0 && dz != 0)
                    {
                        if (map.IsBlocked(current.x + dx, current.y) ||
                            map.IsBlocked(current.x, current.y + dz))
                        {
                            continue; // ❌ không cho đi chéo
                        }
                    }

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }
        }

        return null; // 👉 không tìm được đường
    }

    // 👉 dựng lại đường đi
    List<Vector3> RetracePath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int end)
    {
        List<Vector3> path = new List<Vector3>();

        Vector2Int current = end;

        while (current != start)
        {
            // 👉 grid → world (dùng hàm của bạn)
            path.Add(map.GetGridPosition(current.x, current.y));

            current = cameFrom[current];
        }

        path.Reverse(); // 👉 đảo lại cho đúng thứ tự

        return path;
    }
}

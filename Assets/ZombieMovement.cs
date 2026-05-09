using System.Collections.Generic;
using UnityEngine;

public class ZombieMovement : MonoBehaviour
{
    public ZombieTargeting targeting;
    public Pathfinding pathfinding;

    public float speed = 3f; 
    private List<Vector3> path;
    private int currentIndex = 0;

    private float timer = 0f;
    public float updateRate = 0.5f;

    private float stopDistance; // Khoảng cách để dừng lại và quay mặt

    public LineRenderer line;

    [Header("Line Settings")]
    public float lineWidth = 0.2f;
    public Color lineColor = Color.green;
    public Material lineMaterial;

    [Header("Zombie Spacing")]
    public float separationRadius = 2.2f;
    public float minSeparationDistance = 1.4f;
    public float targetStopDistance = 2.2f;
    public float separationStrength = 5f;
    public int overlapResolveIterations = 2;
    public int slotsPerTargetRing = 8;
    public float targetRingSpacing = 1.4f;
    public float faceTargetDistance = 4f;
    public float faceTargetRotationOffsetY = 0f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask = ~0;
    public float obstacleAvoidanceRadius = 2f;
    public float obstacleLookAhead = 2.5f;
    public float obstaclePadding = 0.8f;
    public float obstacleAvoidanceStrength = 6f;
    public float obstacleHeightTolerance = 0.5f;

    private readonly Collider[] nearbyColliders = new Collider[32];
    
    ZombieAudioController audioCtrl;
    private ZombieAnimation zombieAnim;

    void Start()
    {
        // Random nhẹ để zombie khác nhau
        speed += Random.Range(-0.5f, 0.5f);
        stopDistance = 1.0f + Random.Range(0f, 0.5f); 

        // Line renderer
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        line.widthMultiplier = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.sortingOrder = 10;
        
        targeting = GetComponent<ZombieTargeting>();
        pathfinding = Object.FindFirstObjectByType<Pathfinding>();
        audioCtrl = GetComponent<ZombieAudioController>();
        zombieAnim = GetComponent<ZombieAnimation>();
    }

    void Update()
    {

        
        if (!GameManager.Instance.isPlaying) return;

        Transform target = targeting.GetCurrentTarget();

        if (target == null)
        {
            ClearPath();
            return;
        }

        Vector3 standPosition = GetTargetStandPosition(target);
        float distanceToStandPosition = GetFlatDistance(transform.position, standPosition);

        // Nếu đã tới vị trí đứng riêng quanh target thì dừng + quay mặt
        if (distanceToStandPosition < 0.25f)
        {
            ResolveTargetOverlap(target);
            ResolveZombieOverlap();
            ResolveObstacleOverlap(target);
            StopAndLookAt(target);
            return;
        }

        // Update path theo thời gian
        timer += Time.deltaTime;
        if (path == null || path.Count == 0 || timer >= updateRate)
        {
            timer = 0f;

            path = FindMovementPath(target, standPosition);
            currentIndex = 0;
        }

        Move(target);
        ResolveZombieOverlap();
        ResolveObstacleOverlap(target);

        if (audioCtrl != null)
        {
            audioCtrl.HandleFootstep();
        }

        DrawPath();
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isPlaying) return;
        if (targeting == null) return;

        Transform target = targeting.GetCurrentTarget();
        if (target == null) return;

        LookAtTarget(target, true);
    }

    // ================= MOVE =================
    void Move(Transform target)
    {
        if (path == null || path.Count == 0 || currentIndex >= path.Count) return;

        Vector3 targetPos = path[currentIndex];
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0f;

        Vector3 separationDir = GetSeparationDirection();
        Vector3 obstacleDir = GetObstacleAvoidanceDirection(target, moveDir.normalized);
        Vector3 finalDir = moveDir.normalized +
                           separationDir * separationStrength +
                           obstacleDir * obstacleAvoidanceStrength;

        if (finalDir.sqrMagnitude > 0.001f)
        {
            Vector3 nextPosition = transform.position + finalDir.normalized * speed * Time.deltaTime;
            nextPosition = KeepDistanceFromTarget(nextPosition, target);
            nextPosition = KeepDistanceFromZombies(nextPosition);
            nextPosition = KeepDistanceFromObstacles(nextPosition, target);
            transform.position = nextPosition;
        }

        transform.position = new Vector3(transform.position.x, targetPos.y, transform.position.z);

        if (zombieAnim != null)
        {
            zombieAnim.PlayWalk();
        }

        if (GetFlatDistance(transform.position, target.position) <= faceTargetDistance)
        {
            LookAtTarget(target, false);
        }
        else
        {
            Vector3 dir = finalDir.sqrMagnitude > 0.001f ? finalDir : moveDir;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    10f * Time.deltaTime
                );
            }
        }

        Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        if (Vector3.Distance(transform.position, flatTarget) < 0.15f)
        {
            currentIndex++;
        }
    }

    List<Vector3> FindMovementPath(Transform target, Vector3 standPosition)
    {
        List<Vector3> newPath = null;

        if (pathfinding != null)
        {
            newPath = pathfinding.FindPath(transform.position, standPosition);

            if (newPath == null || newPath.Count == 0)
            {
                newPath = pathfinding.FindPath(transform.position, target.position);
            }
        }

        if (newPath == null || newPath.Count == 0)
        {
            newPath = new List<Vector3> { target.position };
        }

        return newPath;
    }

    // ================= AVOID =================
    Vector3 GetSeparationDirection()
    {
        Vector3 separation = Vector3.zero;
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, nearbyColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = nearbyColliders[i];
            nearbyColliders[i] = null;

            if (hit == null) continue;
            ZombieMovement otherZombie = hit.GetComponentInParent<ZombieMovement>();
            if (otherZombie == null || otherZombie == this) continue;

            Vector3 away = transform.position - otherZombie.transform.position;
            away.y = 0f;

            float distance = away.magnitude;
            if (distance < 0.001f)
            {
                away = transform.right;
                distance = 0.001f;
            }

            float weight = Mathf.Clamp01((separationRadius - distance) / separationRadius);
            separation += away.normalized * weight;
        }

        return separation.sqrMagnitude > 0.001f ? separation.normalized : Vector3.zero;
    }

    void ResolveZombieOverlap()
    {
        for (int iteration = 0; iteration < overlapResolveIterations; iteration++)
        {
            transform.position = KeepDistanceFromZombies(transform.position);
        }
    }

    Vector3 KeepDistanceFromZombies(Vector3 position)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(position, minSeparationDistance, nearbyColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = nearbyColliders[i];
            nearbyColliders[i] = null;

            if (hit == null) continue;
            ZombieMovement otherZombie = hit.GetComponentInParent<ZombieMovement>();
            if (otherZombie == null || otherZombie == this) continue;

            Vector3 away = position - otherZombie.transform.position;
            away.y = 0f;

            float distance = away.magnitude;
            if (distance < 0.001f)
            {
                away = transform.right;
                distance = 0.001f;
            }

            float pushDistance = minSeparationDistance - distance;
            if (pushDistance > 0f)
            {
                position += away.normalized * pushDistance;
            }
        }

        return position;
    }

    Vector3 GetObstacleAvoidanceDirection(Transform target, Vector3 moveDirection)
    {
        Vector3 checkCenter = transform.position;
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            checkCenter += moveDirection.normalized * obstacleLookAhead;
        }

        Vector3 avoidance = Vector3.zero;
        int hitCount = Physics.OverlapSphereNonAlloc(
            checkCenter,
            obstacleAvoidanceRadius,
            nearbyColliders,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = nearbyColliders[i];
            nearbyColliders[i] = null;

            if (ShouldIgnoreObstacle(hit, target)) continue;

            Vector3 closestPoint = hit.ClosestPoint(transform.position);
            Vector3 away = transform.position - closestPoint;
            away.y = 0f;

            float distance = away.magnitude;
            if (distance < 0.001f)
            {
                away = transform.position - hit.bounds.center;
                away.y = 0f;
                distance = Mathf.Max(away.magnitude, 0.001f);
            }

            float weight = Mathf.Clamp01((obstacleAvoidanceRadius - distance) / obstacleAvoidanceRadius);
            avoidance += away.normalized * weight;
        }

        return avoidance.sqrMagnitude > 0.001f ? avoidance.normalized : Vector3.zero;
    }

    void ResolveObstacleOverlap(Transform target)
    {
        transform.position = KeepDistanceFromObstacles(transform.position, target);
    }

    Vector3 KeepDistanceFromObstacles(Vector3 position, Transform target)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            position,
            obstaclePadding,
            nearbyColliders,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = nearbyColliders[i];
            nearbyColliders[i] = null;

            if (ShouldIgnoreObstacle(hit, target)) continue;

            Vector3 closestPoint = hit.ClosestPoint(position);
            Vector3 away = position - closestPoint;
            away.y = 0f;

            float distance = away.magnitude;
            if (distance < 0.001f)
            {
                away = position - hit.bounds.center;
                away.y = 0f;
                distance = Mathf.Max(away.magnitude, 0.001f);
            }

            float pushDistance = obstaclePadding - distance;
            if (pushDistance > 0f)
            {
                position += away.normalized * pushDistance;
            }
        }

        return position;
    }

    bool ShouldIgnoreObstacle(Collider hit, Transform target)
    {
        if (hit == null) return true;
        if (hit.isTrigger) return true;
        if (hit.transform == transform || hit.transform.IsChildOf(transform)) return true;
        if (target != null && (hit.transform == target || hit.transform.IsChildOf(target))) return true;
        if (hit.GetComponentInParent<ZombieMovement>() != null) return true;
        if (hit.CompareTag("Human")) return true;
        if (hit.bounds.max.y < transform.position.y - obstacleHeightTolerance) return true;

        return false;
    }

    Vector3 KeepDistanceFromTarget(Vector3 position, Transform target)
    {
        if (target == null) return position;

        Vector3 away = position - target.position;
        away.y = 0f;

        float distance = away.magnitude;
        if (distance < 0.001f)
        {
            away = transform.position - target.position;
            away.y = 0f;

            if (away.sqrMagnitude < 0.001f)
            {
                away = -transform.forward;
            }

            distance = 0.001f;
        }

        if (distance < targetStopDistance)
        {
            position = target.position + away.normalized * targetStopDistance;
            position.y = transform.position.y;
        }

        return position;
    }

    Vector3 GetTargetStandPosition(Transform target)
    {
        if (target == null) return transform.position;

        int slot = 0;
        if (ZombieManager.Instance != null)
        {
            slot = ZombieManager.Instance.GetAttackSlot(transform, target);
        }

        int slotsInRing = Mathf.Max(1, slotsPerTargetRing);
        int ring = slot / slotsInRing;
        int indexInRing = slot % slotsInRing;
        float radius = targetStopDistance + ring * targetRingSpacing;
        float angle = (360f / slotsInRing) * indexInRing + ring * (180f / slotsInRing);
        Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        Vector3 standPosition = target.position + direction * radius;
        standPosition.y = transform.position.y;

        return standPosition;
    }

    void ResolveTargetOverlap(Transform target)
    {
        if (target == null) return;

        transform.position = KeepDistanceFromTarget(transform.position, target);
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ================= STOP =================
    void StopAndLookAt(Transform target)
    {

        if (zombieAnim != null)
        {
            zombieAnim.PlayWalk();
        }
        ClearPath();


        LookAtTarget(target, true);
    }

    void LookAtTarget(Transform target, bool instant)
    {
        if (target == null) return;

        Vector3 lookPoint = GetTargetLookPoint(target);
        Vector3 lookDir = lookPoint - transform.position;
        lookDir.y = 0;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            float yaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg + faceTargetRotationOffsetY;
            Quaternion targetRotation = Quaternion.Euler(0f, yaw, 0f);

            transform.rotation = instant
                ? targetRotation
                : Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    Vector3 GetTargetLookPoint(Transform target)
    {
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        return targetCollider != null ? targetCollider.bounds.center : target.position;
    }

    // ================= UTILS =================
    void ClearPath()
    {
        path = null;
        currentIndex = 0;
        line.positionCount = 0;
    }

    void DrawPath()
    {
        if (path == null || path.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            line.SetPosition(i, path[i] + Vector3.up * 0.2f);
        }
    }
}

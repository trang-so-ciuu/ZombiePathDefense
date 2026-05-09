using UnityEngine;

public class ZombieAudioController : MonoBehaviour
{
    public ZombieTargeting targeting;

    bool canPlayFootstep;
    float stepCooldown = 1.2f;
    float lastStepTime;

    void Start()
    {
        targeting = GetComponent<ZombieTargeting>();

        // chỉ 20% zombie có footstep
        canPlayFootstep = Random.value < 0.2f;
    }

    // ===== WALK =====
    public void HandleFootstep()
    {
        if (!canPlayFootstep) return;

        if (Time.time - lastStepTime < stepCooldown) return;

        Transform target = targeting.GetCurrentTarget();
        if (target == null) return;

        float d = Vector3.Distance(transform.position, target.position);

        if (d > 10f) return;
        if (d > 5f && Random.value > 0.5f) return;

        AudioManager.Instance.PlayZombieSFX("zombie_walk", d);

        lastStepTime = Time.time;
    }

    // ===== ATTACK =====
    public void PlayAttack()
    {
        Transform target = targeting.GetCurrentTarget();
        if (target == null) return;

        float d = Vector3.Distance(transform.position, target.position);

        AudioManager.Instance.PlayZombieSFX("zombie_attack", d);
    }
}

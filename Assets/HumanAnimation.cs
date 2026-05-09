using UnityEngine;

public class HumanAnimation : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        anim.speed = 0;
    }
    void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.isPlaying)
            anim.speed = 1;
        else
            anim.speed = 0;
    }

    // ================= HIT =================
    public void PlayHit()
    {
        anim.SetTrigger("Hit");
    }

    // ================= DIE =================
    public void PlayDie()
    {
        anim.SetTrigger("Die");
    }
}

using UnityEngine;

public class ZombieAnimation : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void PlayWalk()
    {
        anim.SetTrigger("Walk");
    }

    public void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    public void PlayHit()
    {
        anim.SetTrigger("Hit");
    }

    public void PlayDie()
    {
        anim.SetTrigger("Die");
    }
}

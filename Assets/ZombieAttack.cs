using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public float damage = 20f;
    public float attackRange = 1.8f;
    public float attackSpeed = 1.0f;
    private float timer = 0f;
    private ZombieTargeting targeting;
    private ZombieMovement movement;
    
   

    private bool isDead = false;
    private string currentAnim = "";

    ZombieAudioController audioCtrl;
    private ZombieAnimation zombieAnim;

    void Start()
    {
        targeting = GetComponent<ZombieTargeting>();
        movement = GetComponent<ZombieMovement>();
        audioCtrl = GetComponent<ZombieAudioController>();
        zombieAnim = GetComponent<ZombieAnimation>();

    }

    void Update()
    {


        if (isDead) return;

        Transform target = targeting.GetCurrentTarget();

        if (target == null)
        {
            
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            timer += Time.deltaTime;

            if (timer >= attackSpeed)
            {
                
                Attack(target);
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
            
        }
    }

    void Attack(Transform target)
    {
        if (zombieAnim != null)
        {
            // zombieAnim.PlayWalk();
            zombieAnim.PlayAttack();
        }

        if (target == null) return;

        HumanHealth human = target.GetComponent<HumanHealth>();
        if (human == null)
            human = target.GetComponentInChildren<HumanHealth>();

        if (human != null)
        {
            Debug.Log("ZOMBIE CẮN!!!");
            human.TakeDamage(damage);

            if (audioCtrl != null)
            {
                audioCtrl.PlayAttack();
            }
        }
    }

    public void TakeHit()
    {
        if (isDead) return;

        if (zombieAnim != null)
        {
            zombieAnim.PlayHit();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (zombieAnim != null)
        {
            zombieAnim.PlayDie();
        }

        if (movement != null) movement.enabled = false;
        targeting.enabled = false;

        Destroy(gameObject, 3f);
    }

    

}

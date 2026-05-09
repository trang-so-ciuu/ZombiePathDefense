// HumanHealth.cs
using UnityEngine;

public class HumanHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public HealthBarUI healthBar;
    private bool isDead = false;
    private HumanAnimation humanAnim;
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        humanAnim = GetComponent<HumanAnimation>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (humanAnim != null)
        {
            humanAnim.PlayHit();
        }
        healthBar.UpdateHealth(currentHealth);  
        Debug.Log(name + " bị đánh! Máu còn: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(name + " đã chết!");
        if (humanAnim != null)
        {
            humanAnim.PlayDie();
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeHuman(-1);

        // Giải phóng target
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.ReleaseHuman(transform);

        NotifyAllZombies();

        Destroy(gameObject, 2f);
    }

    // 👉 Thông báo cho tất cả zombie đang nhắm vào mình để họ tìm target mới
    void NotifyAllZombies()
    {
        ZombieTargeting[] allZombies = FindObjectsByType<ZombieTargeting>(FindObjectsSortMode.None);
        foreach (ZombieTargeting zombie in allZombies)
        {
            if (zombie.GetCurrentTarget() == this.transform)
                zombie.SwitchToNearestFreeTarget();
        }
    }
}

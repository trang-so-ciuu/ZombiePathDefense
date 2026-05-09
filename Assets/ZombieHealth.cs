using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;
    private ZombieAttack zombieAttack;

    public HealthBarUI healthBar;

  

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        zombieAttack = GetComponent<ZombieAttack>();
        // ✅ Đăng ký zombie cho tower
        TowerTargeting.RegisterZombie(transform);
    }

    private bool isDead = false;

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        healthBar.UpdateHealth(currentHealth);

        if (zombieAttack != null)
        {
            zombieAttack.TakeHit();
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        zombieAttack.Die();
        TowerTargeting.UnregisterZombie(transform);

        // Giải phóng chỗ trong ZombieManager nếu có
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.ReleaseByZombie(transform);

        // Thông báo trừ zombie TRƯỚC khi biến mất khỏi Scene
        if (GameManager.Instance != null)
            GameManager.Instance.ChangeZombie(-1);

        if (CoinManager.Instance != null)
        {
            int wave = WaveManager.Instance.currentWave;

            int coin = 10 + (wave * 5);

            CoinManager.Instance.AddCoin(coin);

            Debug.Log($"💰 Earn coin: {coin} (Wave {wave})");
        }
            

    }

    void OnDestroy()
    {
        // 👉 đảm bảo luôn remove khỏi list (tránh lỗi target null)
        TowerTargeting.UnregisterZombie(transform);
    }
}

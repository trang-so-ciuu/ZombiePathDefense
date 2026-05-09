using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI humanText;
    public TextMeshProUGUI zombieText;
    public TextMeshProUGUI coinText;

    void Update()
    {
        // 👉 lấy số Human
        int humanCount = GameObject.FindGameObjectsWithTag("Human").Length;

        // 👉 lấy số Zombie
        int zombieCount = GameObject.FindGameObjectsWithTag("Zombie").Length;

        // 👉 lấy Coin
        int coin = 0;
        if (CoinManager.Instance != null)
            coin = CoinManager.Instance.currentCoin;

        // 👉 hiển thị lên UI
        humanText.text = humanCount.ToString();
        zombieText.text = zombieCount.ToString();
        coinText.text = coin.ToString();
    }

}

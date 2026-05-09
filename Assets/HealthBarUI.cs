using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;

    float maxHP;

    public void SetMaxHealth(float max)
    {
        maxHP = max;
        gameObject.SetActive(false); // ẩn từ đầu
    }

    public void UpdateHealth(float current)
    {
        fillImage.fillAmount = current / maxHP;

        gameObject.SetActive(true); // hiện khi bị đánh
        CancelInvoke();
        Invoke(nameof(Hide), 2f);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}

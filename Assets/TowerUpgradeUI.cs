using UnityEngine;
using TMPro;

public class TowerUpgradeUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI sellText;

    private Tower currentTower;

    // 👉 gọi khi chọn tower
    public void SetTower(Tower tower)
    {
        currentTower = tower;
        UpdateUI();
    }

    // 👉 cập nhật UI
    public void UpdateUI()
    {
        if (currentTower == null) return;

        nameText.text = currentTower.towerName;

        levelText.text = "Level: " + (currentTower.currentLevel + 1);
        damageText.text = "Damage: " + currentTower.GetDamage();
        rangeText.text = "Range: " + currentTower.GetRange();

        if (currentTower.IsMaxLevel())
            costText.text = "MAX LEVEL";
        else
            costText.text = "Upgrade: " + currentTower.GetUpgradeCost();

        sellText.text = "Sell: " + currentTower.GetSellValue();
    }

    public void OnClickUpgrade()
    {
        Debug.Log("CLICK UPGRADE");
        BuildManager.Instance.UpgradeTower();
    }

    public void OnClickSell()
    {
        BuildManager.Instance.SellTower();
    }
}
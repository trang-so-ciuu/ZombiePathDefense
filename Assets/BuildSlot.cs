using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class BuildSlot : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject treePrefab;
    public GameObject archerTowerPrefab;
    public GameObject cannonTowerPrefab;
    private GameObject currentTower; // 👉 tower hiện tại
    private bool isBuilt = false;
    public Outline outline;

    void OnMouseDown()

    {

        if (!GameManager.Instance.isPlaying)

            return;

        if (isBuilt)

            return;

        BuildManager.Instance.SelectSlot(this);

    }
    public void BuildArcher()
    {
        currentTower = Instantiate(archerTowerPrefab, transform.position, Quaternion.identity);

        Tower t = currentTower.GetComponent<Tower>();
        if (t != null)
        {
            t.ownerSlot = this;
            t.ShowRangeTemp(10f); // Hiển thị phạm vi tạm thời trong giây
        }

        AudioManager.Instance.PlaySFX("build");
        isBuilt = true;
        gameObject.SetActive(false);
    }
    public void BuildCannon()
    {
        currentTower = Instantiate(cannonTowerPrefab, transform.position, Quaternion.identity); 
        Tower t = currentTower.GetComponent<Tower>();
        if (t != null)
        {
            t.ownerSlot = this;
            t.ShowRangeTemp(10f);
        }

        AudioManager.Instance.PlaySFX("build");
        isBuilt = true;
        gameObject.SetActive(false);
    }
    // ================= GET TOWER =================
    public Tower GetTower()
    {
        if (currentTower == null) return null;
        return currentTower.GetComponent<Tower>();
    }
    // ================= SELL =================
    public void Sell()
    {
        if (currentTower == null) return;

        Tower tower = currentTower.GetComponent<Tower>();

        if (tower != null)
        {
            int sellValue = tower.GetSellValue();
            CoinManager.Instance.AddCoin(sellValue);
            AudioManager.Instance.PlaySFX("coin");

            Debug.Log("💰 Sell +" + sellValue);
        }

        Destroy(currentTower);

        gameObject.SetActive(true);
        isBuilt = false;
    }
    public void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }
    IEnumerator HideRangeAfterTime(Tower tower, float time)
    {
        yield return new WaitForSeconds(time);

        if (tower != null && !BuildManager.Instance.IsUpgradeUIOpen())
        {
            tower.ShowRange(false);
        }
    }
}
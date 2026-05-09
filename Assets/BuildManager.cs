using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BuildManager : MonoBehaviour
{

    public static BuildManager Instance;
    private BuildSlot currentSlot;
    [Header("UI")]
    public GameObject buildUI;
    [Header("Cost")]
    public int archerCost = 50;
    public int cannonCost = 70;
    private Tower currentTower; // 👉 tower đang được chọn
    [Header("Upgrade UI")]
    public TowerUpgradeUI upgradeUI; // 👉 UI upgrade

    private Tower lastTower;
    private BuildSlot lastSlot;
    void Awake()
    {
        Instance = this;
        // đảm bảo UI tắt ngay từ đầu
        if (buildUI != null)
            buildUI.SetActive(false);
    }
    // Click vào Tree
    public void SelectSlot(BuildSlot slot)
    {
        AudioManager.Instance.PlaySFX("click");

        if (lastTower != null)
            lastTower.SetHighlight(false);

        if (lastSlot != null)
            lastSlot.SetHighlight(false);

        currentSlot = slot;
        lastSlot = slot;

        slot.SetHighlight(true);

        CloseUpgradeUI();
        buildUI.SetActive(true); // hiện UI
        SetBuildUIPosition(slot.transform.position + Vector3.up * 1.5f); // 👉 đặt UI lên trên cây
    }
    // Click vào nút build Archer
    public void BuildArcher()
    {
        if (currentSlot == null) return;
        // 👉 check tiền trước
        if (!CoinManager.Instance.SpendCoin(archerCost))
        {
            Debug.Log("❌ Không đủ tiền xây Archer");
            AudioManager.Instance.PlaySFX("fail");
            return;
        }
        currentSlot.BuildArcher();
        CloseUI();
    }
    // Click vào nút build Cannon
    public void BuildCannon()
    {
        if (currentSlot == null) return;
        if (!CoinManager.Instance.SpendCoin(cannonCost))
        {
            Debug.Log("❌ Không đủ tiền xây Cannon");
            AudioManager.Instance.PlaySFX("fail");
            return;
        }
        currentSlot.BuildCannon();
        CloseUI();
    }
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // 👉 Chặn click UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // 👉 Luôn xử lý world
        HandleWorldClick();
    }



    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    void HandleWorldClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 1. Thử tìm Tower (tìm từ vật thể bị chạm lên cha)
            Tower tower = hit.collider.GetComponentInParent<Tower>();
            if (tower != null)
            {
                // Nếu click vào đúng tower đang chọn thì không làm gì cả (để tránh Reset UI)
                if (currentTower == tower && upgradeUI.gameObject.activeSelf) return;

                SelectTower(tower);
                return;
            }

            // 2. Thử tìm BuildSlot
            BuildSlot slot = hit.collider.GetComponentInParent<BuildSlot>();
            if (slot != null)
            {
                if (!GameManager.Instance.isPlaying) return;
                SelectSlot(slot);
                return;
            }
        }

        // 3. Nếu click trượt ra ngoài hoàn toàn mới đóng UI
        CloseUI();
        CloseUpgradeUI();
    }
    void CloseUI()
    {
        if (buildUI != null)
            buildUI.SetActive(false);
        if (lastSlot != null)
            lastSlot.SetHighlight(false);

        currentSlot = null;
        lastSlot = null;
    }
    // 👉 Khi click vào Tower
    public void SelectTower(Tower tower)
    {
        AudioManager.Instance.PlaySFX("click");
        if (lastTower != null)
        {
            lastTower.SetHighlight(false);
            lastTower.ShowRange(false);
        }


        if (lastSlot != null)
            lastSlot.SetHighlight(false);


        currentTower = tower;
        lastTower = tower;

        tower.SetHighlight(true);
        tower.ShowRange(true);

        //tower.ShowRange(true, 0.5f);
        CloseUI();
        if (buildUI != null)
            buildUI.SetActive(false);// ẩn UI build khi chọn tower

        if (upgradeUI != null)
        {
            upgradeUI.gameObject.SetActive(true);
            upgradeUI.SetTower(tower);
        }
    }
    void CloseUpgradeUI()
    {
        if (upgradeUI != null)
            upgradeUI.gameObject.SetActive(false);
        if (lastTower != null)
        {
            lastTower.SetHighlight(false);
            lastTower.ShowRange(false);
        }
        currentTower = null;
        lastTower = null;
    }
    public void UpgradeTower()
    {
        if (currentTower == null) return;
        currentTower.Upgrade();
        if (upgradeUI != null)
            upgradeUI.UpdateUI(); //👉 cập nhật UI sau khi upgrade
    }

    public void SellTower()
    {
        if (currentTower == null) return;

        currentTower.ownerSlot.Sell();

        CloseUpgradeUI();
    }
    public bool IsUpgradeUIOpen()
    {
        return upgradeUI != null && upgradeUI.gameObject.activeSelf;
    }

    void SetBuildUIPosition(Vector3 worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransform rect = buildUI.GetComponent<RectTransform>();

        float width = rect.rect.width;
        float height = rect.rect.height;

        float halfWidth = width / 2;
        float halfHeight = height / 2;

        float offsetY = 120f;

        // 🎯 Mặc định: hiện phía trên
        Vector3 finalPos = screenPos + new Vector3(0, offsetY, 0);

        // 🔥 Nếu không đủ chỗ phía trên → chuyển xuống dưới
        if (finalPos.y + halfHeight > Screen.height)
        {
            finalPos = screenPos - new Vector3(0, offsetY, 0);
        }

        // 🔥 Nếu tràn trái → đẩy vào trong
        if (finalPos.x - halfWidth < 0)
        {
            finalPos.x = halfWidth;
        }

        // 🔥 Nếu tràn phải → đẩy vào trong
        if (finalPos.x + halfWidth > Screen.width)
        {
            finalPos.x = Screen.width - halfWidth;
        }

        // 🔥 Nếu tràn dưới → đẩy lên
        if (finalPos.y - halfHeight < 0)
        {
            finalPos.y = halfHeight;
        }

        buildUI.transform.position = finalPos;
    }
}
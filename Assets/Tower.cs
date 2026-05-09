using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class Tower : MonoBehaviour
{
    [Header("Info")]
    public string towerName;

    [Header("Level Settings")]
    public int currentLevel = 0;

    public List<float> damagePerLevel = new List<float>();
    public List<float> fireRatePerLevel = new List<float>(); // Tốc độ bắn theo level
    public List<float> rangePerLevel = new List<float>(); // Tầm bắn theo level
    public List<int> upgradeCost = new List<int>();
    public List<GameObject> levelModels = new List<GameObject>();
    [Header("References")]
    public Transform modelHolder;
    // Event để notify TowerAttack, TowerTargeting khi upgrade
    public System.Action onLevelChanged;

    public List<int> sellValuePerLevel = new List<int>();

    public BuildSlot ownerSlot;
    public Outline outline;
    public GameObject rangeCircle;

    public enum TowerType
    {
        Archer,
        Canon
    }

    public TowerType towerType;

    void Start()
    {
        ApplyLevel();
    }
    // QUAN TRỌNG: Đảm bảo ApplyLevel() được gọi sau khi tất cả component khác đã Start() để tránh lỗi null reference
    void ApplyLevel()
    {
        // 1. Xóa model cũ
        foreach (Transform child in modelHolder)
        {
            Destroy(child.gameObject);
        }

        // 2. Spawn model mới
        GameObject model = Instantiate(levelModels[currentLevel], modelHolder);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        Outline outlineComp = model.GetComponentInChildren<Outline>();

        if (outlineComp != null)
        {
            outline = outlineComp;
            outline.enabled = false;
        }
        else
        {
            Debug.LogWarning("⚠️ Không tìm thấy Outline trong model!");
        }

        // 3. Lấy TowerAttack
        TowerAttack attack = GetComponent<TowerAttack>();

        if (attack != null)
        {
            // 🔥 Tìm FirePoint đúng cách
            Transform newFirePoint = null;

            foreach (Transform t in model.GetComponentsInChildren<Transform>())
            {
                if (t.name == "FirePoint")
                {
                    newFirePoint = t;
                    break;
                }
            }

            if (newFirePoint != null)
            {
                attack.firePoint = newFirePoint;
            }
            else
            {
                Debug.LogError("❌ Không có FirePoint");
            }

            // 🔥 QUAN TRỌNG NHẤT
            attack.InitAfterUpgrade(); // 👈 thêm dòng này
        }
        // 4. Notify
        onLevelChanged?.Invoke();
        UpdateRangeVisual();
    }
    public float GetDamage() => damagePerLevel[currentLevel];
    public float GetRange() => rangePerLevel[currentLevel];
    public float GetFireRate() => fireRatePerLevel[currentLevel];
    public int GetUpgradeCost()
    {
        if (IsMaxLevel()) return 0;
        return upgradeCost[currentLevel];
    }
    public bool IsMaxLevel() => currentLevel >= levelModels.Count - 1;
    public void Upgrade()
    {
        if (IsMaxLevel())
        {
            Debug.Log("Max level!");
            return;
        }

        int cost = GetUpgradeCost();

        // 🔥 DÙNG CHUNG CoinManager
        if (!CoinManager.Instance.SpendCoin(cost))
        {
            Debug.Log("Không đủ tiền!");
            AudioManager.Instance.PlaySFX("fail");
            return;
        }

        currentLevel++;
        ApplyLevel();
        UpdateRangeVisual();
        AudioManager.Instance.PlaySFX("upgrade");

        Debug.Log($"Upgrade thành Level {currentLevel + 1}");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            Upgrade();

        
    }

    void OnMouseDown()

    {
        BuildManager.Instance.SelectTower(this);

        Debug.Log("Click Tower");
    }


    public int GetSellValue()
    {
        if (sellValuePerLevel.Count == 0) return 0;

        return sellValuePerLevel[currentLevel];
    }

    public void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }

    void UpdateRangeVisual()
    {
        if (rangeCircle == null) return;

        float range = GetRange();
        float diameter = range * 2f;

        rangeCircle.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    public void ShowRange(bool value)
    {
        if (rangeCircle != null)
            rangeCircle.SetActive(value);
    }

    public void ShowRangeTemp(float time)
    {
        StartCoroutine(ShowRangeCoroutine(time));
    }

    IEnumerator ShowRangeCoroutine(float time)
    {
        if (rangeCircle == null) yield break;

        rangeCircle.SetActive(true);

        yield return new WaitForSeconds(time);

        // ❗ không tắt nếu đang select
        if (!BuildManager.Instance.IsUpgradeUIOpen())
        {
            rangeCircle.SetActive(false);
        }
    }

    
}
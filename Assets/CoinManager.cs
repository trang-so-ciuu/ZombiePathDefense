using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("Coin Settings")]
    public int currentCoin = 100;

    public event Action OnCoinChanged;

    void Awake()
    {
        Instance = this;
    }

    public void AddCoin(int amount)
    {
        currentCoin += amount;
        OnCoinChanged?.Invoke();
    }

    public bool SpendCoin(int amount)
    {
        if (currentCoin >= amount)
        {
            currentCoin -= amount;
            OnCoinChanged?.Invoke();
            return true;
        }

        return false;
    }
}
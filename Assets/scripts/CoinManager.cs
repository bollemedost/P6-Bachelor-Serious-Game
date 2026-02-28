using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int TotalCoins { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        TotalCoins += amount;
    }
}
using UnityEngine;


public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("Debug")]
    public bool debugLogs = true;

    [SerializeField] private int totalCoins;

    // Keep compatibility with your CoinDisplay.cs
    public int TotalCoins => totalCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // IMPORTANT: reset on fresh run
        totalCoins = 0;

        if (debugLogs)
            Debug.Log($"[CoinManager] New run => totalCoins reset to {totalCoins}");
    }

    public static CoinManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("CoinManager");
        return go.AddComponent<CoinManager>();
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;
        if (totalCoins < 0) totalCoins = 0;

        if (debugLogs)
            Debug.Log($"[CoinManager] AddCoin({amount}) => totalCoins={totalCoins}");
    }

    public void SetCoins(int amount)
    {
        totalCoins = Mathf.Max(0, amount);

        if (debugLogs)
            Debug.Log($"[CoinManager] SetCoins({amount})");
    }
}
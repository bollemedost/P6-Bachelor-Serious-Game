using TMPro;
using UnityEngine;


public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Start()
    {
        // Make sure manager exists
        CoinManager.EnsureExists();
    }

    private void Update()
    {
        if (CoinManager.Instance == null)
            return;

        coinText.text = CoinManager.Instance.TotalCoins.ToString();
    }
}
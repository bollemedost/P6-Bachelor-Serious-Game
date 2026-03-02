using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Slider moneySlider;
    [SerializeField] private int maxMoneyForSlider = 100;

    private void Update()
    {
        if (CoinManager.Instance == null) return;

        int coins = CoinManager.Instance.TotalCoins;

        if (coinText != null)
            coinText.text = coins.ToString();

        if (moneySlider != null)
        {
            moneySlider.minValue = 0;
            moneySlider.maxValue = maxMoneyForSlider;
            moneySlider.value = Mathf.Clamp(coins, 0, maxMoneyForSlider);
            moneySlider.interactable = false;
        }
    }
}
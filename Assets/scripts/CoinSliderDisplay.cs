using UnityEngine;
using UnityEngine.UI;

public class CoinSliderDisplay : MonoBehaviour
{
    [SerializeField] private Slider coinSlider;
    [SerializeField] private int maxCoins = 100;

    private void Start()
    {
        CoinManager.EnsureExists();

        if (coinSlider != null)
        {
            coinSlider.maxValue = maxCoins;
            coinSlider.value = CoinManager.Instance.TotalCoins;
        }
    }

    private void Update()
    {
        if (CoinManager.Instance == null || coinSlider == null)
            return;

        coinSlider.value = CoinManager.Instance.TotalCoins;
    }
}
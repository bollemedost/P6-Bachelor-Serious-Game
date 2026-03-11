using UnityEngine;
using UnityEngine.UI;

public class MoneySliderDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider moneySlider;
    [SerializeField] private RectTransform coinIcon;
    [SerializeField] private RectTransform fillRect;

    [Header("Settings")]
    [SerializeField] private int maxCoins = 500;

    private void Start()
    {
        CoinManager.EnsureExists();

        if (moneySlider == null)
            moneySlider = GetComponent<Slider>();

        if (moneySlider != null)
        {
            moneySlider.minValue = 0;
            moneySlider.maxValue = maxCoins;
            moneySlider.wholeNumbers = true;

            int currentCoins = 0;

            if (CoinManager.Instance != null)
                currentCoins = CoinManager.Instance.TotalCoins;

            moneySlider.value = Mathf.Clamp(currentCoins, 0, maxCoins);
        }

        UpdateCoinVisual();
    }

    private void Update()
    {
        if (moneySlider == null || CoinManager.Instance == null)
            return;

        moneySlider.value = Mathf.Clamp(CoinManager.Instance.TotalCoins, 0, maxCoins);
        UpdateCoinVisual();
    }

    private void UpdateCoinVisual()
    {
        if (coinIcon == null || fillRect == null || moneySlider == null || moneySlider.maxValue <= 0)
            return;

        float normalized = moneySlider.value / moneySlider.maxValue;

        float left = -fillRect.rect.width * 0.5f;
        float right = fillRect.rect.width * 0.5f;
        float xPos = Mathf.Lerp(left, right, normalized);

        coinIcon.anchoredPosition = new Vector2(xPos, coinIcon.anchoredPosition.y);
    }
}
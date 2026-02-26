using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Slider moneySlider;

    [Header("Slider Settings")]
    [SerializeField] private int maxMoneyForSlider = 100;

    private int totalCoins = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetupSlider();
        UpdateUI();
    }

    private void SetupSlider()
    {
        if (moneySlider == null) return;

        moneySlider.minValue = 0;
        moneySlider.maxValue = maxMoneyForSlider;
        moneySlider.value = 0;
        moneySlider.interactable = false; // makes it behave like a progress bar
    }

    public void AddCoin(int amount)
    {
        totalCoins += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = totalCoins.ToString();

        if (moneySlider != null)
        {
            moneySlider.value = Mathf.Clamp(totalCoins, 0, maxMoneyForSlider);
        }
    }
}
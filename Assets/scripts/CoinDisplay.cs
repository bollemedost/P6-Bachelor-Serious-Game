using TMPro;
using UnityEngine;

public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Color Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color gainColor = Color.green;
    [SerializeField] private Color lossColor = Color.red;
    [SerializeField] private float colorFlashDuration = 0.5f;

    private int lastCoins;
    private float flashTimer;

    private void Start()
    {
        // Make sure manager exists
        CoinManager.EnsureExists();

        if (CoinManager.Instance != null)
        {
            lastCoins = CoinManager.Instance.TotalCoins;
        }

        if (coinText != null)
        {
            coinText.text = lastCoins.ToString();
            coinText.color = normalColor;
        }
    }

    private void Update()
    {
        if (CoinManager.Instance == null || coinText == null)
            return;

        int currentCoins = CoinManager.Instance.TotalCoins;
        coinText.text = currentCoins.ToString();

        if (currentCoins > lastCoins)
        {
            coinText.color = gainColor;
            flashTimer = colorFlashDuration;
        }
        else if (currentCoins < lastCoins)
        {
            coinText.color = lossColor;
            flashTimer = colorFlashDuration;
        }

        lastCoins = currentCoins;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
            {
                coinText.color = normalColor;
            }
        }
    }
}
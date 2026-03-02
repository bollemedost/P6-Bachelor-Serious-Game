using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int TotalCoins { get; private set; }

    [Header("UI Settings")]
    public CanvasGroup moneyCanvasGroup; // assign the MoneyCanvas root here
    public TextMeshProUGUI coinText;
    public float fadeDuration = 0.5f;

    private bool uiShown = false;

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

        if (moneyCanvasGroup != null)
            moneyCanvasGroup.alpha = 0f; // start hidden
    }

    public void AddCoin(int amount)
    {
        TotalCoins += amount;
        UpdateUI();

        if (!uiShown && TotalCoins > 0)
        {
            uiShown = true;
            if (moneyCanvasGroup != null)
                StartCoroutine(FadeInCanvas(moneyCanvasGroup));
        }
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = TotalCoins.ToString();
    }

    private System.Collections.IEnumerator FadeInCanvas(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
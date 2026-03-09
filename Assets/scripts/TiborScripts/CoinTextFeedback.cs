using UnityEngine;
using TMPro;
using System.Collections;

public class CoinTextFeedback : MonoBehaviour
{
    public static CoinTextFeedback Instance;

    [Header("Reference")]
    public TMP_Text coinText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color gainColor = Color.green;
    public Color lossColor = Color.red;

    public float flashDuration = 1f;

    Coroutine flashRoutine;

    void Awake()
    {
        Instance = this;
    }

    public void FlashForChange(int amount)
    {
        if (coinText == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (amount > 0)
            flashRoutine = StartCoroutine(Flash(gainColor));
        else if (amount < 0)
            flashRoutine = StartCoroutine(Flash(lossColor));
    }

    IEnumerator Flash(Color c)
    {
        coinText.color = c;

        yield return new WaitForSeconds(flashDuration);

        coinText.color = normalColor;
    }
}
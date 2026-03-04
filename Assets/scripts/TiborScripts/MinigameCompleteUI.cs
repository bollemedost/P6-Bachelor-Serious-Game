using TMPro;
using UnityEngine;

public class MinigameCompleteUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public TextMeshProUGUI messageText;

    [Header("Reward")]
    public int rewardCoins = 20;
    public string messageTemplate = "Du fuldførte minispillet. Du får nu tildelt {0} mønter";

    [Header("Event")]
    public GameEvent miniGameEvent; // assign the same event as on the cube in main scene

    private bool shown = false;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(int coins)
    {
        rewardCoins = coins;
        shown = true;

        if (messageText != null)
            messageText.text = string.Format(messageTemplate, rewardCoins);

        if (root != null) root.SetActive(true);
    }

    // Hook up to "Tilbage/Færdig" button
    public void OnDoneClicked()
    {
        if (!shown) return;

        // Award coins (runtime only, if you're using the non-PlayerPrefs CoinManager)
        CoinManager.EnsureExists().AddCoin(rewardCoins);

        // Complete story progression event
        var em = FindObjectOfType<EventManager>();
        if (em != null && miniGameEvent != null)
        {
            em.CompleteEvent(miniGameEvent);
        }

        // Return to previous scene + restore position
        ReturnToPreviousSceneT.ReturnNow();
    }
}
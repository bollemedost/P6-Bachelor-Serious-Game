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
    public GameEvent miniGameEvent;

    [Header("SubwayT Special Return")]
    public bool useSubwayTReturnScene = true;

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

    public void OnDoneClicked()
    {
        if (!shown) return;

        CoinManager.EnsureExists().AddCoin(rewardCoins);

        var em = FindObjectOfType<EventManager>();
        if (em != null && miniGameEvent != null)
        {
            em.CompleteEvent(miniGameEvent);
        }

        // SubwayT should go to Scene13Home1915NOINTERACTION
        if (useSubwayTReturnScene)
        {
            SubwayTReturnToScene13T.ReturnNow();
            return;
        }

        // Other minigames can still use old return
        ReturnToPreviousSceneT.ReturnNow();
    }
}
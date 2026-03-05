using UnityEngine;
using UnityEngine.SceneManagement;

public class UlkasseMinigameFinishT : MonoBehaviour
{
    [Header("Finish Condition")]
    [Tooltip("Drag ALL slots from the minigame here (e.g., 13 slots).")]
    public slot[] allSlots;

    [Header("Next Scene")]
    public string nextSceneName = "Scene13Home1915";

    [Header("Coins + Event (like MemoryGame)")]
    public int rewardCoins = 20;
    public GameEvent miniGameEvent; // MiniGame3

    [Header("Debug")]
    public bool debugLogs = true;

    private bool finished = false;

    private void Update()
    {
        if (finished) return;
        if (allSlots == null || allSlots.Length == 0) return;

        if (AreAllSlotsCompleted())
        {
            FinishMinigame();
        }
    }

    private bool AreAllSlotsCompleted()
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            slot s = allSlots[i];
            if (s == null) continue;

            // If acceptAnyItem: must simply be filled (has a child)
            if (s.acceptAnyItem)
            {
                if (s.transform.childCount == 0)
                    return false;

                continue;
            }

            // Normal slots: must be correct
            if (!s.IsCorrectPlaced)
                return false;
        }

        return true;
    }

    private void FinishMinigame()
    {
        finished = true;

        // Coins
        CoinManager.EnsureExists().AddCoin(rewardCoins);

        // Event progression
        var em = FindObjectOfType<EventManager>();
        if (em != null && miniGameEvent != null)
        {
            em.CompleteEvent(miniGameEvent);
        }

        if (debugLogs)
            Debug.Log($"[UlkasseMinigameFinishT] Completed ALL slots! +{rewardCoins} coins, Event={(miniGameEvent ? miniGameEvent.name : "null")} -> Loading {nextSceneName}");

        // Load next scene
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.FadeToScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}
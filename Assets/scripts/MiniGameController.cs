using UnityEngine;

public class MinigameController : MonoBehaviour
{
    public GameEvent minigameEvent; // assign the same event as in MiniGameInteraction

    // Call this when the minigame is finished
    public void FinishMinigame()
    {
        // Complete the event
        EventManager eventManager = Object.FindFirstObjectByType<EventManager>();
        if (eventManager != null && minigameEvent != null)
            eventManager.CompleteEvent(minigameEvent);

        // Return to previous scene
        if (SceneReturnManager.Instance != null)
            SceneReturnManager.Instance.ReturnToPreviousScene();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameInteraction : Interactable
{
    [Header("Event Settings")]
    public GameEvent miniGameEvent;          // Event triggered when minigame is completed
    public GameEvent[] prerequisiteEvents;
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

    [Header("Minigame Scene")]
    public string miniGameSceneName;

    private bool isUnlocked = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();

        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactDistance)
            UpdateCanvasState();
        else
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }
    }

    private void UpdateCanvasState()
    {
        if (eventManager == null) return;

        isUnlocked = true;

        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

        if (isUnlocked)
        {
            if (interactCanvas != null) interactCanvas.SetActive(true);
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
        }
        else
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(true);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (!isUnlocked) return;

        // Save player position before leaving
        if (SceneReturnManager.Instance != null && player != null)
        {
            SceneReturnManager.Instance.SavePlayerState(player);
        }

        // Load minigame
        SceneManager.LoadScene(miniGameSceneName);
    }

    // Call this from your minigame when it ends
    public void MinigameFinished()
    {
        // Complete the event
        if (eventManager != null && miniGameEvent != null)
            eventManager.CompleteEvent(miniGameEvent);

        // Return to previous scene with player in the same position
        if (SceneReturnManager.Instance != null)
            SceneReturnManager.Instance.ReturnToPreviousScene();
    }
}
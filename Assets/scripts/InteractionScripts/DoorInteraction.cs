using UnityEngine;

public class DoorInteraction : Interactable
{
    [Header("Event Settings")]
    public GameEvent doorEvent;                 // Event triggered when door is used
    public GameEvent[] prerequisiteEvents;      // Events that must be completed before door unlocks
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;             // Shown when door locked
    public GameObject interactCanvas;           // Shown when player can interact

    [Header("Scene Transition")]
    public string sceneToLoad;                  // Scene to load

    private bool isUnlocked = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();
        if (eventManager == null)
            Debug.LogError("No EventManager found in scene!");

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

        // Check all prerequisite events
        isUnlocked = true;
        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

        // Update UI
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
        if (!isUnlocked || eventManager == null) return;

        // Fire the door event
        if (doorEvent != null)
            eventManager.CompleteEvent(doorEvent);

        // Trigger scene transition
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.FadeToScene(sceneToLoad);
            else
            {
                Debug.LogWarning("SceneTransition instance not found, loading scene directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
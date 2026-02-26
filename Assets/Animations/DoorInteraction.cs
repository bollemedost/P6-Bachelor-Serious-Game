using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteraction : Interactable
{
    [Header("Event Settings")]
    public GameEvent doorEvent;                     // The event fired when door is used
    public GameEvent[] prerequisiteEvents;         // Events that must be completed before door unlocks
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

    [Header("Scene Transition")]
    public string sceneToLoad;   // Name of the scene you want to load

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
        {
            UpdateCanvasState();
        }
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
        if (!isUnlocked) return;
        if (eventManager == null) return;

        Debug.Log("DoorInteract: Loading new scene...");

        // Fire the door event
        if (doorEvent != null)
        {
            eventManager.CompleteEvent(doorEvent);
        }

        // Load the scene using SceneTransition if available
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.FadeToScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("SceneTransition instance not found!");
            }
        }
    }
}
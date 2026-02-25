using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteraction : Interactable
{
    [Header("Event Settings")]
    public string eventID = "DoorOpened";
    public string[] prerequisiteEvents = { "MomTalk" };
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
        if (eventManager == null) return;

        Debug.Log("DoorInteract: Loading new scene...");

        eventManager.CompleteEvent(eventID);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (!string.IsNullOrEmpty(sceneToLoad) && SceneTransition.Instance != null)
            {
                SceneTransition.Instance.FadeToScene(sceneToLoad);
            }
        else
        {
            Debug.LogWarning("Scene name not set in DoorInteraction!");
        }
    }
}
}
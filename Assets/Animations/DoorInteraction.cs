using UnityEngine;

public class DoorInteraction : Interactable
{
    [Header("Event Settings")]
    public string eventID = "DoorOpened";
    public string[] prerequisiteEvents = { "MomTalk" };
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;   // shows "Locked"
    public GameObject interactCanvas; // shows "Press E"

    [Header("Animator Settings")]
    public Animator doorAnimator;     // assign your Door's Animator
    public string openTrigger = "Open"; // trigger in Animator for opening

    private bool isUnlocked = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();
        if (eventManager == null)
            Debug.LogError("No EventManager found in scene!");

        // Hide both canvases at start
        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Only show UI if player is close enough
        if (distance <= interactDistance)
        {
            UpdateCanvasState();
        }
        else
        {
            // Hide both canvases when too far
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }
    }

    private void UpdateCanvasState()
    {
        if (eventManager == null) return;

        // Check if all prerequisites are completed
        isUnlocked = true;
        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

        // Show correct canvas
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
        if (!isUnlocked) return; // do nothing if locked
        if (eventManager == null) return;

        Debug.Log("DoorInteract: Player pressed E");

        // Play the door open animation
        if (doorAnimator != null)
            doorAnimator.SetTrigger(openTrigger);

        // Mark event as completed
        eventManager.CompleteEvent(eventID);
    }
}
using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NPCInteraction : Interactable
{
    [System.Serializable]
    public class NPCEvent
    {
        public GameEvent gameEvent;          // ScriptableObject reference
        public AudioClip dialogueClip;
        public TalkingAnimations talkingSequence;
    }

    [Header("Events Available For This NPC")]
    public NPCEvent[] npcEvents;

    [Header("Event To Trigger Right Now")]
    public GameEvent currentEvent;

    private EventManager eventManager;
    private AudioSource audioSource;
    private bool isInteracting = false;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera npcCam;

    [Header("Player Settings")]
    public Transform playerTransform;
    public Transform playerInteractionPoint;

    private NPCEvent currentNPCEvent;

    // Track if this NPC has already been interacted with
    private bool interactionCompleted = false;

    protected override void Start()
    {
        base.Start();
        eventManager = FindFirstObjectByType<EventManager>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (!isInteracting && !interactionCompleted)
            StartInteraction();
    }

    private void StartInteraction()
    {
        currentNPCEvent = GetEvent(currentEvent);
        if (currentNPCEvent == null)
        {
            Debug.LogWarning($"No NPCEvent found for {currentEvent?.name}");
            return;
        }

        isInteracting = true;

        // Lock player movement and rotation
        MovementStateManager.canMove = false;
        MovementStateManager.canRotate = false;

        // Snap player to interaction point
        if (playerInteractionPoint != null && playerTransform != null)
            playerTransform.position = playerInteractionPoint.position;

        // Switch cameras
        if (playerCam != null && npcCam != null)
        {
            npcCam.Priority = 10;
            playerCam.Priority = 0;
        }

        // Hide interaction canvas immediately
        if (canvas != null)
            canvas.SetActive(false);

        // Play animation
        if (currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.PlaySequence();

        // Play audio
        if (currentNPCEvent.dialogueClip != null)
        {
            audioSource.clip = currentNPCEvent.dialogueClip;
            audioSource.Play();
        }

        // Fire event
        if (eventManager != null)
            eventManager.CompleteEvent(currentNPCEvent.gameEvent);

        // Start coroutine to monitor when interaction finishes
        StartCoroutine(WaitForInteractionEnd());
    }

    private IEnumerator WaitForInteractionEnd()
    {
        while (isInteracting)
        {
            // Keep player facing NPC
            FaceEachOther();

            // Check if both audio and animation finished
            bool audioFinished = audioSource == null || !audioSource.isPlaying;
            bool animationFinished = currentNPCEvent.talkingSequence == null || !currentNPCEvent.talkingSequence.IsPlaying;

            if (audioFinished && animationFinished)
                break;

            yield return null;
        }

        EndInteraction();
    }

    private void FaceEachOther()
    {
        if (playerTransform == null) return;

        // Player faces NPC
        Vector3 lookDir = transform.position - playerTransform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

        // NPC faces Player
        Vector3 npcLookDir = playerTransform.position - transform.position;
        npcLookDir.y = 0;
        if (npcLookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(npcLookDir), Time.deltaTime * 5f);
    }

    private void EndInteraction()
    {
        isInteracting = false;
        interactionCompleted = true;

        // Unlock movement and rotation
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        // Restore cameras
        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;
            npcCam.Priority = 0;
        }

        // Stop animation and audio
        if (currentNPCEvent != null)
        {
            if (currentNPCEvent.talkingSequence != null)
                currentNPCEvent.talkingSequence.StopSequence();

            if (audioSource.isPlaying)
                audioSource.Stop();
        }

        // Ensure interaction canvas remains hidden
        if (canvas != null)
            canvas.SetActive(false);
    }

    private NPCEvent GetEvent(GameEvent gameEvent)
    {
        foreach (var evt in npcEvents)
        {
            if (evt.gameEvent == gameEvent)
                return evt;
        }
        return null;
    }

    protected override bool IsCurrentlyInteracting()
    {
        // This tells the base Interactable to hide the canvas
        return isInteracting || interactionCompleted;
    }
}
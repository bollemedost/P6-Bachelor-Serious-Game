using UnityEngine;
using Cinemachine;

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

    protected override void Start()
    {
        base.Start();
        eventManager = FindFirstObjectByType<EventManager>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (!isInteracting)
            StartInteraction();
    }

    private void StartInteraction()
    {
        NPCEvent evt = GetEvent(currentEvent);
        if (evt == null)
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

        // Play animation
        if (evt.talkingSequence != null)
            evt.talkingSequence.PlaySequence();

        // Play audio
        if (evt.dialogueClip != null)
        {
            audioSource.clip = evt.dialogueClip;
            audioSource.Play();
        }

        // Fire event
        if (eventManager != null)
            eventManager.CompleteEvent(evt.gameEvent);
    }

    protected override void Update()
    {
        base.Update();

        if (!isInteracting) return;

        // Smooth rotation every frame
        FaceEachOther();

        // Exit on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndInteraction();
        }
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

        // Unlock movement and rotation
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        // Restore cameras
        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;
            npcCam.Priority = 0;
        }

        NPCEvent evt = GetEvent(currentEvent);
        if (evt != null)
        {
            // Stop animation and audio
            if (evt.talkingSequence != null)
                evt.talkingSequence.StopSequence();

            if (audioSource.isPlaying)
                audioSource.Stop();
        }
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
        return isInteracting;
    }
}
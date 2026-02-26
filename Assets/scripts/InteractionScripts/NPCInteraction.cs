using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NPCInteraction : Interactable
{
    // ===============================
    // TIMESTAMP IMAGE STRUCTURE
    // ===============================

    [System.Serializable]
    public class TimedDialogueImage
    {
        [Tooltip("Time in seconds when this image appears")]
        public float timeStamp;

        [Tooltip("Drag your pre-made dialogue image GameObject here")]
        public GameObject dialogueImageObject;
    }

    [System.Serializable]
    public class NPCEvent
    {
        public GameEvent gameEvent;
        public AudioClip dialogueClip;
        public TalkingAnimations talkingSequence;

        [Header("Dialogue Image Timeline")]
        public TimedDialogueImage[] timedImages;
    }

    // ===============================
    // INSPECTOR FIELDS
    // ===============================

    [Header("Events Available For This NPC")]
    public NPCEvent[] npcEvents;

    [Header("Event To Trigger Right Now")]
    public GameEvent currentEvent;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera npcCam;

    [Header("Player Settings")]
    public Transform playerTransform;
    public Transform playerInteractionPoint;

    [Header("Dialogue Canvas")]
    public GameObject dialogueCanvas;

    [Header("Player UI Canvas Group (for fade)")]
    public CanvasGroup playerUICanvasGroup;

    [Header("UI Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    // ===============================
    // PRIVATE VARIABLES
    // ===============================

    private EventManager eventManager;
    private AudioSource audioSource;
    private NPCEvent currentNPCEvent;

    private bool isInteracting = false;
    private bool interactionCompleted = false;

    // ===============================
    // UNITY METHODS
    // ===============================

    protected override void Start()
    {
        base.Start();

        eventManager = FindFirstObjectByType<EventManager>();
        audioSource = GetComponent<AudioSource>();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    public override void Interact()
    {
        if (!isInteracting && !interactionCompleted)
            StartInteraction();
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isInteracting || interactionCompleted;
    }

    // ===============================
    // INTERACTION LOGIC
    // ===============================

    private void StartInteraction()
    {
        currentNPCEvent = GetEvent(currentEvent);

        if (currentNPCEvent == null)
        {
            Debug.LogWarning($"No NPCEvent found for {currentEvent?.name}");
            return;
        }

        isInteracting = true;

        // Hide player UI instantly
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        // Lock player
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

        // Hide interaction canvas
        if (canvas != null)
            canvas.SetActive(false);

        // Show dialogue canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

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
        if (eventManager != null && currentNPCEvent.gameEvent != null)
            eventManager.CompleteEvent(currentNPCEvent.gameEvent);

        StartCoroutine(HandleDialogueTimeline());
    }

    private IEnumerator HandleDialogueTimeline()
    {
        int currentIndex = 0;

        // Disable all images at start
        if (currentNPCEvent.timedImages != null)
        {
            foreach (var entry in currentNPCEvent.timedImages)
            {
                if (entry.dialogueImageObject != null)
                    entry.dialogueImageObject.SetActive(false);
            }
        }

        while (isInteracting)
        {
            FaceEachOther();

            if (currentNPCEvent.timedImages != null &&
                currentIndex < currentNPCEvent.timedImages.Length &&
                audioSource != null)
            {
                float currentTime = audioSource.time;

                if (currentTime >= currentNPCEvent.timedImages[currentIndex].timeStamp)
                {
                    // Hide previous image
                    if (currentIndex > 0)
                    {
                        var previous = currentNPCEvent.timedImages[currentIndex - 1];
                        if (previous.dialogueImageObject != null)
                            previous.dialogueImageObject.SetActive(false);
                    }

                    // Show current image
                    var current = currentNPCEvent.timedImages[currentIndex];
                    if (current.dialogueImageObject != null)
                        current.dialogueImageObject.SetActive(true);

                    currentIndex++;
                }
            }

            bool audioFinished = audioSource == null || !audioSource.isPlaying;
            bool animationFinished = currentNPCEvent.talkingSequence == null ||
                                     !currentNPCEvent.talkingSequence.IsPlaying;

            if (audioFinished && animationFinished)
                break;

            yield return null;
        }

        EndInteraction();
    }

    private void EndInteraction()
    {
        isInteracting = false;
        interactionCompleted = true;

        // Unlock player
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        // Restore cameras
        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;
            npcCam.Priority = 0;
        }

        // Stop animation
        if (currentNPCEvent != null && currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.StopSequence();

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Hide dialogue canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // Disable all images
        if (currentNPCEvent != null && currentNPCEvent.timedImages != null)
        {
            foreach (var entry in currentNPCEvent.timedImages)
            {
                if (entry.dialogueImageObject != null)
                    entry.dialogueImageObject.SetActive(false);
            }
        }

        // Fade player UI back in
        if (playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        // Ensure interaction prompt stays hidden
        if (canvas != null)
            canvas.SetActive(false);
    }

    private void FaceEachOther()
    {
        if (playerTransform == null) return;

        // Player faces NPC
        Vector3 lookDir = transform.position - playerTransform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f);

        // NPC faces Player
        Vector3 npcLookDir = playerTransform.position - transform.position;
        npcLookDir.y = 0;
        if (npcLookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(npcLookDir),
                Time.deltaTime * 5f);
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

    private IEnumerator FadeInUI(CanvasGroup cg, float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float startAlpha = cg.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }
}
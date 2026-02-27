using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MinigameNPCInteraction : Interactable
{
    // ===============================
    // TIMED DIALOGUE IMAGES
    // ===============================

    [System.Serializable]
    public class TimedDialogueImage
    {
        public float timeStamp;
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

        [Header("Signature")]
        public bool givesSignature; // Does this NPC give a signature?
    }

    // ===============================
    // INSPECTOR FIELDS
    // ===============================

    [Header("NPC Events")]
    public NPCEvent[] npcEvents;

    [Header("Current Event")]
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

    [Header("Signature UI Manager")]
    public MinigameUIManager uiManager; // Handles signature counter

    // ===============================
    // PRIVATE VARIABLES
    // ===============================

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

        // Fade out player UI
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        // Lock player movement
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

        // Hide default interact canvas
        if (canvas != null)
            canvas.SetActive(false);

        // Show dialogue canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        // Play talking animation
        currentNPCEvent.talkingSequence?.PlaySequence();

        // Play audio clip
        if (currentNPCEvent.dialogueClip != null)
        {
            audioSource.clip = currentNPCEvent.dialogueClip;
            audioSource.Play();
        }

        // Fire event
        EventManager evtManager = FindObjectOfType<EventManager>();
        if (evtManager != null && currentNPCEvent.gameEvent != null)
            evtManager.CompleteEvent(currentNPCEvent.gameEvent);

        StartCoroutine(HandleDialogueTimeline());
    }

    private IEnumerator HandleDialogueTimeline()
    {
        int currentIndex = 0;

        // Disable all images at start
        if (currentNPCEvent.timedImages != null)
        {
            foreach (var entry in currentNPCEvent.timedImages)
                if (entry.dialogueImageObject != null)
                    entry.dialogueImageObject.SetActive(false);
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
                    if (currentIndex > 0)
                    {
                        var previous = currentNPCEvent.timedImages[currentIndex - 1];
                        if (previous.dialogueImageObject != null)
                            previous.dialogueImageObject.SetActive(false);
                    }

                    var current = currentNPCEvent.timedImages[currentIndex];
                    if (current.dialogueImageObject != null)
                        current.dialogueImageObject.SetActive(true);

                    currentIndex++;
                }
            }

            bool audioFinished = audioSource == null || !audioSource.isPlaying;
            bool animationFinished = currentNPCEvent.talkingSequence == null || !currentNPCEvent.talkingSequence.IsPlaying;

            if (audioFinished && animationFinished)
                break;

            yield return null;
        }

        // Award signature if applicable
        if (currentNPCEvent.givesSignature && uiManager != null)
            uiManager.AddSignature();

        EndInteraction();
    }

    private void EndInteraction()
    {
        isInteracting = false;
        interactionCompleted = true;

        // Stop animation
        currentNPCEvent.talkingSequence?.StopSequence();

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Hide dialogue canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // Fade player UI back in
        if (playerUICanvasGroup != null)
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));

        // Unlock player movement
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        // Switch back to player cam
        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;  // Player cam takes priority
            npcCam.Priority = 0;      // NPC cam lowers priority
        }

        // Ensure interaction prompt stays hidden
        if (canvas != null)
            canvas.SetActive(false);
    }

    // ===============================
    // HELPER METHODS
    // ===============================

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

    private NPCEvent GetEvent(GameEvent gameEvent)
    {
        foreach (var evt in npcEvents)
            if (evt.gameEvent == gameEvent)
                return evt;
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
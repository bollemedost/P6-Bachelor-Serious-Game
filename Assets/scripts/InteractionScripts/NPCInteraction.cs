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
    // WALK AFTER INTERACTION
    // ===============================

    [Header("Walk After Interaction")]
    public bool walkAfterInteraction = false;

    [Tooltip("Delay before NPC starts walking")]
    public float walkStartDelay = 1f;

    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waypointStopDistance = 0.2f;
    public float playerFollowDistance = 5f;
    public Animator animator;
    public string walkBoolName = "isWalking";

    // ===============================
    // PRIVATE VARIABLES
    // ===============================

    private EventManager eventManager;
    private AudioSource audioSource;
    private NPCEvent currentNPCEvent;

    private bool isInteracting = false;
    private bool interactionCompleted = false;
    private bool isWalking = false;

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
        return isInteracting || interactionCompleted || isWalking;
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

        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        MovementStateManager.canMove = false;
        MovementStateManager.canRotate = false;

        if (playerInteractionPoint != null && playerTransform != null)
            playerTransform.position = playerInteractionPoint.position;

        if (playerCam != null && npcCam != null)
        {
            npcCam.Priority = 10;
            playerCam.Priority = 0;
        }

        if (canvas != null)
            canvas.SetActive(false);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.PlaySequence();

        if (currentNPCEvent.dialogueClip != null)
        {
            audioSource.clip = currentNPCEvent.dialogueClip;
            audioSource.Play();
        }

        if (eventManager != null && currentNPCEvent.gameEvent != null)
            eventManager.CompleteEvent(currentNPCEvent.gameEvent);

        StartCoroutine(HandleDialogueTimeline());
    }

    private IEnumerator HandleDialogueTimeline()
    {
        int currentIndex = 0;

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

        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;
            npcCam.Priority = 0;
        }

        if (currentNPCEvent != null && currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.StopSequence();

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (currentNPCEvent != null && currentNPCEvent.timedImages != null)
        {
            foreach (var entry in currentNPCEvent.timedImages)
            {
                if (entry.dialogueImageObject != null)
                    entry.dialogueImageObject.SetActive(false);
            }
        }

        if (playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        if (canvas != null)
            canvas.SetActive(false);

        // ✅ START WALK WITH DELAY
        if (walkAfterInteraction && waypoints != null && waypoints.Length > 0)
        {
            StartCoroutine(StartWalkWithDelay());
        }
    }

    // ===============================
    // WALKING LOGIC
    // ===============================

    private IEnumerator StartWalkWithDelay()
    {
        yield return new WaitForSeconds(walkStartDelay);
        StartCoroutine(WalkRoutine());
    }

    private IEnumerator WalkRoutine()
    {
        isWalking = true;

        if (animator != null)
            animator.SetBool(walkBoolName, true);

        foreach (Transform target in waypoints)
        {
            while (Vector3.Distance(transform.position, target.position) > waypointStopDistance)
            {
                if (playerTransform != null)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                    if (distanceToPlayer > playerFollowDistance)
                    {
                        if (animator != null)
                            animator.SetBool(walkBoolName, false);

                        yield return null;
                        continue;
                    }
                }

                if (animator != null)
                    animator.SetBool(walkBoolName, true);

                Vector3 direction = (target.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(direction),
                        Time.deltaTime * 5f);
                }

                yield return null;
            }
        }

        if (animator != null)
            animator.SetBool(walkBoolName, false);

        isWalking = false;

        Debug.Log("NPC finished walking.");
    }

    // ===============================
    // HELPER METHODS
    // ===============================

    private void FaceEachOther()
    {
        if (playerTransform == null) return;

        Vector3 lookDir = transform.position - playerTransform.position;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f);
        }

        Vector3 npcLookDir = playerTransform.position - transform.position;
        npcLookDir.y = 0;

        if (npcLookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(npcLookDir),
                Time.deltaTime * 5f);
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
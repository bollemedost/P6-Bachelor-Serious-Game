using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class NPCInteraction : Interactable
{
    [System.Serializable]
    public class TimedDialogueImage
    {
        public float timeStamp;
        public GameObject dialogueImageObject;
    }

    [System.Serializable]
    public class NPCEvent
    {
        public GameEvent gameEvent;             // Triggered at start
        public GameEvent finishedEvent;         // Optional, triggered after dialogue finishes
        public AudioClip dialogueClip;
        public TalkingAnimations talkingSequence;

        [Header("Dialogue Image Timeline")]
        public TimedDialogueImage[] timedImages;
    }

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

    [Header("Player UI Canvas Group")]
    public CanvasGroup playerUICanvasGroup;

    [Header("UI Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    [Header("Walk After Interaction")]
    public bool walkAfterInteraction = false;
    public float walkStartDelay = 1f;
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waypointStopDistance = 0.2f;
    public float playerFollowDistance = 5f;
    public Animator animator;
    public string walkBoolName = "isWalking";

    [Header("Scene Transition (Optional)")]
    public bool loadSceneAfterInteraction = false;
    public string sceneToLoad;
    public float sceneLoadDelay = 0.5f;

    [Header("Voting Requirement (Optional)")]
    public bool checkPlayerCoins = false;
    public int requiredCoins = 50;

    public GameEvent enoughCoinsEvent;
    public GameEvent notEnoughCoinsEvent;

    private EventManager eventManager;
    private AudioSource audioSource;
    private NPCEvent currentNPCEvent;

    private bool isInteracting = false;
    private bool interactionCompleted = false;
    private bool isWalking = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();
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

    private void StartInteraction()
    {
        // ================= COIN BRANCH =================
        if (checkPlayerCoins && CoinManager.Instance != null)
        {
            if (CoinManager.Instance.TotalCoins >= requiredCoins)
            {
                currentEvent = enoughCoinsEvent;
            }
            else
            {
                currentEvent = notEnoughCoinsEvent;
            }
        }

        currentNPCEvent = GetEvent(currentEvent);
        if (currentNPCEvent == null)
        {
            Debug.LogWarning($"No NPCEvent found for {currentEvent?.name}");
            return;
        }

        // IMPORTANT:
        // Only lock AFTER we know the interaction can actually start.
        LockInteraction();

        isInteracting = true;

        // Disable player movement
        MovementStateManager.canMove = false;
        MovementStateManager.canRotate = false;

        // Move player to interaction point
        if (playerInteractionPoint != null && playerTransform != null)
            playerTransform.position = playerInteractionPoint.position;

        // Camera switch
        if (playerCam != null && npcCam != null)
        {
            npcCam.Priority = 10;
            playerCam.Priority = 0;
        }

        // Hide player UI immediately
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        // Hide regular canvas
        if (canvas != null)
            canvas.SetActive(false);

        // Show dialogue canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        // Play talking animations
        if (currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.PlaySequence();

        // Play dialogue audio
        if (currentNPCEvent.dialogueClip != null)
        {
            audioSource.clip = currentNPCEvent.dialogueClip;
            audioSource.Play();
        }

        // Trigger start event
        if (eventManager != null && currentNPCEvent.gameEvent != null)
            eventManager.CompleteEvent(currentNPCEvent.gameEvent);

        // Start dialogue timeline
        StartCoroutine(HandleDialogueTimeline());
    }

    private IEnumerator HandleDialogueTimeline()
    {
        int currentIndex = 0;

        // Disable ALL dialogue images from ALL events
        foreach (var evt in npcEvents)
        {
            if (evt.timedImages == null) continue;

            foreach (var entry in evt.timedImages)
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
            bool animationFinished = currentNPCEvent.talkingSequence == null || !currentNPCEvent.talkingSequence.IsPlaying;

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

        // Enable player movement
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        // Camera switch back
        if (playerCam != null && npcCam != null)
        {
            playerCam.Priority = 10;
            npcCam.Priority = 0;
        }

        // Stop animations
        if (currentNPCEvent != null && currentNPCEvent.talkingSequence != null)
            currentNPCEvent.talkingSequence.StopSequence();

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Hide dialogue canvas immediately
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // Hide timed images
        if (currentNPCEvent != null && currentNPCEvent.timedImages != null)
        {
            foreach (var entry in currentNPCEvent.timedImages)
            {
                if (entry.dialogueImageObject != null)
                    entry.dialogueImageObject.SetActive(false);
            }
        }

        // Fade player UI back in ONLY if NOT loading a new scene
        if (!loadSceneAfterInteraction && playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        if (canvas != null)
            canvas.SetActive(false);

        // Trigger finished event if assigned
        if (eventManager != null && currentNPCEvent != null && currentNPCEvent.finishedEvent != null)
            eventManager.CompleteEvent(currentNPCEvent.finishedEvent);

        // Walk after interaction
        if (walkAfterInteraction && waypoints != null && waypoints.Length > 0)
            StartCoroutine(StartWalkWithDelay());

        // Optional scene load after NPC interaction
        if (loadSceneAfterInteraction && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAfterDialogueFinished());
        }

        UnlockInteraction();
    }

    // =============================== WALKING LOGIC ===============================

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

                        Vector3 lookDir = playerTransform.position - transform.position;
                        lookDir.y = 0;
                        if (lookDir != Vector3.zero)
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

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
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

                yield return null;
            }
        }

        if (animator != null)
            animator.SetBool(walkBoolName, false);

        isWalking = false;
    }

    // =============================== HELPER METHODS ===============================

    private void FaceEachOther()
    {
        if (playerTransform == null) return;

        Vector3 lookDir = transform.position - playerTransform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

        Vector3 npcLookDir = playerTransform.position - transform.position;
        npcLookDir.y = 0;
        if (npcLookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(npcLookDir), Time.deltaTime * 5f);
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
        if (cg == null) yield break;

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
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator LoadSceneAfterDialogueFinished()
    {
        // Wait until audio is completely finished
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }

        // Wait until talking animation is completely finished
        if (currentNPCEvent != null && currentNPCEvent.talkingSequence != null)
        {
            while (currentNPCEvent.talkingSequence.IsPlaying)
            {
                yield return null;
            }
        }

        // Extra configurable delay
        yield return new WaitForSeconds(sceneLoadDelay);

        // Now load scene safely
        if (SceneFadeIn.instance != null)
        {
            SceneFadeIn.instance.FadeOutAndLoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
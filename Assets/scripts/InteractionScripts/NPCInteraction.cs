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
        public GameEvent gameEvent;
        public GameEvent finishedEvent;
        public AudioClip dialogueClip;
        public TalkingAnimations talkingSequence;

        [Header("Dialogue Image Timeline")]
        public TimedDialogueImage[] timedImages;
    }

    [Header("Events Available For This NPC")]
    public NPCEvent[] npcEvents;

    [Header("Event To Trigger Right Now")]
    public GameEvent currentEvent;

    [Header("Scene Only - Conversations To Stop When This NPC Starts")]
    public NPCConversation[] conversationsToStopOnInteract;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera npcCam;

    [Header("Player Settings")]
    public Transform playerTransform;
    public Transform playerInteractionPoint;

    [Header("Player Movement Reference")]
    public MovementStateManager playerMovement;

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

    [Header("Interaction UI")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

    [Header("Prerequisite Events (Optional)")]
    public GameEvent[] prerequisiteEvents;

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

        if (playerMovement == null && playerTransform != null)
            playerMovement = playerTransform.GetComponent<MovementStateManager>();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
        if (lockedCanvas != null)
            lockedCanvas.SetActive(false);

        if (interactCanvas != null)
            interactCanvas.SetActive(false);
    }

    public override void Interact()
    {
        if (isInteracting || interactionCompleted)
            return;

        if (!ArePrerequisitesMet())
            return;

        StartInteraction();
    }


    private bool ArePrerequisitesMet()
    {
        if (eventManager == null || prerequisiteEvents == null || prerequisiteEvents.Length == 0)
            return true;

        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
                return false;
        }

        return true;
    }

    private void UpdateCanvasState()
    {
        bool unlocked = ArePrerequisitesMet();

        if (unlocked)
        {
            if (interactCanvas != null)
            interactCanvas.SetActive(true);

            if (lockedCanvas != null)
            lockedCanvas.SetActive(false);
        }
        else
        {
            if (lockedCanvas != null)
            lockedCanvas.SetActive(true);

            if (interactCanvas != null)
            interactCanvas.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (player == null || isInteracting)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactDistance)
        {
            UpdateCanvasState();
        }
        else
        {
            if (lockedCanvas != null)
                lockedCanvas.SetActive(false);

            if (interactCanvas != null)
                interactCanvas.SetActive(false);
    }
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isInteracting || interactionCompleted || isWalking;
    }

    private void StartInteraction()
    {
        if (checkPlayerCoins && CoinManager.Instance != null)
        {
            if (CoinManager.Instance.TotalCoins >= requiredCoins)
                currentEvent = enoughCoinsEvent;
            else
                currentEvent = notEnoughCoinsEvent;
        }

        currentNPCEvent = GetEvent(currentEvent);
        if (currentNPCEvent == null)
        {
            Debug.LogWarning($"No NPCEvent found for {currentEvent?.name}");
            return;
        }

        // QUICK SCENE-SPECIFIC FIX:
        // Stop any background conversations assigned in this scene.
        if (conversationsToStopOnInteract != null)
        {
            foreach (var convo in conversationsToStopOnInteract)
            {
                if (convo != null)
                    convo.StopConversation();
            }
        }

        LockInteraction();
        isInteracting = true;

        if (playerMovement != null)
        {
            playerMovement.LockMovement(true);

            if (playerMovement.anim != null)
            {
                playerMovement.anim.SetFloat("hzInput", 0f);
                playerMovement.anim.SetFloat("vInput", 0f);
                playerMovement.anim.SetBool("Walking", false);
                playerMovement.anim.SetBool("isTalking", true);
            }
        }
        else
        {
            MovementStateManager.canMove = false;
            MovementStateManager.canRotate = false;
        }

        if (playerInteractionPoint != null && playerTransform != null)
            playerTransform.position = playerInteractionPoint.position;

        if (playerCam != null && npcCam != null)
        {
            npcCam.Priority = 10;
            playerCam.Priority = 0;
        }

        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

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

        if (playerMovement != null)
        {
            if (playerMovement.anim != null)
            {
                playerMovement.anim.SetBool("isTalking", false);
                playerMovement.anim.SetBool("Walking", false);
                playerMovement.anim.SetFloat("hzInput", 0f);
                playerMovement.anim.SetFloat("vInput", 0f);
            }

            playerMovement.LockMovement(false);
        }
        else
        {
            MovementStateManager.canMove = true;
            MovementStateManager.canRotate = true;
        }

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

        if (!loadSceneAfterInteraction && playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        if (canvas != null)
            canvas.SetActive(false);

        if (eventManager != null && currentNPCEvent != null && currentNPCEvent.finishedEvent != null)
            eventManager.CompleteEvent(currentNPCEvent.finishedEvent);

        if (walkAfterInteraction && waypoints != null && waypoints.Length > 0)
            StartCoroutine(StartWalkWithDelay());

        if (loadSceneAfterInteraction && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAfterDialogueFinished());
        }

        UnlockInteraction();
    }

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
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }

        if (currentNPCEvent != null && currentNPCEvent.talkingSequence != null)
        {
            while (currentNPCEvent.talkingSequence.IsPlaying)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(sceneLoadDelay);

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
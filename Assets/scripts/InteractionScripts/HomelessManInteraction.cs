using UnityEngine;
using System.Collections;

public class HomelessManInteraction : Interactable
{
    [System.Serializable]
    public class TimedDialogueUI
    {
        public float timeStamp;
        public GameObject uiObject;
    }

    [System.Serializable]
    public class HomelessEvent
    {
        public GameEvent startEvent;
        public GameEvent finishedEvent;
    }

    [Header("Event Settings")]
    public GameEvent giveHomelessManMoneyEvent;
    public GameEvent[] prerequisiteEvents;
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

    [Header("Dialogue Canvas")]
    public GameObject dialogueCanvas;
    public TimedDialogueUI[] timedUI;

    [Header("Coin Settings")]
    public int requiredCoins = 5;

    [Header("Donation Limits")]
    public int maxDonations = 3;
    private int donationCount = 0;

    [Header("Interaction Cooldown")]
    public float interactionCooldown = 2f;
    private bool isOnCooldown = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip giveMoneyClip;
    public AudioClip firstTimeDialogueClip;

    [Header("Homeless Man Event")]
    public HomelessEvent homelessEvent;

    private bool hasTriggeredFinishedEvent = false;
    private bool isUnlocked = false;
    private bool hasPlayedDialogue = false;

    // Dialogue timeline
    private float interactionTimer = 0f;
    private int currentUIIndex = 0;
    private bool dialogueRunning = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();

        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactDistance)
            UpdateCanvasState();
        else
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }

        // ======================
        // TIMED DIALOGUE SYSTEM
        // ======================

        if (!dialogueRunning) return;

        interactionTimer += Time.deltaTime;

        if (timedUI != null && currentUIIndex < timedUI.Length)
        {
            if (interactionTimer >= timedUI[currentUIIndex].timeStamp)
            {
                if (currentUIIndex > 0)
                {
                    var previous = timedUI[currentUIIndex - 1];
                    if (previous.uiObject != null)
                        previous.uiObject.SetActive(false);
                }

                var current = timedUI[currentUIIndex];
                if (current.uiObject != null)
                    current.uiObject.SetActive(true);

                currentUIIndex++;
            }
        }
    }

    private void UpdateCanvasState()
    {
        if (eventManager == null) return;

        if (donationCount >= maxDonations)
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
            return;
        }

        isUnlocked = true;

        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

        if (isUnlocked && !isOnCooldown)
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
        // IMPORTANT:
        // Do NOT lock interaction before checking whether this interaction is actually allowed.
        if (!isUnlocked || isOnCooldown || donationCount >= maxDonations)
            return;

        if (CoinManager.Instance == null)
            return;

        int playerCoins = CoinManager.Instance.TotalCoins;

        if (playerCoins < requiredCoins)
        {
            Debug.Log($"Not enough coins! You need {requiredCoins} coins.");
            return;
        }

        // Only lock once we KNOW the interaction is really happening
        LockInteraction();

        // START EVENT
        if (eventManager != null && homelessEvent != null && homelessEvent.startEvent != null)
        {
            eventManager.CompleteEvent(homelessEvent.startEvent);
        }

        // LOCK PLAYER MOVEMENT/ROTATION DURING THIS INTERACTION
        MovementStateManager.canMove = false;
        MovementStateManager.canRotate = false;

        donationCount++;

        CoinManager.Instance.AddCoin(-requiredCoins);

        // Coin sound every time
        if (audioSource != null && giveMoneyClip != null)
            audioSource.PlayOneShot(giveMoneyClip);

        // Dialogue only first time
        if (!hasPlayedDialogue && audioSource != null && firstTimeDialogueClip != null)
        {
            audioSource.PlayOneShot(firstTimeDialogueClip);
            hasPlayedDialogue = true;

            StartDialogueTimeline();
        }

        if (giveHomelessManMoneyEvent != null && eventManager != null)
            eventManager.CompleteEvent(giveHomelessManMoneyEvent);

        Debug.Log($"Donation {donationCount}/{maxDonations}");

        StartCoroutine(InteractionCooldownRoutine());
    }

    private void StartDialogueTimeline()
    {
        interactionTimer = 0f;
        currentUIIndex = 0;
        dialogueRunning = true;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (timedUI != null)
        {
            foreach (var entry in timedUI)
            {
                if (entry.uiObject != null)
                    entry.uiObject.SetActive(false);
            }
        }
    }

   private IEnumerator InteractionCooldownRoutine()
    {
        isOnCooldown = true;

        if (interactCanvas != null)
            interactCanvas.SetActive(false);

        // WAIT FOR AUDIO TO FINISH
        if (audioSource != null)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        // (Optional) small buffer if you want smoother transition
        yield return new WaitForSeconds(0.2f);

        isOnCooldown = false;
        dialogueRunning = false;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (timedUI != null)
        {
            foreach (var entry in timedUI)
            {
                if (entry.uiObject != null)
                    entry.uiObject.SetActive(false);
            }
        }

        // FINISHED EVENT (NOW PERFECTLY TIMED)
        if (!hasTriggeredFinishedEvent && donationCount >= maxDonations)
        {
            hasTriggeredFinishedEvent = true;

            if (eventManager != null && homelessEvent != null && homelessEvent.finishedEvent != null)
            {
                Debug.Log("Homeless man interaction fully completed!");
                eventManager.CompleteEvent(homelessEvent.finishedEvent);
            }
        }

        // UNLOCK PLAYER
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        UnlockInteraction();
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isOnCooldown;
    }
}
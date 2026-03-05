using UnityEngine;
using System.Collections;

public class HomelessManInteraction : Interactable
{
    [Header("Event Settings")]
    public GameEvent giveHomelessManMoneyEvent;
    public GameEvent[] prerequisiteEvents;
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

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

    private bool isUnlocked = false;
    private bool hasPlayedDialogue = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();

        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);
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
    }

    private void UpdateCanvasState()
    {
        if (eventManager == null) return;

        // If max donations reached hide UI permanently
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
        if (!isUnlocked || isOnCooldown || donationCount >= maxDonations)
            return;

        if (CoinManager.Instance == null) return;

        int playerCoins = CoinManager.Instance.TotalCoins;

        if (playerCoins >= requiredCoins)
        {
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
            }

            if (giveHomelessManMoneyEvent != null && eventManager != null)
                eventManager.CompleteEvent(giveHomelessManMoneyEvent);

            Debug.Log($"Donation {donationCount}/{maxDonations}");

            StartCoroutine(InteractionCooldownRoutine());
        }
        else
        {
            Debug.Log($"Not enough coins! You need {requiredCoins} coins.");
        }
    }

    private IEnumerator InteractionCooldownRoutine()
    {
        isOnCooldown = true;

        if (interactCanvas != null)
            interactCanvas.SetActive(false);

        yield return new WaitForSeconds(interactionCooldown);

        isOnCooldown = false;
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isOnCooldown;
    }
}
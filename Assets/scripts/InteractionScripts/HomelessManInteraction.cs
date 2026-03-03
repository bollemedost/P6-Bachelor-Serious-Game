using UnityEngine;

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

    [Header("Audio")]
    public AudioSource audioSource;         
    public AudioClip giveMoneyClip;         // 🪙 plays every time
    public AudioClip firstTimeDialogueClip; // 🗣 plays only first time

    private bool isUnlocked = false;
    private bool hasPlayedDialogue = false; // 👈 Tracks first-time dialogue

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

        isUnlocked = true;
        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

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
        if (!isUnlocked || CoinManager.Instance == null) return;

        int playerCoins = CoinManager.Instance.TotalCoins;

        if (playerCoins >= requiredCoins)
        {
            // Deduct coins
            CoinManager.Instance.AddCoin(-requiredCoins);

            // 🪙 Coin sound plays EVERY time
            if (audioSource != null && giveMoneyClip != null)
            {
                audioSource.PlayOneShot(giveMoneyClip);
            }

            // 🗣 Dialogue plays ONLY first time
            if (!hasPlayedDialogue && audioSource != null && firstTimeDialogueClip != null)
            {
                audioSource.PlayOneShot(firstTimeDialogueClip);
                hasPlayedDialogue = true;
            }

            // Complete the event
            if (giveHomelessManMoneyEvent != null && eventManager != null)
                eventManager.CompleteEvent(giveHomelessManMoneyEvent);

            Debug.Log($"Player gave {requiredCoins} coins to homeless man!");
        }
        else
        {
            Debug.Log($"Not enough coins! You need {requiredCoins} coins.");
        }
    }

    protected override bool IsCurrentlyInteracting()
    {
        return false;
    }
}
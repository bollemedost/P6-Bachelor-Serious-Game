using UnityEngine;

public class HomelessManInteraction : Interactable
{
    [Header("Event Settings")]
    public GameEvent giveHomelessManMoneyEvent; // Event completed when enough coins given
    public GameEvent[] prerequisiteEvents;      // Events that must be completed before interaction unlocks
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;
    public GameObject interactCanvas;

    [Header("Coin Settings")]
    public int requiredCoins = 5; // Amount player must give

    private bool isUnlocked = false;

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

            // Complete the event
            if (giveHomelessManMoneyEvent != null && eventManager != null)
                eventManager.CompleteEvent(giveHomelessManMoneyEvent);

            Debug.Log($"Player gave {requiredCoins} coins to homeless man!");

            // Optional: Play an animation or sound here
        }
        else
        {
            Debug.Log($"Not enough coins! You need {requiredCoins} coins.");
            // Optional: show UI feedback
        }
    }

    protected override bool IsCurrentlyInteracting()
    {
        return false;
    }
}
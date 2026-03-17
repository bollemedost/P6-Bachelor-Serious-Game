using UnityEngine;
using Cinemachine;
using System.Collections;

public class WindowInteraction : Interactable
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera windowCam;

    [Header("Player UI")]
    public CanvasGroup playerUICanvasGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    [Header("Game Event")]
    public GameEvent windowEvent;

    [Header("Prerequisite Events")]
    public GameEvent[] prerequisiteEvents;

    [Header("Exit Delay (Anti-Spam)")]
    public float exitDelayAfterEnter = 0.3f;

    [Header("Re-Enter Cooldown")]
    public float interactionCooldown = 0.5f;

    private bool isActive = false;
    private bool eventTriggered = false;

    private bool canExitInteraction = true;
    private bool canInteractAgain = true;

    private EventManager eventManager;
    private MovementStateManager playerMovement;

    private Coroutine fadeCoroutine;

    protected override void Start()
    {
        base.Start();
        eventManager = FindFirstObjectByType<EventManager>();
        playerMovement = FindFirstObjectByType<MovementStateManager>();
    }

    protected override void Update()
    {
        base.Update();

        // Ensure canvas only shows when prerequisites are met
        if (canvas != null && !CheckIfUnlocked())
        {
            canvas.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (!CheckIfUnlocked() || !canInteractAgain)
            return;

        EnterWindow();
    }

    private bool CheckIfUnlocked()
    {
        if (eventManager == null) return false;

        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
                return false;
        }

        return true;
    }

    protected override void StopInteraction()
    {
        if (!canExitInteraction)
            return;

        ExitWindow();
    }

    private void EnterWindow()
    {
        isActive = true;

        // Prevent instant exit spam
        canExitInteraction = false;
        StartCoroutine(EnableExitAfterDelay());

        if (playerMovement != null)
            playerMovement.LockMovement(true);

        MovementStateManager.canRotate = false;

        windowCam.Priority = 10;
        playerCam.Priority = 0;

        // Hide player UI immediately
        if (playerUICanvasGroup != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            playerUICanvasGroup.alpha = 0f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!eventTriggered && eventManager != null && windowEvent != null)
        {
            eventTriggered = true;
            eventManager.CompleteEvent(windowEvent);
        }
    }

    private void ExitWindow()
    {
        isActive = false;

        playerCam.Priority = 10;
        windowCam.Priority = 0;

        if (playerMovement != null)
            playerMovement.LockMovement(false);

        MovementStateManager.canRotate = true;

        if (playerUICanvasGroup != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        StartCoroutine(RestoreCursorNextFrame());

        // Prevent rapid re-entry
        StartCoroutine(InteractionCooldownCoroutine());
    }

    private IEnumerator EnableExitAfterDelay()
    {
        yield return new WaitForSeconds(exitDelayAfterEnter);
        canExitInteraction = true;
    }

    private IEnumerator InteractionCooldownCoroutine()
    {
        canInteractAgain = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteractAgain = true;
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isActive;
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

    private IEnumerator RestoreCursorNextFrame()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
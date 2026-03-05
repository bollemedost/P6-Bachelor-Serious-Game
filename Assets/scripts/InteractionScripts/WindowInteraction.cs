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

    private bool isActive = false;
    private bool eventTriggered = false;

    private EventManager eventManager;
    private MovementStateManager playerMovement;

    protected override void Start()
    {
        base.Start();

        eventManager = FindFirstObjectByType<EventManager>();
        playerMovement = FindFirstObjectByType<MovementStateManager>();
    }

    public override void Interact()
    {
        EnterWindow();
    }

    protected override void StopInteraction()
    {
        ExitWindow();
    }

    private void EnterWindow()
    {
        isActive = true;

        // Lock player movement
        if (playerMovement != null)
            playerMovement.LockMovement(true);

        MovementStateManager.canRotate = false;

        // Switch camera
        windowCam.Priority = 10;
        playerCam.Priority = 0;

        // Hide player UI
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        // Cursor locked for gameplay view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Trigger event once
        if (!eventTriggered && eventManager != null && windowEvent != null)
        {
            eventTriggered = true;
            eventManager.CompleteEvent(windowEvent);
        }
    }

    private void ExitWindow()
    {
        isActive = false;

        // Switch camera back
        playerCam.Priority = 10;
        windowCam.Priority = 0;

        // Unlock movement
        if (playerMovement != null)
            playerMovement.LockMovement(false);

        MovementStateManager.canRotate = true;

        // Fade UI back in
        if (playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        // Restore gameplay cursor state safely
        StartCoroutine(RestoreCursorNextFrame());
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
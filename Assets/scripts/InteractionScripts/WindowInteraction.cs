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
    public GameEvent windowEvent; // Assign Window1, Window2, etc.

    private bool isActive = false;
    private bool eventTriggered = false;
    private EventManager eventManager;

    protected override void Start()
    {
        base.Start();
        eventManager = FindFirstObjectByType<EventManager>();
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

        windowCam.Priority = 10;
        playerCam.Priority = 0;

        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🔥 Complete event ONCE
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

        if (playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
}
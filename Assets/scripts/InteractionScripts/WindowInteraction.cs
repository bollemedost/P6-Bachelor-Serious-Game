using UnityEngine;
using Cinemachine;
using System.Collections;

public class WindowInteraction : Interactable
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera windowCam;

    [Header("Player UI")]
    public CanvasGroup playerUICanvasGroup; // Assign your UI CanvasGroup here

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f; // time it takes to fade in
    public float fadeDelay = 0.3f; // optional delay before fade in

    private bool isActive = false;

    protected override void Update()
    {
        base.Update(); // Handles E toggle and canvas

        if (!isActive)
            return;

        // Additional logic while interacting
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

        // Hide UI immediately
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f; // invisible

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ExitWindow()
    {
        isActive = false;

        playerCam.Priority = 10;
        windowCam.Priority = 0;

        // Fade UI back in
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
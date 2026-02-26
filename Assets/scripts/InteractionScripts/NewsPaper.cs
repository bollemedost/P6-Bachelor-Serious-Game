using UnityEngine;
using Cinemachine;
using System.Collections;

public class NewsPaper : Interactable
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;      
    public CinemachineVirtualCamera newspaperCam;   

    [Header("Scroll Zoom Settings")]
    public float zoomAmount = 0.2f;
    public float zoomSpeed = 5f;

    [Header("Player UI")]
    public CanvasGroup playerUICanvasGroup; // assign your UI CanvasGroup here

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    private bool isZoomed = false;
    private Transform camTransform;
    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool isScrollingForward = false;

    protected override void Start()
    {
        base.Start();

        if (newspaperCam != null)
        {
            camTransform = newspaperCam.transform;
            originalPos = camTransform.position;
            targetPos = originalPos;
        }
    }

    protected override void Update()
    {
        base.Update(); // Handles E toggle and canvas

        if (!isZoomed || camTransform == null) 
            return;

        // Scroll zoom
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0.01f)
        {
            targetPos += camTransform.forward * scroll * zoomAmount;
            isScrollingForward = true;
        }
        else if (scroll <= 0.0f && isScrollingForward)
        {
            isScrollingForward = false;
        }

        if (!isScrollingForward)
        {
            targetPos = Vector3.Lerp(camTransform.position, originalPos, Time.deltaTime * zoomSpeed);
        }

        camTransform.position = targetPos;
    }

    public override void Interact()
    {
        ZoomIn();
    }

    protected override void StopInteraction()
    {
        ZoomOut();
    }

    private void ZoomIn()
    {
        isZoomed = true;

        newspaperCam.Priority = 10;
        playerCam.Priority = 0;

        // Hide UI immediately
        if (playerUICanvasGroup != null)
            playerUICanvasGroup.alpha = 0f;

        if (canvas != null)
            canvas.SetActive(false);

        if (camTransform != null)
            targetPos = originalPos;

        isScrollingForward = false;
    }

    private void ZoomOut()
    {
        isZoomed = false;

        playerCam.Priority = 10;
        newspaperCam.Priority = 0;

        if (camTransform != null)
            camTransform.position = originalPos;

        // Fade UI back in
        if (playerUICanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));
        }
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isZoomed;
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
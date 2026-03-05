using UnityEngine;
using Cinemachine;
using System.Collections;

public class NewsPaper : Interactable
{
    [System.Serializable]
    public class TimedDialogueUI
    {
        public float timeStamp;
        public GameObject uiObject; // assign text or image UI
    }

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera newspaperCam;

    [Header("Scroll Zoom Settings")]
    public float zoomAmount = 0.2f;
    public float zoomSpeed = 5f;

    [Header("Player UI")]
    public CanvasGroup playerUICanvasGroup;

    [Header("Dialogue Canvas")]
    public GameObject dialogueCanvas;
    public TimedDialogueUI[] timedUI;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    [Header("Interaction Audio")]
    public AudioSource audioSource;
    public AudioClip interactClip;
    public float soundDelay = 0.3f;

    private bool isZoomed = false;
    private bool isAudioPlaying = false;

    private Transform camTransform;
    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool isScrollingForward = false;

    private float interactionTimer = 0f;
    private int currentUIIndex = 0;

    protected override void Start()
    {
        base.Start();

        if (newspaperCam != null)
        {
            camTransform = newspaperCam.transform;
            originalPos = camTransform.position;
            targetPos = originalPos;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (!isZoomed || camTransform == null)
            return;

        // ======================
        // CAMERA SCROLL ZOOM
        // ======================
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

        // ======================
        // TIMED UI SYSTEM
        // ======================
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

    public override void Interact()
    {
        ZoomIn();

        if (audioSource != null && interactClip != null)
        {
            StartCoroutine(PlaySoundWithDelay());
        }

        StartDialogueTimeline();
    }

    private IEnumerator PlaySoundWithDelay()
    {
        isAudioPlaying = true;

        yield return new WaitForSeconds(soundDelay);

        audioSource.PlayOneShot(interactClip);

        yield return new WaitForSeconds(interactClip.length);

        isAudioPlaying = false;
    }

    private void StartDialogueTimeline()
    {
        interactionTimer = 0f;
        currentUIIndex = 0;

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

    protected override void StopInteraction()
    {
        // 🚫 Prevent exit if audio still playing
        if (interactClip != null && isAudioPlaying)
            return;

        ZoomOut();
    }

    private void ZoomIn()
    {
        isZoomed = true;

        // LOCK PLAYER
        MovementStateManager.canMove = false;
        MovementStateManager.canRotate = false;

        newspaperCam.Priority = 10;
        playerCam.Priority = 0;

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

        // UNLOCK PLAYER
        MovementStateManager.canMove = true;
        MovementStateManager.canRotate = true;

        playerCam.Priority = 10;
        newspaperCam.Priority = 0;

        if (camTransform != null)
            camTransform.position = originalPos;

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
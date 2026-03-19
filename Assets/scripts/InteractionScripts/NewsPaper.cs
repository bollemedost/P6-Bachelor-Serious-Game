    using UnityEngine;
    using Cinemachine;
    using System.Collections;

    public class NewsPaper : Interactable
    {
        [System.Serializable]
        public class TimedDialogueUI
        {
            public float timeStamp;
            public GameObject uiObject;
        }

        [Header("Event Settings")]
        public GameEvent newspaperEvent;
        public GameEvent[] prerequisiteEvents;
        private EventManager eventManager;

        [Header("UI Canvases")]
        public GameObject lockedCanvas;
        public GameObject interactCanvas;

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

        [Header("Optional Scene Load Instead Of Exit")]
        public bool loadSceneInsteadOfExit = false;
        public string sceneToLoad;
        public float sceneLoadDelay = 0.3f;

        private bool isUnlocked = false;
        private bool isInteracting = false;

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

            eventManager = Object.FindFirstObjectByType<EventManager>();

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

            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= interactDistance && !isInteracting)
            {
                UpdateCanvasState();
            }
            else
            {
                if (lockedCanvas != null) lockedCanvas.SetActive(false);
                if (interactCanvas != null) interactCanvas.SetActive(false);
            }

            if (!isInteracting || camTransform == null)
                return;

            // ===== SCROLL ZOOM =====
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

            // ===== TIMED UI =====
            if (timedUI != null && currentUIIndex < timedUI.Length)
            {
                interactionTimer += Time.deltaTime;

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
                if (interactCanvas != null)
                    interactCanvas.SetActive(true);

                if (lockedCanvas != null)
                    lockedCanvas.SetActive(false);
            }
            else
            {
                if (lockedCanvas != null)
                    lockedCanvas.SetActive(true);

                if (interactCanvas != null)
                    interactCanvas.SetActive(false);
            }
        }

        public override void Interact()
        {
            if (!isUnlocked || isInteracting)
                return;

            StartCoroutine(HandleNewspaperInteraction());
        }

    private IEnumerator HandleNewspaperInteraction()
        {
            isInteracting = true;

            if (interactCanvas != null)
                interactCanvas.SetActive(false);

            // Lock player
            MovementStateManager.canMove = false;
            MovementStateManager.canRotate = false;

            // Camera switch
            newspaperCam.Priority = 10;
            playerCam.Priority = 0;

            if (playerUICanvasGroup != null)
                playerUICanvasGroup.alpha = 0f;

            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(true);

            interactionTimer = 0f;
            currentUIIndex = 0;

            foreach (var entry in timedUI)
            {
                if (entry.uiObject != null)
                    entry.uiObject.SetActive(false);
            }

            // Optional audio delay
            if (soundDelay > 0f)
                yield return new WaitForSeconds(soundDelay);

            if (audioSource != null && interactClip != null)
            {
                isAudioPlaying = true;
                audioSource.PlayOneShot(interactClip);
                yield return new WaitForSeconds(interactClip.length);
                isAudioPlaying = false;
            }

            // ===============================
            // NEW BRANCH: SCENE LOAD MODE
            // ===============================
           if (loadSceneInsteadOfExit && !string.IsNullOrEmpty(sceneToLoad))
            {
                while (!Input.GetKeyDown(KeyCode.E))
                    yield return null;

                // ✅ Re-enable player BEFORE scene load
                isInteracting = false;
                MovementStateManager.canMove = true;
                MovementStateManager.canRotate = true;

                yield return new WaitForSeconds(sceneLoadDelay);

                if (SceneTransition.Instance != null)
                {
                    SceneTransition.Instance.FadeToScene(sceneToLoad);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
                }

                yield break;
            }

            // ===============================
            // NORMAL EXIT MODE
            // ===============================

            while (!Input.GetKeyDown(KeyCode.E))
                yield return null;

            EndInteraction();
        }

        
        private void EndInteraction()
        {
            isInteracting = false;

            MovementStateManager.canMove = true;
            MovementStateManager.canRotate = true;

            playerCam.Priority = 10;
            newspaperCam.Priority = 0;

            if (camTransform != null)
                camTransform.position = originalPos;

            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(false);

            foreach (var entry in timedUI)
            {
                if (entry.uiObject != null)
                    entry.uiObject.SetActive(false);
            }

            if (playerUICanvasGroup != null)
                StartCoroutine(FadeInUI(playerUICanvasGroup, fadeDelay, fadeDuration));

            if (newspaperEvent != null && eventManager != null)
                eventManager.CompleteEvent(newspaperEvent);
        }

        protected override bool IsCurrentlyInteracting()
        {
            return isInteracting;
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
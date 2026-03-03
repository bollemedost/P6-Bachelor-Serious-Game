    using UnityEngine;
    using TMPro;
    using System.Collections;

    [RequireComponent(typeof(AudioSource))]
    public class ObjectiveController : MonoBehaviour
    {
        [Header("Event References")]
        public GameEvent startObjectiveEvent;   // e.g. TalkedToHomelessMan
        public GameEvent completeObjectiveEvent; // e.g. GiveHomelessManMoney

        [Header("UI")]
        public GameObject objectiveCanvas;
        public CanvasGroup objectiveCanvasGroup; // optional, for fading
        public TextMeshProUGUI objectiveText;
        [TextArea] public string objectiveDescription;

        [Header("Fade Settings")]
        public float fadeDuration = 0.5f;
        public float fadeDelay = 0.1f;

        [Header("Sound")]
        public AudioClip objectiveSound;

        private EventManager eventManager;
        private AudioSource audioSource;
        private bool objectiveShown = false;

        private void Start()
        {
            eventManager = Object.FindFirstObjectByType<EventManager>();
            audioSource = GetComponent<AudioSource>();

            if (objectiveCanvas != null)
                objectiveCanvas.SetActive(false);

            if (objectiveCanvasGroup != null)
                objectiveCanvasGroup.alpha = 0f;

            if (objectiveText != null)
                objectiveText.text = objectiveDescription;
        }

        private void Update()
        {
            if (eventManager == null) return;

            // Show objective when start event is completed
            if (!objectiveShown && eventManager.IsEventCompleted(startObjectiveEvent))
            {
                objectiveShown = true;

                if (objectiveCanvas != null)
                    objectiveCanvas.SetActive(true);

                // Play sound
                if (audioSource != null && objectiveSound != null)
                {
                    audioSource.PlayOneShot(objectiveSound);
                }

                // Fade in UI
                if (objectiveCanvasGroup != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeInCanvas(objectiveCanvasGroup, fadeDelay, fadeDuration));
                }
            }

            // Hide objective when completion event is completed
            if (objectiveShown && eventManager.IsEventCompleted(completeObjectiveEvent))
            {
                if (objectiveCanvas != null)
                    objectiveCanvas.SetActive(false);
            }
        }

        private IEnumerator FadeInCanvas(CanvasGroup cg, float delay, float duration)
        {
            if (cg == null) yield break;

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
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackboardInteraction : MonoBehaviour
{
    [Header("Key Interaction UI")]
    public CanvasGroup keyUICanvasGroup;   // Canvas with 1,2,3 keys UI
    public List<KeyAudio> keyAudios;       // List of keys and assigned audio clips
    public GameEvent prerequisiteEvent;    // The event that must be completed first

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;      // Duration of fade
    public float fadeDelay = 0.3f;         // Delay before fade starts

    private bool isActive = false;
    private EventManager eventManager;
    private AudioSource audioSource;

    [System.Serializable]
    public class KeyAudio
    {
        public KeyCode mainKey;     // Top row number key (Alpha1, Alpha2, etc.)
        public KeyCode altKey;      // Numpad key (Keypad1, Keypad2, etc.)
        public AudioClip clip;      // Audio to play when pressed
    }

    private void Start()
    {
        if (keyUICanvasGroup != null)
            keyUICanvasGroup.alpha = 0f;

        // Find the EventManager in the scene
        eventManager = FindFirstObjectByType<EventManager>();

        // Add or get AudioSource for playing clips
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Toggle UI on/off with E
        if (Input.GetKeyDown(KeyCode.E) && prerequisiteEvent != null && eventManager != null &&
            eventManager.IsEventCompleted(prerequisiteEvent))
        {
            isActive = !isActive;

            // Stop audio if hiding UI
            if (!isActive && audioSource.isPlaying)
                audioSource.Stop();

            // Start fade coroutine
            if (keyUICanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCanvas(keyUICanvasGroup, isActive ? 1f : 0f, fadeDuration, fadeDelay));
            }
        }

        // If UI is active, check key presses
        if (isActive)
        {
            foreach (var ka in keyAudios)
            {
                if ((Input.GetKeyDown(ka.mainKey) || Input.GetKeyDown(ka.altKey)) && ka.clip != null)
                {
                    audioSource.clip = ka.clip;
                    audioSource.Play(); // This automatically stops the previous clip
                }
            }
        }
    }

    private IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);

        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }
}
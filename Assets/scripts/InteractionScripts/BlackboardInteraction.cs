using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackboardInteraction : MonoBehaviour
{
    [Header("Key Interaction UI")]
    public CanvasGroup keyUICanvasGroup;   // Canvas with 1,2,3 keys UI
    public List<KeyAudio> keyAudios;       // List of keys and assigned audio clips

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;      // Duration of fade
    public float fadeDelay = 0.3f;         // Delay before fade starts

    private bool isActive = false;         // True when inside interaction
    private AudioSource audioSource;

    [System.Serializable]
    public class KeyAudio
    {
        public KeyCode mainKey;     // Top row number key
        public KeyCode altKey;      // Numpad key
        public AudioClip clip;      // Audio to play
    }

    private void Start()
    {
        if (keyUICanvasGroup != null)
            keyUICanvasGroup.alpha = 0f;  // Start hidden

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        // Press E to enter/exit interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            isActive = !isActive;

            // Stop audio if exiting
            if (!isActive && audioSource.isPlaying)
                audioSource.Stop();

            // Fade UI in or out
            if (keyUICanvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCanvas(keyUICanvasGroup, isActive ? 1f : 0f, fadeDuration, fadeDelay));
            }
        }

        // If inside interaction, listen for key presses
        if (isActive)
        {
            foreach (var ka in keyAudios)
            {
                if ((Input.GetKeyDown(ka.mainKey) || Input.GetKeyDown(ka.altKey)) && ka.clip != null)
                {
                    audioSource.clip = ka.clip;
                    audioSource.Play(); // Stops previous clip automatically
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
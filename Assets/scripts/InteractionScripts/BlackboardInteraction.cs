using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackboardInteraction : MonoBehaviour
{
    [Header("Key Interaction UI")]
    public CanvasGroup keyUICanvasGroup;
    public List<KeyAudio> keyAudios;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.3f;

    private bool isActive = false;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    [System.Serializable]
    public class KeyAudio
    {
        public KeyCode mainKey;
        public KeyCode altKey;
        public AudioClip clip;
    }

    private void Start()
    {
        if (keyUICanvasGroup != null)
            keyUICanvasGroup.alpha = 0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!isActive)
            return;

        foreach (var ka in keyAudios)
        {
            if ((Input.GetKeyDown(ka.mainKey) || Input.GetKeyDown(ka.altKey)) && ka.clip != null)
            {
                audioSource.clip = ka.clip;
                audioSource.Play();
            }
        }
    }

    // Called from WindowInteraction when entering
    public void Activate()
    {
        if (isActive)
            return;

        isActive = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvas(keyUICanvasGroup, 1f, fadeDuration, fadeDelay));
    }

    // Called from WindowInteraction when exiting
    public void Deactivate()
    {
        if (!isActive)
            return;

        isActive = false;

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCanvas(keyUICanvasGroup, 0f, fadeDuration, fadeDelay));
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
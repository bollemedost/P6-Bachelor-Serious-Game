using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ObjectiveController : MonoBehaviour
{
    [Header("Event References")]
    public GameEvent startObjectiveEvent;
    public GameEvent completeObjectiveEvent;

    [Header("UI")]
    public GameObject objectiveCanvas;
    public CanvasGroup objectiveCanvasGroup;
    public TextMeshProUGUI objectiveText;
    [TextArea] public string objectiveDescription;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeDelay = 0.1f;

    [Header("Sound")]
    public AudioClip objectiveStartSound;
    public AudioClip objectiveAppearSound;

    [Header("Background Sound Ducking")]
    public AudioSource soundBG;
    [Range(0f, 1f)]
    public float backgroundVolumeMultiplierDuringObjective = 0.25f;

    private EventManager eventManager;
    private AudioSource audioSource;
    private bool objectiveShown = false;

    private float originalBGVolume;
    private Coroutine restoreBGVolumeCoroutine;

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

        if (soundBG != null)
            originalBGVolume = soundBG.volume;
    }

    private void Update()
    {
        if (eventManager == null) return;

        // SHOW OBJECTIVE
        if (!objectiveShown && eventManager.IsEventCompleted(startObjectiveEvent))
        {
            objectiveShown = true;

            if (objectiveCanvas != null)
                objectiveCanvas.SetActive(true);

            // Lower background volume
            if (soundBG != null)
            {
                originalBGVolume = soundBG.volume;
                soundBG.volume = originalBGVolume * backgroundVolumeMultiplierDuringObjective;
            }

            // Stop old coroutines FIRST
            if (objectiveCanvasGroup != null)
            {
                StopAllCoroutinesExceptBG();
                StartCoroutine(FadeInCanvas(objectiveCanvasGroup, fadeDelay, fadeDuration));
            }

            // THEN start sound queue (important order)
            StartCoroutine(PlayObjectiveSounds());
        }

        // HIDE OBJECTIVE
        if (objectiveShown && eventManager.IsEventCompleted(completeObjectiveEvent))
        {
            if (objectiveCanvas != null)
                objectiveCanvas.SetActive(false);
        }
    }

    //  Sequential sound playback (NO overlap)
    private IEnumerator PlayObjectiveSounds()
    {
        // First sound
        if (objectiveStartSound != null)
        {
            audioSource.clip = objectiveStartSound;
            audioSource.Play();

            // Wait until finished
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        // Second sound AFTER first finishes
        if (objectiveAppearSound != null)
        {
            audioSource.clip = objectiveAppearSound;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        // Restore background volume
        RestoreBackgroundVolume();
    }

    //  Fade using unscaled time (works when paused)
    private IEnumerator FadeInCanvas(CanvasGroup cg, float delay, float duration)
    {
        if (cg == null) yield break;

        yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        float startAlpha = cg.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private IEnumerator RestoreBGVolumeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        RestoreBackgroundVolume();
    }

    private void RestoreBackgroundVolume()
    {
        if (soundBG != null)
        {
            soundBG.volume = originalBGVolume;
        }
    }

    private void StopAllCoroutinesExceptBG()
    {
        bool bgRestoreWasRunning = restoreBGVolumeCoroutine != null;

        StopAllCoroutines();

        if (bgRestoreWasRunning)
        {
            restoreBGVolumeCoroutine = StartCoroutine(RestoreBGVolumeAfterDelay(0f));
        }
    }
}
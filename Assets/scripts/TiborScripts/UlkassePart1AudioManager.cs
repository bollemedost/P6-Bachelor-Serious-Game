using System.Collections;
using UnityEngine;

public class UlkassePart1AudioManager : MonoBehaviour
{
    public static UlkassePart1AudioManager Instance;

    [System.Serializable]
    public class NarrationStep
    {
        [TextArea(2, 5)]
        public string note;
        public AudioClip narrationClip;

        [Tooltip("If true, this step waits until one more correct slot is completed before continuing.")]
        public bool waitForNextCorrectPlacement = true;
    }

    [Header("Audio Sources")]
    public AudioSource narrationSource;
    public AudioSource dragItemSource;

    [Header("Narration Flow")]
    [Tooltip("Put the narration clips here in the order they should play.")]
    public NarrationStep[] narrationSteps;

    [Header("Slots To Watch")]
    [Tooltip("Drag all slots used in this minigame here.")]
    public slot[] watchedSlots;

    [Header("Settings")]
    public bool playOnStart = true;
    public bool stopNarrationWhenDraggingItem = false;
    public float autoContinueDelay = 0.15f;
    public bool debugLogs = true;

    private int currentNarrationIndex = 0;
    private int completedSlotsCheckpoint = 0;
    private bool waitingForProgress = false;
    private bool sequenceFinished = false;
    private Coroutine playRoutine;

    public bool IsSequenceFinished => sequenceFinished;
    public bool IsNarrationPlaying => narrationSource != null && narrationSource.isPlaying;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        completedSlotsCheckpoint = CountCompletedSlots();

        if (playOnStart)
        {
            StartSequence();
        }
    }

    void Update()
    {
        if (sequenceFinished)
            return;

        if (!waitingForProgress)
            return;

        int completedNow = CountCompletedSlots();

        if (completedNow > completedSlotsCheckpoint)
        {
            completedSlotsCheckpoint = completedNow;
            waitingForProgress = false;

            if (debugLogs)
                Debug.Log("[UlkassePart1AudioManager] New correct slot completed. Continuing narration.");

            if (playRoutine != null)
                StopCoroutine(playRoutine);

            playRoutine = StartCoroutine(ContinueAfterDelay(autoContinueDelay));
        }
    }

    public void StartSequence()
    {
        currentNarrationIndex = 0;
        completedSlotsCheckpoint = CountCompletedSlots();
        waitingForProgress = false;
        sequenceFinished = false;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        PlayCurrentStep();
    }

    void PlayCurrentStep()
    {
        if (narrationSteps == null || narrationSteps.Length == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[UlkassePart1AudioManager] No narration steps assigned.");

            sequenceFinished = true;
            return;
        }

        if (currentNarrationIndex >= narrationSteps.Length)
        {
            if (debugLogs)
                Debug.Log("[UlkassePart1AudioManager] Narration sequence finished.");

            sequenceFinished = true;
            return;
        }

        NarrationStep step = narrationSteps[currentNarrationIndex];

        if (narrationSource != null)
        {
            narrationSource.Stop();

            if (step.narrationClip != null)
            {
                narrationSource.clip = step.narrationClip;
                narrationSource.Play();

                if (debugLogs)
                    Debug.Log("[UlkassePart1AudioManager] Playing narration step " + currentNarrationIndex + ": " + step.narrationClip.name);
            }
            else
            {
                if (debugLogs)
                    Debug.LogWarning("[UlkassePart1AudioManager] Missing narration clip at step " + currentNarrationIndex);
            }
        }
        else
        {
            if (debugLogs)
                Debug.LogWarning("[UlkassePart1AudioManager] Narration source is not assigned.");
        }

        currentNarrationIndex++;

        if (step.waitForNextCorrectPlacement)
        {
            waitingForProgress = true;
        }
        else
        {
            float waitTime = autoContinueDelay;

            if (step.narrationClip != null)
                waitTime = step.narrationClip.length + autoContinueDelay;

            if (playRoutine != null)
                StopCoroutine(playRoutine);

            playRoutine = StartCoroutine(ContinueAfterDelay(waitTime));
        }
    }

    IEnumerator ContinueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayCurrentStep();
    }

    int CountCompletedSlots()
    {
        if (watchedSlots == null || watchedSlots.Length == 0)
            return 0;

        int count = 0;

        for (int i = 0; i < watchedSlots.Length; i++)
        {
            if (watchedSlots[i] == null)
                continue;

            if (watchedSlots[i].acceptAnyItem)
            {
                if (watchedSlots[i].transform.childCount > 0)
                    count++;
            }
            else
            {
                if (watchedSlots[i].IsCorrectPlaced)
                    count++;
            }
        }

        return count;
    }

    public void PlayDraggedItemSpeech(AudioClip clip)
    {
        if (clip == null)
            return;

        if (stopNarrationWhenDraggingItem && narrationSource != null && narrationSource.isPlaying)
        {
            narrationSource.Stop();
        }

        if (dragItemSource != null)
        {
            dragItemSource.Stop();
            dragItemSource.clip = clip;
            dragItemSource.Play();

            if (debugLogs)
                Debug.Log("[UlkassePart1AudioManager] Playing dragged item speech: " + clip.name);
        }
    }

    public void StopDraggedItemSpeech()
    {
        if (dragItemSource != null && dragItemSource.isPlaying)
        {
            dragItemSource.Stop();
        }
    }
}
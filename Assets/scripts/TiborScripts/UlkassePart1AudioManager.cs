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

    private Coroutine narrationFlowRoutine;
    private Coroutine dragSpeechRoutine;

    // Safer than a counter for this setup
    private bool narrationQueued = false;
    private bool dragSpeechQueued = false;

    private bool completionAudioLocked = false;

    public bool IsSequenceFinished => sequenceFinished;
    public bool IsNarrationPlaying => narrationSource != null && narrationSource.isPlaying;

    public bool IsAnyManagedAudioPlaying
    {
        get
        {
            bool narrationPlaying = narrationSource != null && narrationSource.isPlaying;
            bool dragPlaying = dragItemSource != null && dragItemSource.isPlaying;
            return narrationPlaying || dragPlaying;
        }
    }

    public bool IsAnyManagedAudioPlayingOrQueued
    {
        get
        {
            return IsAnyManagedAudioPlaying || narrationQueued || dragSpeechQueued;
        }
    }

    public bool IsCompletionAudioLocked => completionAudioLocked;

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
        if (completionAudioLocked)
            return;

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
                Debug.Log("[UlkassePart1AudioManager] New correct slot completed. Continuing narration when audio is free.");

            if (narrationFlowRoutine != null)
            {
                StopCoroutine(narrationFlowRoutine);
                narrationFlowRoutine = null;
                narrationQueued = false;
            }

            narrationFlowRoutine = StartCoroutine(ContinueAfterDelayAndSilence(autoContinueDelay));
        }
    }

    public void StartSequence()
    {
        completionAudioLocked = false;
        currentNarrationIndex = 0;
        completedSlotsCheckpoint = CountCompletedSlots();
        waitingForProgress = false;
        sequenceFinished = false;

        narrationQueued = false;
        dragSpeechQueued = false;

        if (narrationFlowRoutine != null)
            StopCoroutine(narrationFlowRoutine);

        if (dragSpeechRoutine != null)
            StopCoroutine(dragSpeechRoutine);

        narrationFlowRoutine = null;
        dragSpeechRoutine = null;

        if (narrationSource != null)
            narrationSource.Stop();

        if (dragItemSource != null)
            dragItemSource.Stop();

        PlayCurrentStep();
    }

    void PlayCurrentStep()
    {
        if (completionAudioLocked)
            return;

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
        int stepIndex = currentNarrationIndex;
        currentNarrationIndex++;

        if (narrationFlowRoutine != null)
        {
            StopCoroutine(narrationFlowRoutine);
            narrationFlowRoutine = null;
            narrationQueued = false;
        }

        narrationQueued = true;
        narrationFlowRoutine = StartCoroutine(PlayNarrationStepWhenReady(step, stepIndex));
    }

    IEnumerator PlayNarrationStepWhenReady(NarrationStep step, int stepIndex)
    {
        yield return WaitUntilAllManagedAudioFinished();

        narrationQueued = false;

        if (completionAudioLocked)
        {
            narrationFlowRoutine = null;
            yield break;
        }

        if (narrationSource != null)
        {
            narrationSource.Stop();

            if (step.narrationClip != null)
            {
                narrationSource.clip = step.narrationClip;
                narrationSource.Play();

                if (debugLogs)
                    Debug.Log("[UlkassePart1AudioManager] Playing narration step " + stepIndex + ": " + step.narrationClip.name);
            }
            else
            {
                if (debugLogs)
                    Debug.LogWarning("[UlkassePart1AudioManager] Missing narration clip at step " + stepIndex);
            }
        }
        else
        {
            if (debugLogs)
                Debug.LogWarning("[UlkassePart1AudioManager] Narration source is not assigned.");
        }

        if (completionAudioLocked)
        {
            narrationFlowRoutine = null;
            yield break;
        }

        if (step.waitForNextCorrectPlacement)
        {
            int completedNow = CountCompletedSlots();

            if (completedNow > completedSlotsCheckpoint)
            {
                completedSlotsCheckpoint = completedNow;
                waitingForProgress = false;

                if (debugLogs)
                    Debug.Log("[UlkassePart1AudioManager] Progress already happened during audio. Continuing automatically.");

                if (narrationFlowRoutine != null)
                {
                    StopCoroutine(narrationFlowRoutine);
                    narrationFlowRoutine = null;
                    narrationQueued = false;
                }

                narrationFlowRoutine = StartCoroutine(ContinueAfterDelayAndSilence(autoContinueDelay));
            }
            else
            {
                waitingForProgress = true;
            }
        }
        else
        {
            float waitTime = autoContinueDelay;

            if (step.narrationClip != null)
                waitTime = step.narrationClip.length + autoContinueDelay;

            if (narrationFlowRoutine != null)
            {
                StopCoroutine(narrationFlowRoutine);
                narrationFlowRoutine = null;
                narrationQueued = false;
            }

            narrationFlowRoutine = StartCoroutine(ContinueAfterDelayAndSilence(waitTime));
        }

        narrationFlowRoutine = null;
    }

    IEnumerator ContinueAfterDelayAndSilence(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return WaitUntilAllManagedAudioFinished();

        if (completionAudioLocked)
        {
            narrationFlowRoutine = null;
            yield break;
        }

        narrationFlowRoutine = null;
        PlayCurrentStep();
    }

    IEnumerator WaitUntilAllManagedAudioFinished()
    {
        while (IsAnyManagedAudioPlaying)
            yield return null;
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
        if (completionAudioLocked)
        {
            if (debugLogs)
                Debug.Log("[UlkassePart1AudioManager] Ignored dragged item speech because completion audio lock is active.");
            return;
        }

        if (clip == null)
            return;

        if (dragSpeechRoutine != null)
        {
            StopCoroutine(dragSpeechRoutine);
            dragSpeechRoutine = null;
            dragSpeechQueued = false;
        }

        dragSpeechQueued = true;
        dragSpeechRoutine = StartCoroutine(PlayDraggedItemSpeechWhenReady(clip));
    }

    IEnumerator PlayDraggedItemSpeechWhenReady(AudioClip clip)
    {
        if (stopNarrationWhenDraggingItem && narrationSource != null && narrationSource.isPlaying)
        {
            narrationSource.Stop();
        }
        else
        {
            yield return WaitUntilAllManagedAudioFinished();
        }

        dragSpeechQueued = false;

        if (completionAudioLocked)
        {
            dragSpeechRoutine = null;
            yield break;
        }

        if (dragItemSource != null)
        {
            dragItemSource.Stop();
            dragItemSource.clip = clip;
            dragItemSource.Play();

            if (debugLogs)
                Debug.Log("[UlkassePart1AudioManager] Playing dragged item speech: " + clip.name);
        }

        dragSpeechRoutine = null;
    }

    public void BeginCompletionAudioLock()
    {
        completionAudioLocked = true;
        waitingForProgress = false;

        narrationQueued = false;
        dragSpeechQueued = false;

        if (narrationFlowRoutine != null)
        {
            StopCoroutine(narrationFlowRoutine);
            narrationFlowRoutine = null;
        }

        if (dragSpeechRoutine != null)
        {
            StopCoroutine(dragSpeechRoutine);
            dragSpeechRoutine = null;
        }

        if (narrationSource != null)
            narrationSource.Stop();

        if (dragItemSource != null)
            dragItemSource.Stop();

        if (debugLogs)
            Debug.Log("[UlkassePart1AudioManager] Completion audio lock enabled. All managed audio stopped and future requests blocked.");
    }

    public void StopAllManagedAudio()
    {
        if (narrationFlowRoutine != null)
        {
            StopCoroutine(narrationFlowRoutine);
            narrationFlowRoutine = null;
        }

        if (dragSpeechRoutine != null)
        {
            StopCoroutine(dragSpeechRoutine);
            dragSpeechRoutine = null;
        }

        narrationQueued = false;
        dragSpeechQueued = false;
        waitingForProgress = false;

        if (narrationSource != null)
            narrationSource.Stop();

        if (dragItemSource != null)
            dragItemSource.Stop();

        if (debugLogs)
            Debug.Log("[UlkassePart1AudioManager] All managed audio stopped.");
    }

    public void StopDraggedItemSpeech()
    {
        if (dragSpeechRoutine != null)
        {
            StopCoroutine(dragSpeechRoutine);
            dragSpeechRoutine = null;
        }

        dragSpeechQueued = false;

        if (dragItemSource != null && dragItemSource.isPlaying)
        {
            dragItemSource.Stop();
        }
    }
}


//References:
//Troublshooting/inspiration with chatgpt
//https://www.youtube.com/watch?v=6OT43pvUyfY
//https://www.youtube.com/watch?v=g5WT91Sn3hg
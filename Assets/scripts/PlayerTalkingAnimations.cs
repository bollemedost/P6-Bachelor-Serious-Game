using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerTalkingAnimations : MonoBehaviour
{
    private Animator animator;

    // ===== BODY ANIMATIONS (must match exact Animator state names) =====
    public enum BodyAnimationType
    {
        Idle, Idle2, StandingIdle,
        Talk1, Talk2, Talk3, Talk4, Talk5,
        Cheer, LookAtLiam, LookAtEmma, Laugh
    }

    [System.Serializable]
    public struct AnimationStep
    {
        public float time;            // When to trigger in seconds
        public BodyAnimationType bodyAnim;
    }

    [System.Serializable]
    public class TalkingEvent
    {
        public string eventID;                 // Must match interaction's event ID
        public AnimationStep[] sequence;       // Sequence of animations
    }

    [Header("Event-Based Sequences")]
    public TalkingEvent[] talkingEvents;

    private Coroutine currentSequence;
    private bool isPlaying = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EventManager.OnEventCompleted += HandleEventCompleted;
    }

    private void OnDisable()
    {
        EventManager.OnEventCompleted -= HandleEventCompleted;
    }

    private void HandleEventCompleted(string eventID)
    {
        PlayEventSequence(eventID);
    }

    public void PlayEventSequence(string eventID)
    {
        if (isPlaying)
            StopSequence();

        TalkingEvent foundEvent = null;

        foreach (var evt in talkingEvents)
        {
            if (evt.eventID == eventID)
            {
                foundEvent = evt;
                break;
            }
        }

        if (foundEvent == null)
        {
            Debug.LogWarning($"No talking event found with ID: {eventID}");
            return;
        }

        currentSequence = StartCoroutine(RunSequence(foundEvent.sequence));
    }

    public void StopSequence()
    {
        if (currentSequence != null)
            StopCoroutine(currentSequence);

        currentSequence = null;
        isPlaying = false;

        // Return to idle
        animator.CrossFadeInFixedTime("Idle", 0.15f, 0);
    }

    private IEnumerator RunSequence(AnimationStep[] sequence)
    {
        if (sequence == null || sequence.Length == 0)
            yield break;

        isPlaying = true;
        float timer = 0f;
        int index = 0;

        while (index < sequence.Length)
        {
            timer += Time.deltaTime;

            while (index < sequence.Length && timer >= sequence[index].time)
            {
                PlayBodyAnimation(sequence[index].bodyAnim);
                index++;
            }

            yield return null;
        }

        isPlaying = false;
        currentSequence = null;
    }

    private void PlayBodyAnimation(BodyAnimationType type)
    {
        string stateName = type.ToString();

        if (animator.HasState(0, Animator.StringToHash(stateName)))
        {
            animator.CrossFadeInFixedTime(stateName, 0.1f, 0);
        }
        else
        {
            Debug.LogWarning($"Animator state '{stateName}' does not exist!");
        }
    }
}
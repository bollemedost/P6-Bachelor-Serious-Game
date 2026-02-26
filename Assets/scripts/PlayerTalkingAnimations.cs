using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerTalkingAnimations : MonoBehaviour
{
    private Animator animator;

    // ===== BODY ANIMATIONS (Must match Animator state names exactly) =====
    public enum BodyAnimationType
    {
        Idle,
        Idle2,
        StandingIdle,
        Talk1,
        Talk2,
        Talk3,
        Talk4,
        Talk5,
        Cheer,
        LookAtLiam,
        LookAtEmma,
        Laugh
    }

    [System.Serializable]
    public struct AnimationStep
    {
        [Tooltip("Time in seconds when this step triggers")]
        public float time;

        public BodyAnimationType bodyAnim;
    }

    [System.Serializable]
    public class TalkingEvent
    {
        [Header("Event That Triggers This Sequence")]
        public GameEvent gameEvent;

        [Header("Animation Timeline")]
        public AnimationStep[] sequence;
    }

    [Header("Event-Based Talking Sequences")]
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

    private void HandleEventCompleted(GameEvent completedEvent)
    {
        PlayEventSequence(completedEvent);
    }

    public void PlayEventSequence(GameEvent gameEvent)
    {
        if (gameEvent == null)
            return;

        // Stop any currently playing sequence
        if (isPlaying)
            StopSequence();

        foreach (var evt in talkingEvents)
        {
            if (evt.gameEvent == gameEvent)
            {
                currentSequence = StartCoroutine(RunSequence(evt.sequence));
                return;
            }
        }

        // No sequence found for this event (not an error)
    }

    public void StopSequence()
    {
        if (currentSequence != null)
            StopCoroutine(currentSequence);

        currentSequence = null;
        isPlaying = false;

        PlayBodyAnimation(BodyAnimationType.Idle);
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

        // Return to idle after finishing
        PlayBodyAnimation(BodyAnimationType.Idle);
    }

    private void PlayBodyAnimation(BodyAnimationType type)
    {
        string stateName = type.ToString();
        int stateHash = Animator.StringToHash(stateName);

        if (animator.HasState(0, stateHash))
        {
            animator.CrossFadeInFixedTime(stateHash, 0.1f, 0);
        }
        else
        {
            Debug.LogWarning($"Animator state '{stateName}' not found on {gameObject.name}");
        }
    }
}
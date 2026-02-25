using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerTalkingAnimations : MonoBehaviour
{
    [Header("Animator Reference")]
    public Animator animator;

    // BODY animations = Layer 0
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
        [Tooltip("Time in seconds when this step should trigger")]
        public float time;
        public BodyAnimationType bodyAnim;
    }

    [System.Serializable]
    public class TalkingEvent
    {
        public string eventID;             // Unique ID for the event
        public AnimationStep[] sequence;   // Animation sequence for this event
    }

    [Header("Event Sequences")]
    public TalkingEvent[] talkingEvents;

    private Coroutine sequenceCoroutine;

    /// <summary>
    /// Plays the animation sequence for a specific event ID
    /// </summary>
    public void PlayEventSequence(string eventID)
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerTalkingAnimations: No Animator assigned!");
            return;
        }

        // Find the event by ID
        TalkingEvent talkingEvent = null;
        foreach (var evt in talkingEvents)
        {
            if (evt.eventID == eventID)
            {
                talkingEvent = evt;
                break;
            }
        }

        if (talkingEvent == null)
        {
            Debug.LogWarning($"PlayerTalkingAnimations: EventID '{eventID}' not found!");
            return;
        }

        // Stop any running sequence
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        // Start the new sequence
        sequenceCoroutine = StartCoroutine(RunSequence(talkingEvent.sequence));
    }

    /// <summary>
    /// Stops the current sequence
    /// </summary>
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = null;
    }

    private IEnumerator RunSequence(AnimationStep[] sequence)
    {
        float timer = 0f;
        int index = 0;

        while (index < sequence.Length)
        {
            timer += Time.deltaTime;

            if (timer >= sequence[index].time)
            {
                PlayBodyAnimation(sequence[index].bodyAnim);
                index++;
            }

            yield return null;
        }

        sequenceCoroutine = null; // Sequence finished
    }

    private void PlayBodyAnimation(BodyAnimationType type)
    {
        if (animator != null)
            animator.CrossFade(type.ToString(), 0.1f, 0); // Layer 0
    }
}
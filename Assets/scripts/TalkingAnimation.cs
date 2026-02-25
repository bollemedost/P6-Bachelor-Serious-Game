using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TalkingAnimations : MonoBehaviour
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

    [Header("Animation Sequence")]
    public AnimationStep[] sequence;

    private Coroutine sequenceCoroutine;

    /// <summary>
    /// Starts the animation sequence from the beginning
    /// </summary>
    public void PlaySequence()
    {
        if (animator == null)
        {
            Debug.LogWarning("TalkingAnimations: No Animator assigned!");
            return;
        }

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    /// <summary>
    /// Stops the sequence immediately
    /// </summary>
    public void StopSequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = null;
    }

    private IEnumerator RunSequence()
    {
        float timer = 0f;
        int index = 0;

        while (index < sequence.Length)
        {
            timer += Time.deltaTime;

            // Trigger all steps that have elapsed
            if (timer >= sequence[index].time)
            {
                PlayBodyAnimation(sequence[index].bodyAnim);
                index++;
            }

            yield return null;
        }

        sequenceCoroutine = null; // sequence finished
    }

    private void PlayBodyAnimation(BodyAnimationType type)
    {
        if (animator != null)
            animator.CrossFade(type.ToString(), 0.1f, 0); // Layer 0
    }
}
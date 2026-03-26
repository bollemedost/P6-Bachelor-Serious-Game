using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimationStep
{
    public string triggerName;  // Animator trigger/state
    public float duration = 4f; // Duration of this step
}

public class NPCConversation : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public AnimationStep[] animationSequence;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public Transform player;
    public float hearingDistance = 15f;
    public float fadeSpeed = 2f;
    public bool randomPitch = true;

    private bool audioStarted = false;      // Audio has started
    private bool audioFinished = false;     // Audio has finished completely
    private float targetVolume = 0f;
    private Coroutine animationCoroutine;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.volume = 0f;

            if (randomPitch)
                audioSource.pitch = Random.Range(0.95f, 1.05f);
        }

        // Start looping animation sequence immediately
        if (animationSequence != null && animationSequence.Length > 0)
            animationCoroutine = StartCoroutine(PlayAnimationSequence());
    }

    void Update()
    {
        if (player == null || audioSource == null || audioSource.clip == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Start audio only once
        if (!audioStarted && distance <= hearingDistance)
        {
            audioSource.Play();
            audioStarted = true;
            targetVolume = 1f;
        }

        // Fade in/out based on distance
        if (audioStarted && !audioFinished)
        {
            targetVolume = distance <= hearingDistance ? 1f : 0f;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

            if (!audioSource.isPlaying && audioSource.volume <= 0.01f)
            {
                audioFinished = true;
                audioSource.volume = 0f;
            }
        }
    }

    IEnumerator PlayAnimationSequence()
    {
        int index = 0;

        while (animationSequence.Length > 0)
        {
            AnimationStep step = animationSequence[index];

            if (animator != null && !string.IsNullOrEmpty(step.triggerName))
                animator.CrossFade(step.triggerName, 0.2f);

            yield return new WaitForSeconds(step.duration);

            index++;
            if (index >= animationSequence.Length)
                index = 0; // loop sequence indefinitely
        }
    }
}
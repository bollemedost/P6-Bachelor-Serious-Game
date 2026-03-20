using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimationStep
{
    public string triggerName; // Animator state name
    public float duration = 4f; // How long this animation lasts before moving to next
}

public class NPCConversation : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public AnimationStep[] animationSequence; // Ordered sequence of animations

    [Header("Audio Settings")]
    public AudioSource audioSource; // Assign conversation audio
    public Transform player;        // Assign the player transform
    public float hearingDistance = 15f; // Distance where audio starts
    public float fadeSpeed = 2f;        // Optional fade-in speed

    private bool hasPlayed = false;
    private float targetVolume = 0f;

    void Start()
    {
        if (audioSource != null)
        {
            audioSource.loop = false;          // Do not repeat
            audioSource.playOnAwake = false;   // Do not start automatically
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = 0f;           // Start silent for fade-in
        }

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if (player == null || audioSource == null || audioSource.clip == null)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Start the audio only once when player gets close enough
        if (distance <= hearingDistance && !hasPlayed)
        {
            audioSource.Play();
            hasPlayed = true;
            targetVolume = 1f;
        }

        // Fade in only while the sound is playing
        if (audioSource.isPlaying)
        {
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                targetVolume,
                fadeSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator PlaySequence()
    {
        if (animationSequence == null || animationSequence.Length == 0)
            yield break;

        int index = 0;

        while (true)
        {
            AnimationStep step = animationSequence[index];

            if (animator != null && !string.IsNullOrEmpty(step.triggerName))
            {
                // Smoothly blend into the next animation
                animator.CrossFade(step.triggerName, 0.2f);
            }

            // Wait for the duration of this step
            yield return new WaitForSeconds(step.duration);

            // Move to next step, loop to start if at the end
            index = (index + 1) % animationSequence.Length;
        }
    }
}
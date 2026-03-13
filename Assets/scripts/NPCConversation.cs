using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimationStep
{
    public string triggerName; // Animator Trigger name
    public float duration = 4f; // How long this animation lasts before moving to next
}

public class NPCConversation : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator;
    public AnimationStep[] animationSequence; // Ordered sequence of animations

    [Header("Audio Settings")]
    public AudioSource audioSource; // Assign looping conversation audio
    public Transform player;        // Assign the player transform
    public float hearingDistance = 15f; // Distance where audio fades in
    public float fadeSpeed = 2f;        // Speed of audio fade

    private float targetVolume;

    void Start()
    {
        if(audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.volume = 0f;
        }

        StartCoroutine(PlaySequence());
    }

    void Update()
    {
        if(player == null || audioSource == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        targetVolume = (distance <= hearingDistance) ? 1f : 0f;

        // Smoothly fade volume
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        if(audioSource.volume > 0 && !audioSource.isPlaying)
            audioSource.Play();

        if(audioSource.volume == 0 && targetVolume == 0 && audioSource.isPlaying)
            audioSource.Stop();
    }

   IEnumerator PlaySequence()
    {
        if(animationSequence.Length == 0) yield break;

        int index = 0;

        while(true)
        {
            AnimationStep step = animationSequence[index];

            if(animator != null && !string.IsNullOrEmpty(step.triggerName))
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
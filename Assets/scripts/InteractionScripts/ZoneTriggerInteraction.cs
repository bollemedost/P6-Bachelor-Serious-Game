using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TimedAudio
{
    public AudioClip clip;   // Audio clip to play
    public float time;       // Time (in seconds) after animation starts
}

public class ZoneTriggerInteraction : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string waveTriggerName = "Wave";
    [SerializeField] private string upperBodyLayerName = "UpperBody";
    [SerializeField] private float waveDuration = 2f; // total duration of the wave animation

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<TimedAudio> timedAudios = new List<TimedAudio>();

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            int layerIndex = animator.GetLayerIndex(upperBodyLayerName);

            // Enable upper body layer
            animator.SetLayerWeight(layerIndex, 1f);

            // Trigger wave animation
            animator.SetTrigger(waveTriggerName);

            // Start coroutine to play timed audios
            if (audioSource != null && timedAudios.Count > 0)
            {
                StartCoroutine(PlayTimedAudios());
            }

            // Disable upper body layer after animation ends
            StartCoroutine(DisableLayerAfterTime(layerIndex, waveDuration));
        }
    }

    private IEnumerator DisableLayerAfterTime(int layerIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetLayerWeight(layerIndex, 0f); // return to base animation
    }

    private IEnumerator PlayTimedAudios()
    {
        float startTime = Time.time;

        foreach (var ta in timedAudios)
        {
            float waitTime = ta.time - (Time.time - startTime);
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            audioSource.PlayOneShot(ta.clip);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TimedAudio
{
    public AudioClip clip;
    public float time; // Time after interaction starts
}

public class ZoneTriggerInteraction : MonoBehaviour
{
    [Header("Player Animation Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string playerTriggerName = "Wave";
    [SerializeField] private string playerUpperBodyLayerName = "UpperBody";
    [SerializeField] private float playerAnimationDuration = 2f;
    [SerializeField] private float playerEarlyExitTime = 0.2f; // Goes back to base layer slightly early
    [SerializeField] private float playerLayerFadeTime = 0.15f; // Smooth fade out duration

    [Header("NPC Animation Settings")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string npcTriggerName = "Wave";
    [SerializeField] private float npcDelay = 0f;
    [SerializeField] private float npcAnimationDuration = 2f;

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
            StartCoroutine(HandleInteraction());
        }
    }

    private IEnumerator HandleInteraction()
    {
        float interactionStartTime = Time.time;

        // =====================
        // PLAYER ANIMATION
        // =====================
        int layerIndex = -1;

        if (playerAnimator != null)
        {
            layerIndex = playerAnimator.GetLayerIndex(playerUpperBodyLayerName);

            if (layerIndex >= 0)
                playerAnimator.SetLayerWeight(layerIndex, 1f);

            playerAnimator.SetTrigger(playerTriggerName);
        }

        // =====================
        // NPC ANIMATION
        // =====================
        if (npcAnimator != null)
        {
            if (npcDelay > 0f)
                yield return new WaitForSeconds(npcDelay);

            npcAnimator.SetTrigger(npcTriggerName);
            yield return new WaitForSeconds(npcAnimationDuration);
        }

        // =====================
        // AUDIO
        // =====================
        if (audioSource != null && timedAudios.Count > 0)
        {
            StartCoroutine(PlayTimedAudios(interactionStartTime));
        }

        // =====================
        // RESET PLAYER LAYER (fade slightly early)
        // =====================
        if (layerIndex >= 0)
        {
            float adjustedDuration = Mathf.Max(0f, playerAnimationDuration - playerEarlyExitTime);
            yield return new WaitForSeconds(adjustedDuration);

            // Smooth fade out
            StartCoroutine(FadeOutLayer(playerAnimator, layerIndex, playerLayerFadeTime));
        }
    }

    private IEnumerator PlayTimedAudios(float startTime)
    {
        foreach (var ta in timedAudios)
        {
            float waitTime = ta.time - (Time.time - startTime);

            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);

            if (ta.clip != null)
                audioSource.PlayOneShot(ta.clip);
        }
    }

    private IEnumerator FadeOutLayer(Animator anim, int layerIndex, float fadeTime)
    {
        float startWeight = anim.GetLayerWeight(layerIndex);
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float weight = Mathf.Lerp(startWeight, 0f, timer / fadeTime);
            anim.SetLayerWeight(layerIndex, weight);
            yield return null;
        }

        anim.SetLayerWeight(layerIndex, 0f);
    }
}
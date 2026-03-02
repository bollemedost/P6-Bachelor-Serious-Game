using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float playerEarlyExitTime = 0.2f; // fade slightly early
    [SerializeField] private float playerLayerFadeTime = 0.15f; // smooth fade duration

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
        // =====================
        // START TIMED AUDIO
        // =====================
        if (audioSource != null && timedAudios.Count > 0)
            StartCoroutine(PlayTimedAudios());

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

    private IEnumerator PlayTimedAudios()
    {
        foreach (var ta in timedAudios)
        {
            if (ta.time > 0f)
                yield return new WaitForSeconds(ta.time);

            if (ta.clip != null && audioSource != null)
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
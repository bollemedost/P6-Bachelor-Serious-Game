using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TimedAudio
{
    public AudioClip clip;
    public float time;
}

public class ZoneTriggerInteraction : MonoBehaviour
{
    [Header("Player Animation Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string playerTriggerName = "Wave";
    [SerializeField] private string playerUpperBodyLayerName = "UpperBody";
    [SerializeField] private float playerAnimationDuration = 2f;

    [Header("NPC Animation Settings")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string npcTriggerName = "Wave";
    [SerializeField] private float npcDelay = 0f;
    [SerializeField] private float npcAnimationDuration = 2f; // NEW

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
        // PLAYER ANIMATION (UNCHANGED)
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

            // Wait for NPC animation duration
            yield return new WaitForSeconds(npcAnimationDuration);

            // Optional: Force return to Idle if needed
            // npcAnimator.Play("Idle");
        }

        // =====================
        // AUDIO
        // =====================
        if (audioSource != null && timedAudios.Count > 0)
        {
            StartCoroutine(PlayTimedAudios(interactionStartTime));
        }

        // =====================
        // RESET PLAYER LAYER
        // =====================
        if (layerIndex >= 0)
        {
            yield return new WaitForSeconds(playerAnimationDuration);
            playerAnimator.SetLayerWeight(layerIndex, 0f);
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
}
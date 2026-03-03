using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimedAudio
{
    public AudioClip clip;
    public float time;
}

public class ZoneTriggerInteraction : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private GameEvent zoneEvent;                  // 👈 Event this zone triggers
    [SerializeField] private GameEvent[] prerequisiteEvents;       // 👈 Optional requirements
    private EventManager eventManager;

    [Header("Player Animation Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string playerTriggerName = "Wave";
    [SerializeField] private string playerUpperBodyLayerName = "UpperBody";
    [SerializeField] private float playerAnimationDuration = 2f;
    [SerializeField] private float playerEarlyExitTime = 0.2f;
    [SerializeField] private float playerLayerFadeTime = 0.15f;

    [Header("NPC Animation Settings")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private string npcTriggerName = "Wave";
    [SerializeField] private float npcDelay = 0f;
    [SerializeField] private float npcAnimationDuration = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<TimedAudio> timedAudios = new List<TimedAudio>();

    private void Start()
    {
        eventManager = FindFirstObjectByType<EventManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (eventManager == null || zoneEvent == null) return;

        // 🚫 If already completed → do nothing
        if (eventManager.IsEventCompleted(zoneEvent))
            return;

        // 🚫 Check prerequisites
        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
                return;
        }

        // ✅ Trigger interaction
        StartCoroutine(HandleInteraction());

        // ✅ Mark event as completed
        eventManager.CompleteEvent(zoneEvent);
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
        // RESET PLAYER LAYER
        // =====================
        if (layerIndex >= 0)
        {
            float adjustedDuration = Mathf.Max(0f, playerAnimationDuration - playerEarlyExitTime);
            yield return new WaitForSeconds(adjustedDuration);
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
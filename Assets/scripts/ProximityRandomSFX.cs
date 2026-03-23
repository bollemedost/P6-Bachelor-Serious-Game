using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProximityRandomSFX : MonoBehaviour
{
    [Header("Audio Settings")]
    public List<AudioClip> soundEffects;
    public AudioSource audioSource;
    public float minInterval = 3f;
    public float maxInterval = 10f;

    [Header("Proximity Settings")]
    public Transform player;
    public float maxHearingDistance = 15f;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;
    

    private void Start()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f; // 2D sound controlled by volume script
        StartCoroutine(PlayRandomSounds());
    }

    private IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            if (soundEffects.Count == 0 || player == null)
            {
                yield return null;
                continue;
            }

            // Pick a random clip
            AudioClip clip = soundEffects[Random.Range(0, soundEffects.Count)];
            audioSource.clip = clip;

            // Calculate volume based on proximity
            float distance = Vector3.Distance(transform.position, player.position);
            float normalizedDistance = Mathf.Clamp01(distance / maxHearingDistance);
            audioSource.volume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);

            // Play the clip
            audioSource.Play();

            // Wait until the clip finishes
            yield return new WaitForSeconds(clip.length);

            // Then wait a random interval before next clip
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxHearingDistance);
    }
}
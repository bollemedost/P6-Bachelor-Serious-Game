using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class ProximityRandomSFX : MonoBehaviour
{
    [Header("Audio Settings")]
    public List<AudioClip> soundEffects;
    public float minInterval = 3f;
    public float maxInterval = 10f;

    [Header("Proximity Settings")]
    public Transform player;
    public float maxHearingDistance = 15f;
    public float minDistance = 1f;   // Full volume distance
    public float maxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // --- 3D Audio Settings ---
        audioSource.spatialBlend = 1f; // FULL 3D
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxHearingDistance;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = maxVolume;
    }

    private void Start()
    {
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

            // Pick random clip
            AudioClip clip = soundEffects[Random.Range(0, soundEffects.Count)];

            audioSource.clip = clip;
            audioSource.Play();

            // Wait until clip finishes completely
            yield return new WaitWhile(() => audioSource.isPlaying);

            // Wait random break before next sound
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
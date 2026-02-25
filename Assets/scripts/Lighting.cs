using UnityEngine;
using System.Collections;

public class Lightning : MonoBehaviour
{
    [Header("Light Reference")]
    public Light lightningLight;

    [Header("Storm Timing")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    [Header("Flash Intensity")]
    public float minFlashIntensity = 2f;
    public float maxFlashIntensity = 4f;
    public float dipIntensity = 0.5f;

    [Header("Flash Timing")]
    public float firstFlashDuration = 0.05f;
    public float dipDuration = 0.05f;
    public float secondFlashDuration = 0.08f;

    void Start()
    {
        lightningLight.intensity = 0f;
        StartCoroutine(LightningRoutine());
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            yield return StartCoroutine(Flash());
        }
    }

    IEnumerator Flash()
    {
        // First flash
        lightningLight.intensity = Random.Range(minFlashIntensity, maxFlashIntensity);
        yield return new WaitForSeconds(firstFlashDuration);

        // Dip
        lightningLight.intensity = dipIntensity;
        yield return new WaitForSeconds(dipDuration);

        // Second stronger flash
        lightningLight.intensity = Random.Range(minFlashIntensity, maxFlashIntensity);
        yield return new WaitForSeconds(secondFlashDuration);

        lightningLight.intensity = 0f;
    }
}
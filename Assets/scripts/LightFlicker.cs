using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Light Reference")]
    public Light lampLight;

    [Header("Base Intensity")]
    public float baseIntensity = 2f;

    [Header("Flicker Settings")]
    public float flickerAmount = 0.5f;     // How strong the flicker is
    public float flickerSpeed = 5f;        // How fast it flickers
    public bool randomFlicker = true;      // Organic vs consistent flicker

    private float noiseOffset;

    void Start()
    {
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float flicker;

        if (randomFlicker)
        {
            // Smooth organic flicker using Perlin Noise
            flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        }
        else
        {
            // More mechanical flicker
            flicker = Mathf.PingPong(Time.time * flickerSpeed, 1f);
        }

        float intensity = baseIntensity - (flickerAmount * flicker);
        lampLight.intensity = intensity;
    }
}
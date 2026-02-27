using TMPro;
using UnityEngine;

public class MinigameUIManager : MonoBehaviour
{
    public TextMeshProUGUI signatureText;
    public int totalSignatures = 10;
    private int currentSignatures = 0;

    [Header("All Signatures Collected Image")]
    public GameObject allSignaturesCollectedImage; // Drag your image here

    [Header("Sound Effects")]
    public AudioClip signatureSound; // Drag your SFX here
    private AudioSource audioSource;

    private void Start()
    {
        if (allSignaturesCollectedImage != null)
            allSignaturesCollectedImage.SetActive(false); // hide at start

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Call this when the player gets a signature
    public void AddSignature()
    {
        currentSignatures++;
        UpdateUI();

        // Play sound
        if (signatureSound != null && audioSource != null)
            audioSource.PlayOneShot(signatureSound);

        // Show image if all signatures collected
        if (currentSignatures >= totalSignatures && allSignaturesCollectedImage != null)
        {
            allSignaturesCollectedImage.SetActive(true);
        }
    }

    private void UpdateUI()
    {
        if (signatureText != null)
            signatureText.text = $"Underskrift {currentSignatures}/{totalSignatures}";
    }

    public void ResetSignatures()
    {
        currentSignatures = 0;
        UpdateUI();

        if (allSignaturesCollectedImage != null)
            allSignaturesCollectedImage.SetActive(false); // hide again on reset
    }
}
using TMPro;
using UnityEngine;

public class MinigameUIManager : MonoBehaviour
{
    public TextMeshProUGUI signatureText;

    [Header("Signature Settings")]
    public int signaturesRequiredToReturn = 10; // REQUIRED amount
    private int currentSignatures = 0;

    [Header("All Signatures Collected Image")]
    public GameObject allSignaturesCollectedImage;

    [Header("Return Objective Event")]
    public GameEvent returnAvailableEvent; // NEW
    private bool returnEventTriggered = false; // prevent multiple triggers

    [Header("Sound Effects")]
    public AudioClip signatureSound;
    private AudioSource audioSource;

    private void Start()
    {
        if (allSignaturesCollectedImage != null)
            allSignaturesCollectedImage.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateUI();
    }

    public void AddSignature()
    {
        currentSignatures++;
        UpdateUI();

        // Play signature sound
        if (signatureSound != null && audioSource != null)
            audioSource.PlayOneShot(signatureSound);

        // If required amount reached → trigger return event ONCE
        if (!returnEventTriggered && currentSignatures >= signaturesRequiredToReturn)
        {
            returnEventTriggered = true;

            EventManager evtManager = FindFirstObjectByType<EventManager>();
            if (evtManager != null && returnAvailableEvent != null)
            {
                evtManager.CompleteEvent(returnAvailableEvent);
            }

            if (allSignaturesCollectedImage != null)
                allSignaturesCollectedImage.SetActive(true);
        }
    }

    private void UpdateUI()
    {
        if (signatureText != null)
            signatureText.text = $"Underskrifter Indsamlet {currentSignatures}/{signaturesRequiredToReturn}";
    }

    public void ResetSignatures()
    {
        currentSignatures = 0;
        returnEventTriggered = false;

        UpdateUI();

        if (allSignaturesCollectedImage != null)
            allSignaturesCollectedImage.SetActive(false);
    }
}
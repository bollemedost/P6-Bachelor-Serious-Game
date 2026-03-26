using UnityEngine;

public class InteractableSound : MonoBehaviour
{
    public Transform player;
    public float interactionDistance = 3f;

    public GameObject canvasUI;
    public AudioSource audioSource;
    public AudioClip[] soundClips;

    public KeyCode interactKey = KeyCode.E;

    private bool canInteract = true;
    private bool playerInRange = false;

    void Start()
    {
        if (canvasUI != null)
            canvasUI.SetActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionDistance;

        // Show UI only if player is close AND can interact
        if (playerInRange && canInteract)
        {
            if (canvasUI != null && !canvasUI.activeSelf)
                canvasUI.SetActive(true);

            if (Input.GetKeyDown(interactKey))
            {
                PlayRandomSound();
            }
        }
        else
        {
            if (canvasUI != null && canvasUI.activeSelf)
                canvasUI.SetActive(false);
        }

        // Check if sound finished
        if (!canInteract && audioSource != null && !audioSource.isPlaying)
        {
            canInteract = true;
        }
    }

    void PlayRandomSound()
    {
        if (soundClips == null || soundClips.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, soundClips.Length);
        audioSource.clip = soundClips[randomIndex];
        audioSource.Play();

        // Disable interaction + hide UI
        canInteract = false;

        if (canvasUI != null)
            canvasUI.SetActive(false);
    }
}
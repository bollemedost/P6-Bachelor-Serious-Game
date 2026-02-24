using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(AudioSource))]
public class MomInteraction : Interactable
{
    [Header("Event Settings")]
    public string eventID = "MomTalk";           // unique event for Mom
    private EventManager eventManager;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera momCam;

    [Header("Audio & Animations")]
    public AudioSource audioSource;
    public Animation momAnimation;
    [Tooltip("Set timestamps (seconds) and animation names")]
    public AudioAnimationKey[] audioKeys;

    [Header("Player Interaction Settings")]
    public Transform playerTransform;
    public float interactionDistance = 2f;
    public float maxDistanceFromCam = 1.5f;

    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private Vector3 interactionCenter;
    private bool isInteracting = false;

    protected override void Start()
    {
        base.Start();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // EventManager reference (using new Unity API)
        eventManager = Object.FindFirstObjectByType<EventManager>();
        if (eventManager == null)
            Debug.LogError("No EventManager found in scene!");
    }

    public override void Interact()
    {
        if (!isInteracting)
        {
            StartInteraction();
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;

        if (playerTransform != null)
        {
            originalPlayerPos = playerTransform.position;
            originalPlayerRot = playerTransform.rotation;

            Vector3 direction = (playerTransform.position - transform.position).normalized;
            interactionCenter = transform.position + direction * interactionDistance;
            playerTransform.position = interactionCenter;
        }

        // Lock player movement slightly
        MovementStateManager.canMove = true;

        // Camera switch
        momCam.Priority = 10;
        playerCam.Priority = 0;

        if (canvas != null)
            canvas.SetActive(false);

        if (audioSource.clip != null)
            audioSource.Play();

        foreach (var key in audioKeys)
            key.triggered = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!isInteracting) return;

        // Exit interaction with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndInteraction();
        }

        // Trigger animations based on audio timestamp
        if (audioSource.isPlaying)
        {
            foreach (var key in audioKeys)
            {
                if (!key.triggered && audioSource.time >= key.time)
                {
                    momAnimation.Play(key.animationName);
                    key.triggered = true;
                }
            }
        }

        if (playerTransform != null)
        {
            // Clamp player within max distance of interaction center
            Vector3 offset = playerTransform.position - interactionCenter;
            if (offset.magnitude > maxDistanceFromCam)
                playerTransform.position = interactionCenter + offset.normalized * maxDistanceFromCam;

            // Make Player face Mom
            Vector3 playerLookDir = transform.position - playerTransform.position;
            playerLookDir.y = 0;
            if (playerLookDir != Vector3.zero)
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(playerLookDir), Time.deltaTime * 5f);

            // Make Mom face Player
            Vector3 momLookDir = playerTransform.position - transform.position;
            momLookDir.y = 0;
            if (momLookDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(momLookDir), Time.deltaTime * 5f);
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;

        if (playerTransform != null)
        {
            playerTransform.position = originalPlayerPos;
            playerTransform.rotation = originalPlayerRot;
        }

        MovementStateManager.canMove = true;

        playerCam.Priority = 10;
        momCam.Priority = 0;

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (momAnimation != null)
            momAnimation.Play("Idle");

        // ✅ Mark MomTalk event as completed
        if (eventManager != null)
            eventManager.CompleteEvent(eventID);
    }
}

// Simple struct to map audio timestamp → animation
[System.Serializable]
public class AudioAnimationKey
{
    public float time;
    public string animationName;
    [HideInInspector] public bool triggered;
}
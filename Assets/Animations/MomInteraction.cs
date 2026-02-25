using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(TalkingAnimations), typeof(AudioSource))]
public class MomInteraction : Interactable
{
    [Header("Event Settings")]
    public string eventID = "MomTalk";
    public string[] prerequisiteEvents; // optional: events that must be completed before talking
    private EventManager eventManager;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera momCam;

    [Header("Player Interaction Settings")]
    public Transform playerTransform;
    public float interactionRadius = 2f;

    [Header("Talking Animations")]
    public TalkingAnimations talkingAnimations; // Mom's talking animations

    [Header("Audio")]
    public AudioClip dialogueClip;       // Audio clip to play
    private AudioSource audioSource;     // AudioSource component

    private Vector3 interactionCenter;
    private bool isInteracting = false;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();

        if (talkingAnimations == null)
            talkingAnimations = GetComponent<TalkingAnimations>();

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public override void Interact()
    {
        if (!isInteracting && CanStartInteraction())
            StartInteraction();
    }

    private bool CanStartInteraction()
    {
        if (eventManager == null) return true;

        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
                return false; // prerequisites not completed
        }

        return true;
    }

    private void StartInteraction()
    {
        isInteracting = true;
        interactionCenter = transform.position;

        // Switch cameras for cinematic view
        if (playerCam != null && momCam != null)
        {
            momCam.Priority = 10;
            playerCam.Priority = 0;
        }

        // Start Mom's talking animation sequence
        if (talkingAnimations != null)
            talkingAnimations.PlaySequence();

        // Play audio if assigned
        if (dialogueClip != null && audioSource != null)
        {
            audioSource.clip = dialogueClip;
            audioSource.Play();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isInteracting) return;

        // Escape cancels interaction
        if (Input.GetKeyDown(KeyCode.Escape))
            EndInteraction();

        ClampPlayerInsideRadius();
        FaceEachOther();
    }

    private void ClampPlayerInsideRadius()
    {
        Vector3 offset = playerTransform.position - interactionCenter;
        offset.y = 0;

        if (offset.magnitude > interactionRadius)
        {
            Vector3 clampedPosition = interactionCenter + offset.normalized * interactionRadius;
            playerTransform.position = new Vector3(
                clampedPosition.x,
                playerTransform.position.y,
                clampedPosition.z
            );
        }
    }

    private void FaceEachOther()
    {
        // Player faces Mom
        Vector3 lookDir = transform.position - playerTransform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.LookRotation(lookDir),
                Time.deltaTime * 5f
            );
        }

        // Mom faces Player
        Vector3 momLookDir = playerTransform.position - transform.position;
        momLookDir.y = 0;
        if (momLookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(momLookDir),
                Time.deltaTime * 5f
            );
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;

        // Restore cameras
        if (playerCam != null && momCam != null)
        {
            playerCam.Priority = 10;
            momCam.Priority = 0;
        }

        // Stop talking animation sequence if needed
        if (talkingAnimations != null)
            talkingAnimations.StopSequence();

        // Stop audio if still playing
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        // Mark event complete
        if (eventManager != null)
            eventManager.CompleteEvent(eventID);
    }

    // Blocks the E canvas while interacting
    protected override bool IsCurrentlyInteracting()
    {
        return isInteracting;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
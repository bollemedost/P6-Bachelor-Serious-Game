using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(AudioSource))]
public class MomInteraction : Interactable
{
    [Header("Event Settings")]
    public string eventID = "MomTalk";
    private EventManager eventManager;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera momCam;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Player Interaction Settings")]
    public Transform playerTransform;
    public float interactionRadius = 2f;   // Radius player can move inside

    private Vector3 interactionCenter;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private bool isInteracting = false;

    protected override void Start()
    {
        base.Start();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        eventManager = Object.FindFirstObjectByType<EventManager>();
    }

    public override void Interact()
    {
        if (!isInteracting)
            StartInteraction();
    }

    private void StartInteraction()
    {
        isInteracting = true;

        originalPlayerPos = playerTransform.position;
        originalPlayerRot = playerTransform.rotation;

        // Set center to Mom's position
        interactionCenter = transform.position;

        // Disable free roaming movement
        MovementStateManager.canMove = true; // Keep true if you want movement INSIDE radius

        // Switch cameras
        momCam.Priority = 10;
        playerCam.Priority = 0;

        if (canvas != null)
            canvas.SetActive(false);

        if (audioSource.clip != null)
            audioSource.Play();
    }

    protected override void Update()
    {
        base.Update();

        if (!isInteracting) return;

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
            playerTransform.position = new Vector3(clampedPosition.x, playerTransform.position.y, clampedPosition.z);
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
                Time.deltaTime * 5f);
        }

        // Mom faces Player
        Vector3 momLookDir = playerTransform.position - transform.position;
        momLookDir.y = 0;
        if (momLookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(momLookDir),
                Time.deltaTime * 5f);
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;

        playerTransform.position = originalPlayerPos;
        playerTransform.rotation = originalPlayerRot;

        playerCam.Priority = 10;
        momCam.Priority = 0;

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (eventManager != null)
            eventManager.CompleteEvent(eventID);
    }

    // Draw radius in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
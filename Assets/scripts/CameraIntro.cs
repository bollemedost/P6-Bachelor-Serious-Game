using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraIntro : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera cinematicCam;
    public CinemachineVirtualCamera gameplayCam;

    [Header("Timing")]
    public float introDuration = 2f;       // Duration of cinematic
    public float blendDuration = 0.5f;     // Smooth blend to gameplay cam
    public float rotationUnlockDelay = 0.1f; // Small delay before allowing rotation

    void Start()
    {
        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        // Find player and lock movement/rotation
        MovementStateManager player = FindObjectOfType<MovementStateManager>();
        if (player != null) player.LockMovement(true);
        MovementStateManager.canRotate = false;

        // Wait for cinematic duration
        yield return new WaitForSeconds(introDuration);

        // Switch camera priorities
        cinematicCam.Priority = 5;
        gameplayCam.Priority = 20;

        // Smooth blend
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null) brain.m_DefaultBlend.m_Time = blendDuration;

        // Unlock movement
        if (player != null) player.LockMovement(false);

        // Optional delay before unlocking rotation to prevent snapping
        yield return new WaitForSeconds(rotationUnlockDelay);
        MovementStateManager.canRotate = true;
    }
}
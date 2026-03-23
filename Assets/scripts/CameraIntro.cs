using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraIntro : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera cinematicCam;
    public CinemachineVirtualCamera gameplayCam;

    [Header("Timing")]
    public float introDuration = 2f;       
    public float blendDuration = 0.5f;     
    public float rotationUnlockDelay = 0.1f; 

    [Header("Zoom Settings (FOV)")]
    public float introFOV = 80f;      // Starting FOV (Wide)
    public float gameplayFOV = 75f;   // Final FOV (Tighter)

    [Header("Audio")]
    public AudioSource introAudio;

    void Start()
    {
        if (introAudio != null)
            introAudio.Play();

        // Ensure cameras start at the correct FOV
        gameplayCam.m_Lens.FieldOfView = introFOV;

        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        MovementStateManager player = FindObjectOfType<MovementStateManager>();
        AimStateManager aimState = FindObjectOfType<AimStateManager>();

        if (player != null) player.LockMovement(true);
        MovementStateManager.canRotate = false;

        yield return new WaitForSeconds(introDuration);

        // 1. Swap Priorities to start the transition
        cinematicCam.Priority = 5;
        gameplayCam.Priority = 20;

        // 2. Sync the rotation immediately
        if (aimState != null)
        {
            aimState.MatchRotation(cinematicCam.transform.rotation);
        }

        // 3. UNLOCK MOVEMENT IMMEDIATELY (Don't wait for the blend!)
        if (player != null) 
        {
            player.LockMovement(false);
            Debug.Log("Movement Unlocked instantly!");
        }

        // 4. Wait for the camera to actually finish its flight
        yield return new WaitForSeconds(blendDuration);
        
        // 5. Unlock rotation last to prevent snapping during the blend
        yield return new WaitForSeconds(rotationUnlockDelay);
        MovementStateManager.canRotate = true;
    }
}
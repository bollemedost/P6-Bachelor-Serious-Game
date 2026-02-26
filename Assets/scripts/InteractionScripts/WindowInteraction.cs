using UnityEngine;
using Cinemachine;

public class WindowInteraction : Interactable
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera windowCam;

    private bool isActive = false;

    protected override void Update()
    {
        base.Update(); // Handles E toggle and canvas

        // Additional logic if needed while interacting
        if (!isActive)
            return;

        // You can add window-specific logic here
    }

    public override void Interact()
    {
        EnterWindow();
    }

    protected override void StopInteraction()
    {
        ExitWindow();
    }

    private void EnterWindow()
    {
        isActive = true;

        windowCam.Priority = 10;
        playerCam.Priority = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ExitWindow()
    {
        isActive = false;

        playerCam.Priority = 10;
        windowCam.Priority = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isActive;
    }
}
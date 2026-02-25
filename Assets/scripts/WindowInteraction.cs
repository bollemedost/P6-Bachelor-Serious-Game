using UnityEngine;
using Cinemachine;

public class WindowInteraction : Interactable
{
    public CinemachineVirtualCamera playerCam;
    public CinemachineVirtualCamera windowCam;

    private bool isActive = false;

    protected override void Update()
    {
        base.Update();

        if (!isActive)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitWindow();
        }
    }

    public override void Interact()
    {
        if (!isActive)
            EnterWindow();
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
}
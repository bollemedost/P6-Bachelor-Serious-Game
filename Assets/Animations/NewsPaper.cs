using UnityEngine;
using Cinemachine;

public class NewsPaper : Interactable
{
    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;      
    public CinemachineVirtualCamera newspaperCam;   

    [Header("Scroll Zoom Settings")]
    public float zoomAmount = 0.2f;
    public float zoomSpeed = 5f;

    private bool isZoomed = false;
    private Transform camTransform;
    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool isScrollingForward = false;

    protected override void Start()
    {
        base.Start();

        if (newspaperCam != null)
        {
            camTransform = newspaperCam.transform;
            originalPos = camTransform.position;
            targetPos = originalPos;
        }
    }

    protected override void Update()
    {
        base.Update();

        // 🔒 Force canvas OFF while zoomed
        if (isZoomed && canvas != null)
            canvas.SetActive(false);

        if (!isZoomed || camTransform == null)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0.01f)
        {
            targetPos += camTransform.forward * scroll * zoomAmount;
            isScrollingForward = true;
        }
        else if (scroll <= 0.0f && isScrollingForward)
        {
            isScrollingForward = false;
        }

        // Smooth return to original position
        if (!isScrollingForward)
        {
            targetPos = Vector3.Lerp(camTransform.position, originalPos, Time.deltaTime * zoomSpeed);
        }

        camTransform.position = targetPos;

        // Exit zoom with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ZoomOut();
        }
    }

    public override void Interact()
    {
        if (!isZoomed)
            ZoomIn();
    }

    private void ZoomIn()
    {
        isZoomed = true;

        // Switch cameras
        newspaperCam.Priority = 10;
        playerCam.Priority = 0;

        // Hide canvas immediately
        if (canvas != null)
            canvas.SetActive(false);

        if (camTransform != null)
            targetPos = originalPos;

        isScrollingForward = false;
    }

    private void ZoomOut()
    {
        isZoomed = false;

        // Switch cameras back
        playerCam.Priority = 10;
        newspaperCam.Priority = 0;

        // Reset camera position
        if (camTransform != null)
            camTransform.position = originalPos;
    }
}
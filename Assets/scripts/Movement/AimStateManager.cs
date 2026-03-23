using UnityEngine;

public class AimStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform camFollowPos;

    [Header("Mouse Settings")]
    [SerializeField] float mouseSense = 0.1f;
    [SerializeField] float smoothTime = 0.05f;
    [SerializeField] float verticalClamp = 40f;

    float xAxis;
    float yAxis;

    Vector2 currentMouse;
    Vector2 mouseSmoothVelocity;

    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        // If you forgot to drag it in the inspector, this will try to find it
        if (camFollowPos == null)
        {
            // Adjust "CameraPivot" to whatever the name of your child object is
            Transform found = transform.Find("CameraPivot"); 
            if (found != null) camFollowPos = found;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 targetMouse = controls.Player.Look.ReadValue<Vector2>();

        currentMouse = Vector2.SmoothDamp(
            currentMouse,
            targetMouse,
            ref mouseSmoothVelocity,
            smoothTime
        );

        xAxis += currentMouse.x * mouseSense;
        yAxis -= currentMouse.y * mouseSense;

        yAxis = Mathf.Clamp(yAxis, -verticalClamp, verticalClamp);
    }

    void LateUpdate()
    {
        // Rotate camera pivot ONLY (not player)

        // vertical
        camFollowPos.localRotation = Quaternion.Euler(yAxis, 0f, 0f);

        // horizontal
        transform.rotation = Quaternion.Euler(0f, xAxis, 0f);
    }

    // Inside AimStateManager.cs

    public void MatchRotation(Quaternion targetRotation)
    {
        // If camFollowPos is missing, log a helpful error but don't CRASH the coroutine
        if (camFollowPos == null)
        {
            Debug.LogError($"AimStateManager on {gameObject.name} is missing camFollowPos! Assign it in the Inspector.");
            return; 
        }

        Vector3 angles = targetRotation.eulerAngles;
        xAxis = angles.y;

        float x = angles.x;
        if (x > 180) x -= 360; 
        yAxis = x;
        
        transform.rotation = Quaternion.Euler(0f, xAxis, 0f);
        camFollowPos.localRotation = Quaternion.Euler(yAxis, 0f, 0f);
    }
}
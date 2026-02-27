using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRunnerT : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 4f;
    public float laneOffset = 2f;
    public float laneChangeSpeed = 10f;

    [Header("Gravity")]
    public float gravity = -25f;
    private float verticalVelocity = 0f;

    [Header("Bounce Back (wrong answer)")]
    public float bounceBackDistance = 8f;
    public float bounceBackDuration = 0.30f;
    public float bounceUpVelocity = 2.5f;
    public bool stopForwardDuringBounce = true;

    [Header("Anti re-trigger")]
    public float ignoreTriggersAfterBounce = 0.20f; // prevents instant re-hit
    private float ignoreTriggerTimer = 0f;

    [Header("Debug")]
    public bool debugLogs = true;

    private CharacterController cc;
    private int targetLane = 0; // -1 left, 0 center, 1 right

    private bool isBouncing = false;
    private float bounceTimer = 0f;
    private Vector3 bounceVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (debugLogs) Debug.Log($"[PlayerRunnerT] Awake on {name}. CC={(cc != null)}");
    }

    void Update()
    {
        if (ignoreTriggerTimer > 0f)
            ignoreTriggerTimer -= Time.deltaTime;

        HandleLaneInput();

        if (isBouncing)
            UpdateBounce();
        else
            UpdateRun();
    }

    void HandleLaneInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) targetLane = -1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) targetLane = 1;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) targetLane = 0;
    }

    void UpdateRun()
    {
        Vector3 move = Vector3.zero;

        move += Vector3.forward * forwardSpeed;

        float targetX = targetLane * laneOffset;
        float diffX = targetX - transform.position.x;
        move += Vector3.right * (diffX * laneChangeSpeed);

        ApplyGravity();
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }

    void UpdateBounce()
    {
        bounceTimer -= Time.deltaTime;

        if (debugLogs)
            Debug.Log($"[PlayerRunnerT] BOUNCING timer={bounceTimer:F2}");

        if (bounceTimer <= 0f)
        {
            isBouncing = false;
            if (debugLogs) Debug.Log("[PlayerRunnerT] Bounce finished.");
            return;
        }

        Vector3 move = bounceVelocity;

        if (!stopForwardDuringBounce)
            move += Vector3.forward * forwardSpeed;

        ApplyGravity();
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    // GateChoice calls this on wrong answer
    public void BounceBack(string reason = "")
    {
        // DO NOT reset lane anymore -> keeps same left/right lane

        isBouncing = true;
        bounceTimer = bounceBackDuration;

        float speedBack = bounceBackDistance / bounceBackDuration;
        bounceVelocity = Vector3.back * speedBack;

        verticalVelocity = bounceUpVelocity;

        // Prevent instant retrigger while still inside trigger volume
        ignoreTriggerTimer = ignoreTriggersAfterBounce;

        if (debugLogs)
            Debug.Log($"[PlayerRunnerT] BounceBack CALLED reason='{reason}' lane={targetLane}");
    }

    // GateTriggerProxy can call this to ignore trigger hits right after bounce
    public bool CanTriggerChoices()
    {
        return ignoreTriggerTimer <= 0f;
    }
}
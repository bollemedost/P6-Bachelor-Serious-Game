using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRunnerT : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 4f;
    public float laneOffset = 2f;
    public float laneChangeSpeed = 10f;

    [Header("Pause Near Active Gate")]
    public bool pauseNearActiveGate = true;
    public float gatePauseDistance = 2f;

    [Header("Gravity")]
    public float gravity = -25f;
    private float verticalVelocity = 0f;

    [Header("Bounce Back (optional, not used for wrong answers anymore)")]
    public float bounceBackDistance = 8f;
    public float bounceBackDuration = 0.30f;
    public float bounceUpVelocity = 2.5f;
    public bool stopForwardDuringBounce = true;

    [Header("Anti re-trigger")]
    public float ignoreTriggersAfterBounce = 0.20f;
    private float ignoreTriggerTimer = 0f;

    [Header("Debug")]
    public bool debugLogs = true;

    private CharacterController cc;
    private int targetLane = 0; // -1 left, 0 center, 1 right

    private bool isBouncing = false;
    private float bounceTimer = 0f;
    private Vector3 bounceVelocity;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool waitingForLaneChoice = false;
    private GateChoice pauseConsumedGate = null;
    private GateChoice lastSeenCurrentGate = null;

    void Awake()
    {
        cc = GetComponent<CharacterController>();

        if (debugLogs)
            Debug.Log($"[PlayerRunnerT] Awake on {name}. CC={(cc != null)}");
    }

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (debugLogs)
            Debug.Log($"[PlayerRunnerT] Start position saved: {startPosition}");
    }

    void Update()
    {
        if (ignoreTriggerTimer > 0f)
            ignoreTriggerTimer -= Time.deltaTime;

        if (isBouncing)
        {
            UpdateBounce();
            return;
        }

        CheckGatePause();

        if (waitingForLaneChoice)
        {
            UpdatePausedAtGate();
            return;
        }

        HandleLaneInput();
        UpdateRun();
    }

    void CheckGatePause()
    {
        if (!pauseNearActiveGate) return;

        GameManagerQuizRunner gm = FindFirstObjectByType<GameManagerQuizRunner>();
        if (gm == null) return;
        if (gm.IsFinished()) return;

        GateChoice currentGate = gm.GetCurrentGate();

        if (currentGate != lastSeenCurrentGate)
        {
            lastSeenCurrentGate = currentGate;
            pauseConsumedGate = null;
            waitingForLaneChoice = false;
        }

        if (currentGate == null) return;
        if (pauseConsumedGate == currentGate) return;

        float distance = Vector3.Distance(transform.position, currentGate.transform.position);

        if (distance <= gatePauseDistance)
        {
            waitingForLaneChoice = true;

            if (debugLogs)
                Debug.Log($"[PlayerRunnerT] Paused near gate '{currentGate.name}' at distance {distance:F2}");
        }
    }

    void UpdatePausedAtGate()
    {
        ApplyGravityOnly();

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetLane = -1;
            ResumeFromGatePause();
            return;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetLane = 1;
            ResumeFromGatePause();
            return;
        }
    }

    void ResumeFromGatePause()
    {
        GameManagerQuizRunner gm = FindFirstObjectByType<GameManagerQuizRunner>();
        if (gm != null)
            pauseConsumedGate = gm.GetCurrentGate();

        waitingForLaneChoice = false;

        if (debugLogs)
            Debug.Log("[PlayerRunnerT] Gate pause ended. Forward movement resumed.");
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

        if (bounceTimer <= 0f)
        {
            isBouncing = false;
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

    void ApplyGravityOnly()
    {
        ApplyGravity();

        Vector3 move = Vector3.zero;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }

    public void BounceBack(string reason = "")
    {
        isBouncing = true;
        bounceTimer = bounceBackDuration;

        float speedBack = bounceBackDistance / bounceBackDuration;
        bounceVelocity = Vector3.back * speedBack;

        verticalVelocity = bounceUpVelocity;
        ignoreTriggerTimer = ignoreTriggersAfterBounce;

        if (debugLogs)
            Debug.Log($"[PlayerRunnerT] BounceBack CALLED reason='{reason}' lane={targetLane}");
    }

    public void RespawnAtStart()
    {
        if (debugLogs)
            Debug.Log("[PlayerRunnerT] RespawnAtStart called.");

        isBouncing = false;
        bounceTimer = 0f;
        verticalVelocity = -2f;
        targetLane = 0;

        waitingForLaneChoice = false;
        pauseConsumedGate = null;
        lastSeenCurrentGate = null;

        if (cc != null)
            cc.enabled = false;

        transform.position = startPosition;
        transform.rotation = startRotation;

        if (cc != null)
            cc.enabled = true;

        ThirdPersonFollow camFollow = FindFirstObjectByType<ThirdPersonFollow>();
        if (camFollow != null)
        {
            camFollow.transform.position = transform.position + camFollow.offset;

            Vector3 lookTarget = transform.position + camFollow.lookAtOffset;
            Vector3 dir = lookTarget - camFollow.transform.position;

            if (dir.sqrMagnitude > 0.0001f)
                camFollow.transform.rotation = Quaternion.LookRotation(dir);
        }

        ignoreTriggerTimer = ignoreTriggersAfterBounce;
    }

    public bool CanTriggerChoices()
    {
        return ignoreTriggerTimer <= 0f;
    }
}
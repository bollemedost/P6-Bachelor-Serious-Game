using UnityEngine;
using UnityEngine.InputSystem;

public class MovementStateManager : MonoBehaviour
{
    public float moveSpeed = 3f;
    [HideInInspector] public Vector3 dir;

    CharacterController controller;
    PlayerControls controls;
    Vector2 moveInput;

    [SerializeField] float groundYOffset;
    [SerializeField] LayerMask groundMask;
    Vector3 spherePos;

    [SerializeField] float gravity = -9.81f;
    Vector3 velocity;

    MovementBaseState currentState;

    public IdleState idle = new IdleState();
    public WalkState walk = new WalkState();

    [HideInInspector] public Animator anim;

    [SerializeField] Transform camTransform;

    public static bool canMove = true;
    public static bool canRotate = true;

    private bool isLocked = false;

    [Header("Footsteps")]
    public AudioSource footstepSource;

    // After interaction ends, player must release movement input once
    // before walking is allowed again.
    private bool requireInputRelease = false;

    private bool wasMovingBeforeLock = false;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;
    }

    void OnDisable()
    {
        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Move.canceled -= OnMoveCanceled;
        controls.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (isLocked || !canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;

        // Once player releases movement after an interaction,
        // walking can be allowed again.
        if (requireInputRelease)
        {
            requireInputRelease = false;
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        currentState = idle;
        currentState.EnterState(this);
    }

    void Update()
    {
        ApplyGravity();

        if (!canMove || isLocked)
        {
            dir = Vector3.zero;
            moveInput = Vector2.zero;

            // Keep player visually idle while locked
            if (anim != null)
            {
                anim.SetBool("Walking", false);
                anim.SetFloat("hzInput", 0f);
                anim.SetFloat("vInput", 0f);
            }

            return;
        }

        // If interaction just ended and player is still holding movement,
        // force idle until they release the input once.
        if (requireInputRelease)
        {
            dir = Vector3.zero;

            if (anim != null)
            {
                anim.SetBool("Walking", false);
                anim.SetFloat("hzInput", 0f);
                anim.SetFloat("vInput", 0f);
            }

            if (currentState != idle)
                SwitchState(idle);

            return;
        }

        GetDirectionAndMove();
        currentState.UpdateState(this);
    }

    public void SwitchState(MovementBaseState state)
    {
        currentState.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }

    void GetDirectionAndMove()
    {
        float hzInput = moveInput.x;
        float vInput = moveInput.y;

        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        dir = camForward * vInput + camRight * hzInput;

        if (dir.magnitude > 0.1f && canRotate)
        {
            Vector3 lookDir = dir.normalized;
            lookDir.y = 0;

            Quaternion targetRot = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                10f * Time.deltaTime
            );

            transform.rotation = targetRot;
        }

        controller.Move(dir * moveSpeed * Time.deltaTime);
    }

    bool IsGrounded()
    {
        spherePos = new Vector3(
            transform.position.x,
            transform.position.y - groundYOffset,
            transform.position.z
        );

        return Physics.CheckSphere(
            spherePos,
            controller.radius - 0.05f,
            groundMask
        );
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
            velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    public void LockMovement(bool locked)
    {
        if (locked)
        {
            // ONLY store if player was actually moving
            wasMovingBeforeLock = moveInput.magnitude > 0.1f;
        }

        isLocked = locked;

        moveInput = Vector2.zero;
        dir = Vector3.zero;

        canMove = !locked;
        canRotate = !locked;

        if (!locked)
        {
            // ONLY require release IF player was moving before
            requireInputRelease = wasMovingBeforeLock;

            if (anim != null)
            {
                anim.SetBool("Walking", false);
                anim.SetFloat("hzInput", 0f);
                anim.SetFloat("vInput", 0f);
            }

            if (currentState != idle)
                SwitchState(idle);
        }

        if (locked && footstepSource != null)
        {
            footstepSource.Stop();
        }
    }

    public void ClearPostInteractionInputRequirement()
    {
        requireInputRelease = false;
        wasMovingBeforeLock = false;
        moveInput = Vector2.zero;
        dir = Vector3.zero;
    }
}
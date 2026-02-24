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

    // ================= INPUT SETUP =================
    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnDisable()
    {
        controls.Disable();
    }
    // =================================================

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        currentState = idle;
        currentState.EnterState(this);
    }

    void Update()
    {
        GetDirectionAndMove();
        ApplyGravity();

        Vector3 localDir = transform.InverseTransformDirection(dir);
        anim.SetFloat("hzInput", localDir.x);
        anim.SetFloat("vInput", localDir.z);

        currentState.UpdateState(this);
        if (!canMove) return;
    }

    public void SwitchState(MovementBaseState state)
    {
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

        if (dir.magnitude > 0.1f)
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

        controller.Move(dir.normalized * moveSpeed * Time.deltaTime);
    }

    bool IsGrounded()
    {
        spherePos = new Vector3(transform.position.x, transform.position.y - groundYOffset, transform.position.z);
        return Physics.CheckSphere(spherePos, controller.radius - 0.05f, groundMask);
    }

    void ApplyGravity()
    {
        if (!IsGrounded()) velocity.y += gravity * Time.deltaTime;
        else if (velocity.y < 0) velocity.y = -2f;

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spherePos, controller.radius - 0.05f);
    }
}
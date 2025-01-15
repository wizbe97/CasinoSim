using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputActions inputActions;

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    #region Sprint
    public bool enableSprint = true;
    public float sprintSpeed = 7f;
    private bool isSprinting = false;
    #endregion

    #endregion

    #region Jump
    public bool enableJump = true;
    public float jumpPower = 5f;
    private bool isGrounded = false;
    #endregion

    #region Crouch
    public bool enableCrouch = true;
    public float crouchHeight = 0.75f;
    public float speedReduction = 0.5f;

    private bool isCrouched = false;
    private Vector3 originalScale;
    #endregion

    #region Camera Variables
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;

    private Vector2 lookInput;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        LockCursor();

        inputActions = new PlayerInputActions();
    }


    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Clear input values to avoid startup issues
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => Jump();

        inputActions.Player.Sprint.started += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        inputActions.Player.Crouch.performed += ctx => ToggleCrouch();
    }


    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        #region Camera Look
        float scaledSensitivity = mouseSensitivity * Time.deltaTime;
        yaw += lookInput.x * scaledSensitivity;
        pitch -= lookInput.y * scaledSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.eulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        #endregion

        CheckGround();
    }


    private void FixedUpdate()
    {
        if (playerCanMove)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        // Calculate movement direction
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        moveDirection = transform.TransformDirection(moveDirection);

        if (isSprinting)
        {
            moveDirection *= sprintSpeed;
        }
        else
        {
            moveDirection *= walkSpeed;
        }

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = moveDirection - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        Vector3 direction = Vector3.down;
        float distance = 0.75f;

        isGrounded = Physics.Raycast(origin, direction, distance);
    }

    private void Jump()
    {
        if (enableJump && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;

            if (isCrouched)
            {
                ToggleCrouch();
            }
        }
    }

    private void ToggleCrouch()
    {
        if (!enableCrouch) return;

        if (isCrouched)
        {
            transform.localScale = originalScale;
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }
}

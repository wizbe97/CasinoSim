using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float maxVelocityChange = 10f;

    [Header("Jump Settings")]
    [SerializeField] private bool enableJump = true;
    [SerializeField] private float jumpPower = 5f;

    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isJumping = false;
    private bool isSprinting = false;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();

        LockCursor();
    }

    private void OnEnable()
    {
        inputHandler.OnJump += HandleJump;
        inputHandler.OnSprintStart += () => isSprinting = true;
        inputHandler.OnSprintEnd += () => isSprinting = false;
    }


    private void OnDisable()
    {
        inputHandler.OnSprint -= HandleSprint;
        inputHandler.OnJump -= HandleJump;
    }

    private void Update()
    {
        moveInput = inputHandler.MoveInput;
        lookInput = inputHandler.LookInput;

        HandleCameraLook();
        CheckGround();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleCameraLook()
    {
        float scaledSensitivity = mouseSensitivity * Time.deltaTime;
        yaw += lookInput.x * scaledSensitivity;
        pitch -= lookInput.y * scaledSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.eulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    private void HandleMovement()
    {
        if (moveInput == Vector2.zero) return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        moveDirection = transform.TransformDirection(moveDirection);

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        moveDirection *= currentSpeed;

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = moveDirection - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleJump()
    {
        if (!enableJump || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isGrounded = false;
    }

    private void HandleSprint(bool sprinting)
    {
        isSprinting = sprinting;
    }

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * 0.5f), transform.position.z);
        Vector3 direction = Vector3.down;
        float distance = 0.75f;

        isGrounded = Physics.Raycast(origin, direction, distance);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
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
    private bool isSprinting = false;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    public override void OnNetworkSpawn()
    {

        base.OnNetworkSpawn();

        if (IsOwner)
        {
            inputHandler = GetComponent<PlayerInputHandler>();
            LockCursor();
            rb = GetComponent<Rigidbody>();
            OnEnable();
        }
    }

    private void OnEnable()
    {
        inputHandler.OnJump += HandleJump;
        inputHandler.OnSprintStart += () => isSprinting = true;
        inputHandler.OnSprintEnd += () => isSprinting = false;
    }

    private void OnDisable()
    {
        if (!IsOwner) return;

        inputHandler.OnJump -= HandleJump;
        inputHandler.OnSprintStart -= () => isSprinting = true;
        inputHandler.OnSprintEnd -= () => isSprinting = false;
    }

    private void Update()
    {
        if (!IsOwner) return; // Ensure only the owner processes input

        moveInput = inputHandler.MoveInput; // Movement input
        lookInput = inputHandler.LookInput; // Look input

        HandleCameraLook(); // Handle camera look rotation
        CheckGround(); // Check if the player is grounded
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return; // Ensure only the owner processes physics-based movement

        HandleMovement(); // Handle movement
    }


    private void HandleCameraLook()
    {
        if (!IsOwner) return; // Only the owner updates camera look

        float scaledSensitivity = mouseSensitivity * Time.deltaTime;
        yaw += lookInput.x * scaledSensitivity; // Horizontal look
        pitch -= lookInput.y * scaledSensitivity; // Vertical look
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.eulerAngles = new Vector3(0, yaw, 0); // Rotate player horizontally
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0); // Rotate camera vertically
    }


    private void HandleMovement()
    {
        if (!IsOwner) return; // Only the owner updates Rigidbody movement

        Vector3 velocity = rb.velocity;

        if (moveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            moveDirection = transform.TransformDirection(moveDirection);

            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
            moveDirection *= currentSpeed;

            Vector3 velocityChange = moveDirection - velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // Slow down the Rigidbody when no input is provided
            rb.velocity = Vector3.zero;
        }
    }

    private void HandleJump()
    {
        if (!enableJump || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isGrounded = false;
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

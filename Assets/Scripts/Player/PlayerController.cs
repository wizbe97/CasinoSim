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
        if (!IsOwner)
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
            return;
        }
        else
        {
            base.OnNetworkSpawn();

            LockCursor();
            rb = GetComponent<Rigidbody>();
            inputHandler = GetComponent<PlayerInputHandler>();
            inputHandler.OnJump += HandleJump;
            inputHandler.OnSprintStart += () => isSprinting = true;
            inputHandler.OnSprintEnd += () => isSprinting = false;
        }
    }

    private void OnEnable()
    {
        if (!IsOwner) return;

        inputHandler.OnJump += HandleJump;
        inputHandler.OnSprintStart += () => isSprinting = true;
        inputHandler.OnSprintEnd += () => isSprinting = false;
    }

    private void OnDisable()
    {
        if (!IsOwner) return;

        inputHandler.OnJump -= HandleJump;
    }

    private void Update()
    {
        if (!IsOwner) return;

        moveInput = inputHandler.MoveInput;
        lookInput = inputHandler.LookInput;

        HandleCameraLook();
        CheckGround();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

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
            // Gradually slow down when no input is provided
            float dampingFactor = 5f;
            Vector3 deceleration = -velocity * dampingFactor * Time.fixedDeltaTime;

            if (velocity.magnitude < 0.1f)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.AddForce(deceleration, ForceMode.VelocityChange);
            }
        }
    }

    private void HandleJump()
    {
        if (!IsOwner || !enableJump || !isGrounded) return;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables

    #region Sprint

    public bool enableSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;

    private bool isSprinting = false;

    #endregion

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    private bool isCrouched = false;
    private Vector3 originalScale;

    #endregion

    #region Camera Variables

    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        #region Camera Look

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        transform.eulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);

        #endregion

        #region Sprint

        if (enableSprint && Input.GetKey(sprintKey))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        #endregion

        #region Jump

        if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch && Input.GetKeyDown(crouchKey))
        {
            ToggleCrouch();
        }

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
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        targetVelocity = transform.TransformDirection(targetVelocity);

        if (isSprinting)
        {
            targetVelocity *= sprintSpeed;
        }
        else
        {
            targetVelocity *= walkSpeed;
        }

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        if (isCrouched)
        {
            ToggleCrouch();
        }
    }

    private void ToggleCrouch()
    {
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
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

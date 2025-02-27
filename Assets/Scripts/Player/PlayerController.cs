using Photon.Pun;
using UnityEditor.VersionControl;
using UnityEngine;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
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

	// Network sync variables
	private Vector3 networkPosition;
	private Quaternion networkRotation;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		inputHandler = GetComponent<PlayerInputHandler>();

		if (photonView.IsMine)
		{
			UnLockCursor();
		}
		else
		{
			// Disable camera for other players
			playerCamera.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if (photonView.IsMine)
		{
			inputHandler.OnJump += HandleJump;
			inputHandler.OnSprintStart += () => isSprinting = true;
			inputHandler.OnSprintEnd += () => isSprinting = false;
		}
	}

	private void OnDisable()
	{
		if (photonView.IsMine)
		{
			inputHandler.OnJump -= HandleJump;
		}
	}

	private void Update()
	{
		if (!photonView.IsMine) return;

		if (FindObjectOfType<Chatsystem>())
		{
			if (!FindObjectOfType<Chatsystem>().canMove)
			{
				return;
			}
		}

		moveInput = inputHandler.MoveInput;
		lookInput = inputHandler.LookInput;

		HandleCameraLook();
		CheckGround();

		if (FindObjectOfType<InviteVan>().InviteObj.activeSelf || FindObjectOfType<Phone>())
		{
			UnLockCursor();
		}
		else
		{
			LockCursor();
		}
	}

	private void FixedUpdate()
	{
		if (photonView.IsMine)
		{
			HandleMovement();
		}
		else
		{
			// Smoothly interpolate position and rotation for remote players
			rb.position = Vector3.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * 10f);
			rb.rotation = Quaternion.Lerp(rb.rotation, networkRotation, Time.fixedDeltaTime * 10f);
		}
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
	private void UnLockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	// Photon synchronization
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			// Send position and rotation to other players
			stream.SendNext(rb.position);
			stream.SendNext(rb.rotation);
		}
		else
		{
			// Receive position and rotation from the network
			networkPosition = (Vector3)stream.ReceiveNext();
			networkRotation = (Quaternion)stream.ReceiveNext();
		}
	}
}

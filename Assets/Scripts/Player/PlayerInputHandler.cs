using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public delegate void InputActionEvent();
    public event InputActionEvent OnJump;
    public event InputActionEvent OnSprintStart;
    public event InputActionEvent OnSprintEnd;
    public event InputActionEvent OnRotatePreviewOrOpenBox;
    public event InputActionEvent OnPickupOrPlace;
    public event InputActionEvent OnBoxOrSell;
    public event InputActionEvent OnCancel;
    public event InputActionEvent OnPhoneMenu;
    public event InputActionEvent OnJoinTable;
    public event InputActionEvent OnDealCard;
    

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Movement
        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        // Look
        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        // Jump
        inputActions.Player.Jump.performed += ctx => OnJump?.Invoke();

        // Sprint
        inputActions.Player.Sprint.started += ctx => OnSprintStart?.Invoke();
        inputActions.Player.Sprint.canceled += ctx => OnSprintEnd?.Invoke();

        // Phone Menu
        inputActions.Player.PhoneMenu.performed += ctx => OnPhoneMenu?.Invoke();

        // RotatePreviewOrOpenBox
        inputActions.Player.RotatePreviewOrOpenBox.performed += ctx => OnRotatePreviewOrOpenBox?.Invoke();

        // PickupOrPlace
        inputActions.Player.PickupOrPlace.performed += ctx => OnPickupOrPlace?.Invoke();

        // BoxOrSell
        inputActions.Player.BoxOrSell.performed += ctx => OnBoxOrSell?.Invoke();

        // Cancel
        inputActions.Player.Cancel.performed += ctx => OnCancel?.Invoke();

        // JoinTable
        inputActions.Player.JoinTable.performed += ctx => OnJoinTable?.Invoke();

        // Deal Card
        inputActions.Player.DealCard.performed += ctx => OnDealCard?.Invoke();

    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
}

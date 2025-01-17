using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private LayerMask _interactableLayerMask;
    [SerializeField] private Transform _cameraTransform;

    [Header("References")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private PlacementManager _placementManager;
    [SerializeField] private GameManagerSO _gameManager;

    private PlayerInputHandler _inputHandler;
    private GameObject _heldBox;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
            OnEnable();
        }

    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        // Subscribe to input events
        _inputHandler.OnPhoneMenu += TogglePhoneMenu;
        _inputHandler.OnRotatePreviewOrOpenBox += HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace += HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell += HandleBoxOrSell;
        _inputHandler.OnCancel += HandleCancelPlacement;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        _inputHandler.OnPhoneMenu -= TogglePhoneMenu;
        _inputHandler.OnRotatePreviewOrOpenBox -= HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace -= HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell -= HandleBoxOrSell;
        _inputHandler.OnCancel -= HandleCancelPlacement;
    }

    private void TogglePhoneMenu()
    {
        if (!IsOwner) return;
        Debug.Log("Toggling phone menu");

        if (_uiManager.IsPhonePanelActive())
        {
            _uiManager.ResetPanelStates();
        }
        else
        {
            _uiManager.ShowPhonePanel();
        }
    }

    private void HandleRotateOrOpenBox()
    {
        if (!IsOwner) return;
        Debug.Log("Rotating or opening box");

        if (_placementManager.IsPlacing && _heldBox == null)
        {
            _placementManager.RotatePreview();
        }
        else if (_heldBox != null)
        {
            OpenHeldBox();
        }
    }

    private void HandlePickupOrPlace()
    {
        if (!IsOwner) return;
        if (_heldBox != null)
        {
            // Drop the box in the environment
            BoxPickup box = _heldBox.GetComponent<BoxPickup>();
            if (box != null)
            {
                box.Place();
                _heldBox = null;
            }
        }
        else if (IsLookingAtBox())
        {
            // Interact with a box in the environment
            InteractWithBox();
        }
        else if (_placementManager.IsPlacing && _placementManager.CanPlace)
        {
            // Place the currently selected item
            _placementManager.PlaceObject();
        }
        else if (!_placementManager.IsPlacing)
        {
            Debug.Log("Picking up object");
            _placementManager.TryPickUpObject();
        }
    }


    private void HandleBoxOrSell()
    {
        if (!IsOwner) return;
        Debug.Log("Box or sell");
        if (_heldBox != null)
        {
            SellItem();
        }
        else if (_placementManager.IsPlacing)
        {
            _placementManager.BoxCurrentPreview();
        }
    }

    public void PickupBox(GameObject box)
    {
        if (!IsOwner) return;
        Debug.Log("Picking up box");
        _heldBox = box;
    }

    private void HandleCancelPlacement()
    {
        if (!IsOwner) return;
        Debug.Log("Cancelling placement");

        if (_heldBox != null)
        {
            // Drop the box
            BoxPickup box = _heldBox.GetComponent<BoxPickup>();
            if (box != null)
            {
                box.Place();
                _heldBox = null;
            }
        }
        else if (_placementManager.IsPlacing)
            _placementManager.CancelPlacement();
    }

    private bool IsLookingAtBox()
    {
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactableLayerMask))
        {
            return hit.collider.GetComponent<BoxPickup>() != null;
        }

        return false;
    }

    private void InteractWithBox()
    {
        if (!IsOwner) return;
        Debug.Log("Interacting with box");

        if (_heldBox == null)
        {
            Ray ray = _placementManager.PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _placementManager.ReticleUI.position));

            if (Physics.Raycast(ray, out RaycastHit hit, _placementManager.MaxPlacementDistance, _placementManager.PlacedObjectLayerMask))
            {
                BoxPickup box = hit.collider.GetComponent<BoxPickup>();
                if (box != null)
                {
                    _heldBox = hit.collider.gameObject;
                    box.Pickup(_placementManager.PlayerTransform);
                }
            }
        }
        else
        {
            BoxPickup box = _heldBox.GetComponent<BoxPickup>();
            if (box != null)
            {
                box.Place();
                _heldBox = null;
            }
        }
    }

    private void OpenHeldBox()
    {
        if (!IsOwner) return;
        Debug.Log("Opening held box");

        BoxPickup box = _heldBox.GetComponent<BoxPickup>();
        if (box != null)
        {
            PlaceableItemSO itemInside = box.GetContainedItem();
            if (itemInside != null)
            {
                _placementManager.StartPlacement(itemInside);
                Destroy(_heldBox);
                _heldBox = null;
            }
        }
    }

    private void SellItem()
    {
        if (!IsOwner) return;
        Debug.Log("Selling item");

        if (_heldBox == null) return;

        BoxPickup box = _heldBox.GetComponent<BoxPickup>();
        if (box == null) return;

        PlaceableItemSO itemInside = box.GetContainedItem();
        if (itemInside == null) return;

        int salePrice = Mathf.FloorToInt(itemInside.Price / 2f);
        _gameManager.playerBalanceManager.AddBalance(salePrice);

        Destroy(_heldBox);
        _heldBox = null;

        Debug.Log($"Sold {itemInside.name} for ${salePrice}.");
    }
}

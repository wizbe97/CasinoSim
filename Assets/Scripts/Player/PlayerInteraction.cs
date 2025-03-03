using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private LayerMask _interactableLayerMask;
    [SerializeField] private RectTransform _reticleUI;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask _placementLayerMask;
    [SerializeField] private LayerMask _collisionLayerMask;
    [SerializeField] private LayerMask _placedObjectLayerMask;
    [SerializeField] private float _maxPlacementDistance = 10f;

    [Header("References")]
    [SerializeField] PlayerUI _playerUI;
    [SerializeField] private GameManagerSO _gameManager;
    [SerializeField] private GameEventSO onBalanceChangedEvent;

    private PlayerInputHandler _inputHandler;
    private PlayerController _playerController;
    private GameObject _currentPreview;
    private GameObject _pickedUpObject;
    private PlaceableItemSO _currentItem;
    private GameObject _heldBox;
    private bool _isPlacing = false;
    private bool _canPlace = false;

    #region Public Properties
    public bool IsPlacing => _isPlacing;
    public bool CanPlace => _canPlace;
    #endregion

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _reticleUI = GetComponentInChildren<RectTransform>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
		if (!PhotonNetwork.IsMasterClient)
		{
            return;
		}

		_inputHandler.OnPhoneMenu += TogglePhoneMenu;
        _inputHandler.OnRotatePreviewOrOpenBox += HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace += HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell += HandleBoxOrSell;
        _inputHandler.OnCancel += HandleCancelPlacement;
    }

    private void OnDisable()
    {
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		_inputHandler.OnPhoneMenu -= TogglePhoneMenu;
        _inputHandler.OnRotatePreviewOrOpenBox -= HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace -= HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell -= HandleBoxOrSell;
        _inputHandler.OnCancel -= HandleCancelPlacement;
    }

    private void Update()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_isPlacing && _currentPreview != null)
        {
            UpdatePreviewPosition();
        }
    }

    private void TogglePhoneMenu()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_playerUI.IsPhonePanelActive())
        {
            _playerUI.ResetPanelStates();
        }
        else
        {
            _playerUI.ShowPhonePanel();
        }
    }


    private void HandleRotateOrOpenBox()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_isPlacing && _heldBox == null)
        {
            RotatePreview();
        }
        else if (_heldBox != null)
        {
            OpenHeldBox();
        }
    }

    private void HandlePickupOrPlace()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

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
            InteractWithBox();
        }
        else if (_isPlacing && _canPlace)
        {
            PlaceObject();
        }
        else if (!_isPlacing)
        {
            TryPickUpObject();
        }
    }

    private void HandleBoxOrSell()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_heldBox != null)
        {
            SellItem();
        }
        else if (_isPlacing)
        {
            BoxCurrentPreview();
        }
    }

    private void HandleCancelPlacement()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

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
        else if (_isPlacing)
        {
            CancelPlacement();
        }
    }

    #region Placement Logic

    public void StartPlacement(PlaceableItemSO item)
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (item == null) return;

        _currentItem = item;
        _isPlacing = true;

        if (_pickedUpObject != null)
        {
            _currentPreview = PhotonNetwork.Instantiate(_currentItem.GetPreviewPrefab().name, _pickedUpObject.transform.position, _pickedUpObject.transform.rotation);
            PhotonNetwork.Destroy(_pickedUpObject);
            _pickedUpObject = null;
        }
        else
        {
            _currentPreview = PhotonNetwork.Instantiate(_currentItem.GetPreviewPrefab().name, Vector3.zero, Quaternion.identity);
        }
    }

    private void TryPickUpObject()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		Ray ray = PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _reticleUI.position));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placedObjectLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            PlacedItem placedItem = hitObject.GetComponentInParent<PlacedItem>();
            if (placedItem != null)
            {
                float distance = Vector3.Distance(transform.position, placedItem.transform.position);

                if (distance <= _maxPlacementDistance && placedItem.CanBePickedUp())
                {
                    _pickedUpObject = placedItem.gameObject;
                    _currentItem = placedItem.GetPlaceableItem();

                    StartPlacement(_currentItem);
                }
            }
        }
    }

    private void UpdatePreviewPosition()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, _reticleUI.position);
        Ray ray = PlayerCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placementLayerMask))
        {
            Vector3 newPosition = hit.point;
            float gridSize = 1.0f;
            newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
            newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;

            Vector3 directionFromPlayer = newPosition - transform.position;
            if (directionFromPlayer.magnitude > _maxPlacementDistance)
            {
                directionFromPlayer = directionFromPlayer.normalized * _maxPlacementDistance;
                newPosition = transform.position + directionFromPlayer;

                if (Physics.Raycast(newPosition + Vector3.up * 5f, Vector3.down, out RaycastHit clampHit, Mathf.Infinity, _placementLayerMask))
                {
                    newPosition.y = clampHit.point.y;
                }
            }

            _currentPreview.transform.position = newPosition;

            BoxCollider collider = _currentPreview.GetComponent<BoxCollider>();
            Collider[] colliders = Physics.OverlapBox(
                newPosition,
                collider.size / 2,
                _currentPreview.transform.rotation,
                _collisionLayerMask
            );

            _canPlace = colliders.Length == 0;
            UpdatePreviewMaterial(_canPlace);
        }
        else
        {
            _canPlace = false;
            UpdatePreviewMaterial(false);
        }
    }

    private void UpdatePreviewMaterial(bool isValid)
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_currentPreview != null)
        {
            Renderer[] renderers = _currentPreview.GetComponentsInChildren<Renderer>();
            Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = color;
            }
        }
    }

    public void RotatePreview()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_currentPreview != null)
        {
            _currentPreview.transform.Rotate(0, 22.5f, 0);
        }
    }

    public void PlaceObject()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (!_canPlace || _currentItem == null) return;

        GameObject placedObject = PhotonNetwork.Instantiate(_currentItem.GetPlacedPrefab().name, _currentPreview.transform.position, _currentPreview.transform.rotation);

        var placedItemScript = placedObject.GetComponent<PlacedItem>();
        placedItemScript.Initialize(_currentItem, _currentItem.GetPlacementCooldown());

        PhotonNetwork.Destroy(_currentPreview);
        ResetPlacementState();
    }

    public void CancelPlacement()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_currentPreview != null)
        {
            Destroy(_currentPreview);
        }
        ResetPlacementState();
    }

    private void BoxCurrentPreview()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_currentPreview != null && _currentItem != null)
        {
            GameObject box = PhotonNetwork.Instantiate(_currentItem.GetCardboardBoxPrefab().name, _currentPreview.transform.position, _currentPreview.transform.rotation);

            // Ensure the Rigidbody is kinematic before picking up
            if (box.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }

            if (box.TryGetComponent(out BoxPickup boxPickup))
            {
                boxPickup.SetContainedItem(_currentItem);

                _heldBox = box;

                boxPickup.Pickup(transform);
            }

            PhotonNetwork.Destroy(_currentPreview);
            ResetPlacementState();
        }
    }


    private void ResetPlacementState()
    {
        _currentPreview = null;
        _currentItem = null;
        _isPlacing = false;
    }

    #endregion

    private Camera PlayerCamera => GetComponentInChildren<Camera>();

    private void OpenHeldBox()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_heldBox != null && _heldBox.TryGetComponent(out BoxPickup box))
        {
            PlaceableItemSO itemInside = box.GetContainedItem();
            if (itemInside != null)
            {
                StartPlacement(itemInside);
                Destroy(_heldBox);
                _heldBox = null;
            }
        }
    }

    private void SellItem()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_heldBox == null) return;

        if (_heldBox.TryGetComponent(out BoxPickup box))
        {
            PlaceableItemSO itemInside = box.GetContainedItem();
            if (itemInside == null) return;

            int salePrice = Mathf.FloorToInt(itemInside.Price / 2f);
            _gameManager.playerBalanceManager.AddBalance(salePrice);

            Destroy(_heldBox);
            _heldBox = null;

            Debug.Log($"Sold {itemInside.name} for ${salePrice}.");
        }
    }

    private bool IsLookingAtBox()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return false;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return false;
		}

		Ray ray = PlayerCamera.ScreenPointToRay(_reticleUI.position);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactableLayerMask))
        {
            return hit.collider.GetComponent<BoxPickup>() != null;
        }

        return false;
    }

    private void InteractWithBox()
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		if (_heldBox == null)
        {
            Ray ray = PlayerCamera.ScreenPointToRay(_reticleUI.position);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxPlacementDistance, _placedObjectLayerMask))
            {
                if (hit.collider.TryGetComponent(out BoxPickup box))
                {
                    _heldBox = hit.collider.gameObject;
                    box.Pickup(transform);
                }
            }
        }
        else
        {
            if (_heldBox.TryGetComponent(out BoxPickup box))
            {
                box.Place();
                _heldBox = null;
            }
        }
    }

    public void SetReticleVisibility(bool isVisible)
    {
        if (_reticleUI != null)
        {
            _reticleUI.gameObject.SetActive(isVisible);
        }
    }

    public void DisableMovement()
    {
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }
    }

    public void EnableMovement()
    {
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
    }

    public void BuyItem(PlaceableItemSO item)
    {
		if (FindObjectOfType<Chatsystem>().isTyping)
		{
			return;
		}

		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}

		var balanceManager = _gameManager.playerBalanceManager;

        if (balanceManager.CanAfford(item.Price))
        {
            balanceManager.DeductBalance(item.Price);

            onBalanceChangedEvent.Raise();
            _playerUI.ResetPanelStates();

            DeliveryVehicleManager.Instance.SpawnDeliveryVehicle(item.GetCardboardBoxPrefab());
        }

    }
}

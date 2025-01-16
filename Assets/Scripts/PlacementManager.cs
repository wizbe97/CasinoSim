using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private RectTransform _reticleUI;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerInteraction _playerInteraction;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask _placementLayerMask;
    [SerializeField] private LayerMask _collisionLayerMask;
    [SerializeField] private LayerMask _placedObjectLayerMask;

    [Header("Placement Settings")]
    [SerializeField] private float _maxPlacementDistance = 10f;

    [Header("Available Items")]
    [SerializeField] private PlaceableItemSO[] _placeableItems;

    private GameObject _currentPreview;
    private GameObject _pickedUpObject;
    private PlaceableItemSO _currentItem;
    private bool _isPlacing = false;
    private bool _canPlace = false;

    #region Public Properties
    public bool IsPlacing => _isPlacing;
    public bool CanPlace => _canPlace;
    public Camera PlayerCamera => _playerCamera;
    public float MaxPlacementDistance => _maxPlacementDistance;
    public LayerMask PlacedObjectLayerMask => _placedObjectLayerMask;
    public Transform PlayerTransform => _playerTransform;
    public RectTransform ReticleUI => _reticleUI;

    #endregion

    private void Start()
    {
        InitializePredefinedItems();
    }

    private void Update()
    {
        if (_isPlacing && _currentPreview != null)
        {
            UpdatePreviewPosition();
        }
    }

    private void InitializePredefinedItems()
    {
        GameObject[] predefinedItems = GameObject.FindGameObjectsWithTag("PlacedObject");

        foreach (GameObject item in predefinedItems)
        {
            PlacedItem placedItem = item.GetComponent<PlacedItem>();
            if (placedItem != null && placedItem.GetPlaceableItem() == null)
            {
                if (item.name.Contains("BlackJack_Table"))
                {
                    placedItem.Initialize(FindPlaceableItemSO(PlaceableItemType.BlackjackTable), 0);
                }
                else if (item.name.Contains("Roulette_Table"))
                {
                    placedItem.Initialize(FindPlaceableItemSO(PlaceableItemType.RouletteTable), 0);
                }
            }
        }
    }

    public void TryPickUpObject()
    {
        Ray ray = _playerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _reticleUI.position));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placedObjectLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            PlacedItem placedItem = hitObject.GetComponentInParent<PlacedItem>();
            if (placedItem != null)
            {
                float distance = Vector3.Distance(_playerTransform.position, placedItem.transform.position);
                if (distance <= _maxPlacementDistance && placedItem.CanBePickedUp())
                {
                    _pickedUpObject = placedItem.gameObject;
                    _currentItem = placedItem.GetPlaceableItem();
                    StartPlacement(_currentItem);
                }
            }
        }
    }

    public void BoxCurrentPreview()
    {
        if (_currentItem == null || _currentPreview == null || !_isPlacing)
            return;

        // Create a box containing the current item
        GameObject box = Instantiate(_currentItem.GetCardboardBoxPrefab(), _currentPreview.transform.position, Quaternion.identity);
        if (box == null)
            return;

        // Assign the current item to the box
        BoxPickup boxPickup = box.GetComponent<BoxPickup>();
        if (boxPickup == null)
        {
            Destroy(box); // Clean up the improperly created box
            return;
        }

        // Prevent the box from falling or interacting with physics
        Rigidbody boxRigidbody = box.GetComponent<Rigidbody>();
        if (boxRigidbody != null)
        {
            boxRigidbody.isKinematic = true;
        }

        boxPickup.SetContainedItem(_currentItem);

        // Clean up the current preview
        Destroy(_currentPreview);
        _currentPreview = null;

        // Cancel placement mode
        _isPlacing = false;
        _playerInteraction.PickupBox(box);
        boxPickup.Pickup(_playerTransform); // Use the existing logic to attach the box to the player
    }


    public void StartPlacement(PlaceableItemSO item)
    {
        if (item == null)
            return;

        _currentItem = item;
        _isPlacing = true;

        if (_pickedUpObject != null)
        {
            _currentPreview = Instantiate(_currentItem.GetPreviewPrefab(), _pickedUpObject.transform.position, _pickedUpObject.transform.rotation);
            Destroy(_pickedUpObject);
            _pickedUpObject = null;
        }
        else
        {
            _currentPreview = Instantiate(_currentItem.GetPreviewPrefab());
        }
    }


    private void UpdatePreviewPosition()
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, _reticleUI.position);
        Ray ray = _playerCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placementLayerMask))
        {
            Vector3 newPosition = hit.point;
            float gridSize = 1.0f;
            newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
            newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;

            Vector3 directionFromPlayer = newPosition - _playerTransform.position;
            if (directionFromPlayer.magnitude > _maxPlacementDistance)
            {
                directionFromPlayer = directionFromPlayer.normalized * _maxPlacementDistance;
                newPosition = _playerTransform.position + directionFromPlayer;

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
        if (_currentPreview != null)
        {
            _currentPreview.transform.Rotate(0, 22.5f, 0);
        }
    }

    public void PlaceObject()
    {
        if (!_canPlace || _currentItem == null)
            return;

        GameObject placedObject = Instantiate(_currentItem.GetPlacedPrefab(), _currentPreview.transform.position, _currentPreview.transform.rotation);

        var placedItemScript = placedObject.GetComponent<PlacedItem>();
        placedItemScript.Initialize(_currentItem, _currentItem.GetPlacementCooldown());

        CancelPlacement();
    }

    public void CancelPlacement()
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
        }
        _currentPreview = null;
        _currentItem = null;
        _isPlacing = false;
    }

    private PlaceableItemSO FindPlaceableItemSO(PlaceableItemType itemType)
    {
        foreach (PlaceableItemSO item in _placeableItems)
        {
            if (item.ItemType == itemType)
            {
                return item;
            }
        }

        return null;
    }


}

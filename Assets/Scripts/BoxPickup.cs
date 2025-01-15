using UnityEngine;

public class BoxPickup : MonoBehaviour
{
    private bool _isPickedUp = false;
    private Transform _playerTransform;
    private Rigidbody _rigidbody;

    [Header("Attachment Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 2f);
    [SerializeField] private Vector3 _rotation = Vector3.zero;

    [Header("Box Contents")]
    [SerializeField] private PlaceableItemSO _containedItem;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            Debug.LogError($"BoxPickup on {gameObject.name} requires a Rigidbody component.");
        }
    }

    public void Pickup(Transform playerTransform)
    {
        if (_isPickedUp) return;

        _isPickedUp = true;
        _playerTransform = playerTransform;

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }

        transform.SetParent(_playerTransform);
        transform.localPosition = _offset;
        transform.localRotation = Quaternion.Euler(_rotation);
    }

    public void Place()
    {
        if (!_isPickedUp) return;

        _isPickedUp = false;

        transform.SetParent(null);

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }
    }

    public PlaceableItemSO GetContainedItem()
    {
        return _containedItem;
    }

    public void SetContainedItem(PlaceableItemSO item)
    {
        _containedItem = item;
    }
}

using UnityEngine;

public class BoxPickup : MonoBehaviour
{
    private bool _isPickedUp = false; // Tracks whether the box is currently picked up
    private Transform _playerTransform; // Reference to the player's transform
    private Rigidbody _rigidbody; // Reference to the box's Rigidbody

    [Header("Attachment Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 2f); // Offset relative to the player
    [SerializeField] private Vector3 _rotation = Vector3.zero; // Fixed rotation when held

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

        // Disable physics
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }

        // Attach to player
        transform.SetParent(_playerTransform);
        transform.localPosition = _offset; // Set position relative to player
        transform.localRotation = Quaternion.Euler(_rotation); // Set rotation
        Debug.Log($"BoxPickup: {gameObject.name} picked up and attached to the player.");
    }

    public void Place()
    {
        if (!_isPickedUp) return;

        _isPickedUp = false;

        // Detach from player
        transform.SetParent(null);

        // Enable physics
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }

        Debug.Log($"BoxPickup: {gameObject.name} placed down.");
    }
}

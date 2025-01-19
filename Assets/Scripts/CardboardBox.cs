using UnityEngine;

public class CardboardBox : MonoBehaviour
{
    private PlaceableItemSO _item;
    private Transform _playerHoldPoint;
    private bool _isBeingHeld = false;

    public void Initialize(PlaceableItemSO item)
    {
        _item = item;
    }

    public PlaceableItemSO GetItem()
    {
        return _item;
    }

    public void SetPlayerHoldPoint(Transform holdPoint)
    {
        _playerHoldPoint = holdPoint;
    }

    public void PickUp()
    {
        if (_playerHoldPoint == null)
        {
            Debug.LogError("Player hold point not set!");
            return;
        }

        _isBeingHeld = true;
        transform.SetParent(_playerHoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void Drop()
    {
        _isBeingHeld = false;
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public void OpenBox(PlayerInteraction playerInteraction)
    {
        if (_isBeingHeld && _item != null)
        {
            playerInteraction.StartPlacement(_item);
            _item = null;
            Drop();
            Destroy(gameObject);
        }
        else if (_item == null)
        {
            Debug.LogWarning("The box is empty!");
        }
    }

    public bool IsBeingHeld()
    {
        return _isBeingHeld;
    }
}

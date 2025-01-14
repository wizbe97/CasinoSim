using UnityEngine;

public class PlacedItem : MonoBehaviour
{
    private PlaceableItemSO _itemData;

    // Initialize the item with its associated ScriptableObject and type
    public void Initialize(PlaceableItemSO itemData, float cooldown)
    {
        _itemData = itemData;
        PickupCooldown = Time.time + cooldown;
    }

    public float PickupCooldown { get; private set; }

    public PlaceableItemSO GetPlaceableItem() => _itemData;

    public PlaceableItemType GetItemType()
    {
        if (_itemData == null)
        {
            throw new System.NullReferenceException($"_itemData is null on {gameObject.name}. Ensure a PlaceableItemSO is assigned.");
        }
        return _itemData.ItemType;
    }

    public bool CanBePickedUp()
    {
        return Time.time >= PickupCooldown;
    }
}

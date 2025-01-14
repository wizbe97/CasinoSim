using UnityEngine;

[CreateAssetMenu(fileName = "PlaceableItem", menuName = "PlaceableItems/PlaceableItem")]
public class PlaceableItemSO : ScriptableObject
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _previewPrefab;
    [SerializeField] private GameObject _placedPrefab;

    [Header("Settings")]
    [SerializeField] private PlaceableItemType _itemType;
    [SerializeField] private int _price;
    [SerializeField] private float _placementCooldown = 0.25f;

    public GameObject GetPreviewPrefab() => _previewPrefab;
    public GameObject GetPlacedPrefab() => _placedPrefab;
    public float GetPlacementCooldown() => _placementCooldown;
    public PlaceableItemType ItemType => _itemType;
    public int Price => _price;

}

public enum PlaceableItemType
{
    Box,
    Chair,
    Table,
    BlackjackTable,
    RouletteTable,
}

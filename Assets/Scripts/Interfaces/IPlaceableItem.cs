using UnityEngine;

public interface IPlaceableItem
{
    GameObject GetPreviewPrefab();
    GameObject GetPlacedPrefab();
    float GetPlacementCooldown();
}

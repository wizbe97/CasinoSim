using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiManagerPrefab;
    [SerializeField] private GameObject _deliveryVehicleManagerPrefab;
    [SerializeField] private GameObject _placementManagerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform _playerSpawnPoint;

    [Header("Placeable Items")]
    [SerializeField] private PlaceableItemSO[] _placeableItems;


    private void Start()
    {
        // SpawnPlayer();
        // SpawnManagers();
        InitializePredefinedItems();
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
    private void SpawnPlayer()
    {
        if (_playerPrefab == null || _playerSpawnPoint == null)
        {
            Debug.LogError("Player prefab or spawn point is not assigned in the GameManager.");
            return;
        }

        Instantiate(_playerPrefab, _playerSpawnPoint.position, _playerSpawnPoint.rotation);
    }

    private void SpawnManagers()
    {
        if (_uiManagerPrefab == null)
        {
            Debug.LogError("UI Manager prefab is not assigned in the GameManager.");
        }
        else
        {
            Instantiate(_uiManagerPrefab);
        }

        if (_deliveryVehicleManagerPrefab == null)
        {
            Debug.LogError("Delivery Vehicle Manager prefab is not assigned in the GameManager.");
        }
        else
        {
            Instantiate(_deliveryVehicleManagerPrefab);
        }

        if (_placementManagerPrefab == null)
        {
            Debug.LogError("Placement Manager prefab is not assigned in the GameManager.");
        }
        else
        {
            Instantiate(_placementManagerPrefab);
        }
    }
}

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _deliveryVehicleManagerPrefab;
    [SerializeField] private GameObject _purchaseManagerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform _playerSpawnPoint;

    [Header("Placeable Items")]
    [SerializeField] private PlaceableItemSO[] _placeableItems;

    private void Awake()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate GameManager detected and destroyed on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keeps the instance alive across scenes
    }

    private void Start()
    {
        SpawnManagers();
        SpawnPlayer();
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
        if (_deliveryVehicleManagerPrefab == null)
        {
            Debug.LogError("Delivery Vehicle Manager prefab is not assigned in the GameManager.");
        }
        else
        {
            Instantiate(_deliveryVehicleManagerPrefab);
        }

        if (_purchaseManagerPrefab == null)
        {
            Debug.LogError("Purchase Manager prefab is not assigned in the GameManager.");
        }
        else
        {
            Instantiate(_purchaseManagerPrefab);
        }
    }
}

using UnityEngine;

public class PurchaseManager : MonoBehaviour
{
    public static PurchaseManager Instance { get; private set; } // Singleton instance

    [Header("Game Manager")]
    [SerializeField] private GameManagerSO _gameManager;

    [Header("Events")]
    [SerializeField] private GameEventSO onBalanceChangedEvent;


    private void Start()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Duplicate PurchaseManager detected and destroyed on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keeps the instance alive across scenes

    }

    public void BuyItem(PlaceableItemSO item)
    {
        // Access the player-specific balance manager
        var balanceManager = _gameManager.playerBalanceManager;

        if (balanceManager.CanAfford(item.Price))
        {
            // Deduct balance
            balanceManager.DeductBalance(item.Price);

            // Notify other systems about the balance change
            onBalanceChangedEvent.Raise();

            // Spawn the delivery vehicle
            DeliveryVehicleManager.Instance.SpawnDeliveryVehicle(item.GetCardboardBoxPrefab());

            Debug.Log($"Item purchased: {item.name} for ${item.Price}.");
        }
        else
        {
            Debug.LogWarning("Insufficient balance to purchase this item.");
        }
    }
}

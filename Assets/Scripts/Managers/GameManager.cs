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

    private void Start()
    {
        SpawnPlayer();
        SpawnManagers();
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

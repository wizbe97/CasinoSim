using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
	public static GameManager Instance { get; private set; }

	[Header("Prefabs")]
	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private GameObject _deliveryVehicleManagerPrefab;

	[Header("Spawn Points")]
	[SerializeField] private Transform _playerSpawnPoint;

	[Header("Placeable Items")]
	[SerializeField] private PlaceableItemSO[] _placeableItems;


	private void Awake()
	{
		/*	if (Instance != null && Instance != this)
			{
				Debug.LogWarning($"Duplicate GameManager detected and destroyed on {gameObject.name}");
				Destroy(gameObject);
				return;
			}

			Instance = this;
			DontDestroyOnLoad(gameObject); */
	}

	private void Start()
	{
		if (!PhotonNetwork.IsConnected)
		{
			Debug.LogError("Not connected to Photon Network.");
			return;
		}

		if (PhotonNetwork.IsMasterClient)
		{
			SpawnManagers();
		}

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

		// Instantiate player object across the network
		PhotonNetwork.Instantiate(_playerPrefab.name, _playerSpawnPoint.position, _playerSpawnPoint.rotation);
	}

	private void SpawnManagers()
	{
		if (_deliveryVehicleManagerPrefab == null)
		{
			Debug.LogError("Delivery Vehicle Manager prefab is not assigned in the GameManager.");
		}
		else
		{
			// Only the MasterClient spawns the manager
			PhotonNetwork.InstantiateRoomObject(_deliveryVehicleManagerPrefab.name, Vector3.zero, Quaternion.identity);
		}
	}

}

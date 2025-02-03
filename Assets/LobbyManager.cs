using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Steamworks;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	public GameObject roomButtonPrefab; // Prefab for room button
	public Transform roomListContainer; // Parent UI panel to hold room buttons

	private List<GameObject> roomButtons = new List<GameObject>();

	void Start()
	{
		if (!PhotonNetwork.IsConnected)
		{
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby(TypedLobby.Default);

		// Create a test room if none exist
		if (PhotonNetwork.CountOfRooms == 0)
		{
			RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 4 };
			PhotonNetwork.CreateRoom("TestRoom", roomOptions);
			Debug.Log("Creating a test room...");
		}
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		Debug.Log("Received Room List Update: " + roomList.Count + " rooms found.");
		if (roomList == null || roomList.Count == 0)
		{
			Debug.Log("No rooms available.");
			return;
		}

		UpdateRoomList(roomList);
	}

	void UpdateRoomList(List<RoomInfo> roomList)
	{
		// Clear previous room buttons
		foreach (GameObject button in roomButtons)
		{
			Destroy(button);
		}
		roomButtons.Clear();

		// Populate new room list
		foreach (RoomInfo room in roomList)
		{
			//if (room.RemovedFromList || room.PlayerCount == 0)
				//continue;

			// Debugging log
			Debug.Log("Adding Room: " + room.Name + " | Players: " + room.PlayerCount + "/" + room.MaxPlayers);

			GameObject roomButton = Instantiate(roomButtonPrefab, roomListContainer);
			roomButton.transform.GetChild(1).GetComponent<Text>().text = room.Name;
			roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
			roomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));
			roomButtons.Add(roomButton);
		}
	}

	void JoinRoom(string roomName)
	{
		PhotonNetwork.JoinRoom(roomName);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
		PhotonNetwork.LoadLevel(0);
	}

	public void ReturnMyWorld()
	{
		string username = SteamFriends.GetPersonaName();
		PhotonNetwork.CreateRoom(username + "s' room");
	}
}
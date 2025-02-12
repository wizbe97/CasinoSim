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
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		// Clear previous room buttons
		for (int i = 0; i < roomButtons.Count; i++) {
			Destroy(roomButtons[i]);
		}

		string myUsername = SteamFriends.GetPersonaName(); // Get Steam username

		Debug.Log("Received Room List Update: " + roomList.Count + " rooms found.");

		// Populate new room list
		foreach (RoomInfo room in roomList)
		{
			if (room.RemovedFromList || room.PlayerCount == 0 || room.Name.Contains(myUsername))
			{
				return; // Skip the room if it includes your username
			}

			GameObject roomButton = Instantiate(roomButtonPrefab, roomListContainer);
			roomButton.transform.GetChild(1).GetComponent<Text>().text = room.Name;
			roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
			roomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));
			roomButtons.Add(roomButton);

			// Debugging log
			Debug.Log("Adding Room: " + room.Name + " | Players: " + room.PlayerCount + "/" + room.MaxPlayers);
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
		PhotonNetwork.JoinOrCreateRoom(username + "s' room", new RoomOptions { MaxPlayers = 4, IsOpen = true, IsVisible = true}, TypedLobby.Default);
	}
}
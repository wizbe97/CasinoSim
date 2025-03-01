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
		string myUsername = SteamFriends.GetPersonaName(); // Get Steam username
		Debug.Log("Received Room List Update: " + roomList.Count + " rooms found.");

		if (roomList == null || roomList.Count == 0)
		{
			Debug.Log("No available rooms.");
			ClearRoomButtons();
			return;
		}

		// Dictionary to track existing buttons
		Dictionary<string, GameObject> roomButtonsDict = new Dictionary<string, GameObject>();

		foreach (GameObject button in roomButtons)
		{
			string roomName = button.transform.GetChild(1).GetComponent<Text>().text;
			roomButtonsDict[roomName] = button;
		}

		List<GameObject> buttonsToRemove = new List<GameObject>();

		foreach (RoomInfo room in roomList)
		{
			if (room.RemovedFromList || room.PlayerCount == 0 || room.Name.Contains(myUsername))
			{
				// Mark button for removal if the room is no longer valid
				if (roomButtonsDict.ContainsKey(room.Name))
				{
					buttonsToRemove.Add(roomButtonsDict[room.Name]);
					roomButtonsDict.Remove(room.Name);
				}
				continue; // Skip this room
			}

			if (roomButtonsDict.ContainsKey(room.Name))
			{
				// Update existing room button
				GameObject roomButton = roomButtonsDict[room.Name];
				roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
			}
			else
			{
				// Create new room button
				GameObject roomButton = Instantiate(roomButtonPrefab, roomListContainer);
				roomButton.transform.GetChild(1).GetComponent<Text>().text = room.Name;
				roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
				roomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));

				roomButtons.Add(roomButton);
				roomButtonsDict[room.Name] = roomButton;
			}

			Debug.Log("Adding/Updating Room: " + room.Name + " | Players: " + room.PlayerCount + "/" + room.MaxPlayers);
		}

		// Remove any buttons that no longer correspond to an active room
		foreach (GameObject button in buttonsToRemove)
		{
			roomButtons.Remove(button);
			Destroy(button);
		}
	}

	// Helper function to clear all buttons
	void ClearRoomButtons()
	{
		foreach (GameObject button in roomButtons)
		{
			Destroy(button);
		}
		roomButtons.Clear();
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

	public void RefreshRoomList()
	{
		if (PhotonNetwork.InLobby)
		{
			PhotonNetwork.LeaveLobby();
			PhotonNetwork.JoinLobby();
			Debug.Log("Refreshing Room List...");
		}
		else
		{
			Debug.Log("Not in a lobby. Joining now...");
			PhotonNetwork.JoinLobby();
		}
	}
}
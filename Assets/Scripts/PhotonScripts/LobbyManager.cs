using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Steamworks;
using System.Linq;

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
		Debug.Log("Received Room List Update: " + roomList.Count + " rooms found.");
		UpdateRoomList(roomList);
	}

	void UpdateRoomList(List<RoomInfo> roomList)
	{
		string myUsername = SteamFriends.GetPersonaName(); // Get Steam username  

		if (roomList == null || roomList.Count == 0)
		{
			Debug.Log("No available rooms.");
			foreach (GameObject button in roomButtons)
			{
				Destroy(button);
			}
			roomButtons.Clear();
			return;
		}

		// Create a dictionary to track existing room buttons
		Dictionary<string, GameObject> existingButtons = new Dictionary<string, GameObject>();

		foreach (GameObject button in roomButtons)
		{
			string roomName = button.transform.GetChild(1).GetComponent<Text>().text;
			existingButtons[roomName] = button;
		}

		// Update or create new room buttons
		foreach (RoomInfo room in roomList)
		{
			if (room.RemovedFromList || room.PlayerCount == 0)
				continue; // Skip removed or empty rooms

			if (existingButtons.ContainsKey(room.Name))
			{
				// Update existing button
				GameObject roomButton = existingButtons[room.Name];
				roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
			}
			else
			{
				// Create new button
				GameObject roomButton = Instantiate(roomButtonPrefab, roomListContainer);
				roomButton.transform.GetChild(1).GetComponent<Text>().text = room.Name;
				roomButton.transform.GetChild(2).GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
				roomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));

				roomButtons.Add(roomButton);
			}
		}

		// Remove buttons for rooms that no longer exist
		foreach (var roomName in existingButtons.Keys)
		{
			if (!roomList.Any(room => room.Name == roomName))
			{
				Destroy(existingButtons[roomName]);
				roomButtons.Remove(existingButtons[roomName]);
			}
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Steamworks;
using Photon.Realtime;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
	public GameObject Offline_cam = null;
	public GameObject GameManager;

	private Dictionary<string, RoomInfo> roomList = new Dictionary<string, RoomInfo>();


	private void Start()
	{
		if (PhotonNetwork.InRoom)
		{
			Offline_cam.SetActive(false);
			GameManager.SetActive(true);
		}
		else
		{
			Offline_cam.SetActive(true);
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	private void Update()
	{
	}

	public override void OnConnectedToMaster()
	{
		// Join the lobby to receive room list updates
		PhotonNetwork.JoinLobby(TypedLobby.Default);
	}

	public override void OnJoinedLobby()
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = false; // Ensure the room is visible
		roomOptions.IsOpen = false; // Ensure the room is open
		roomOptions.MaxPlayers = 4;

		string username = SteamFriends.GetPersonaName();
		if (username != null)
		{
			PhotonNetwork.CreateRoom(username + Random.Range(221, 132) + "'s Room", roomOptions);
		}
		else
		{
			PhotonNetwork.CreateRoom("Quest" + Random.Range(221, 132) + "'s Room", roomOptions);
		}
	}

	// Check if a room exists by name
	public bool DoesRoomExist(string roomName)
	{
		return roomList.ContainsKey(roomName);
	}

	public override void OnJoinedRoom()
	{
		PhotonNetwork.NickName = SteamFriends.GetPersonaName();

		PhotonNetwork.LoadLevel(0);

		print(PhotonNetwork.CurrentRoom.Name);
	}

	public void DiscoverWorlds()
	{
		PhotonNetwork.LeaveRoom();
		StartCoroutine(WaitToJoinLobby("Lobby"));
	}

	private IEnumerator WaitToJoinLobby(string roomName)
	{
		while (PhotonNetwork.InRoom)
		{
			yield return null; // Wait until the player leaves the current room
		}
		PhotonNetwork.JoinLobby();
		SceneManager.LoadScene(1);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		// Check if the Master Client has left
		if (otherPlayer.IsMasterClient)
		{
			Debug.Log("Master Client has left. Closing the room and making all players leave.");

			// Close the room so no new players can join
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;

			// Make all clients leave
			PhotonNetwork.LeaveRoom();
		}
	}

	public override void OnLeftRoom()
	{
		// Load a different scene or return to the main menu
		PhotonNetwork.LoadLevel(0); // Replace 0 with the correct scene index for your main menu
	}

}
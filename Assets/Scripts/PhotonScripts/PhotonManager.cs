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

	public GameObject ConnectionLostScreen;

	public GameObject DiscoverWorlds_obj = null;
	public GameObject Chat = null;
	public UnityEngine.UI.Image ChatBack = null;
	public GameObject offline_btn, online_btn;


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

		int offlineMode = PlayerPrefs.GetInt("OfflineMode", 0); // 0 = Online, 1 = Offline

		if (offlineMode == 1) // Offline Mode
		{
			Debug.Log("Currently in Offline Mode");
			DiscoverWorlds_obj.SetActive(false);
			Chat.SetActive(false);
			ChatBack.enabled = (false);
			offline_btn.SetActive(true); online_btn.SetActive(false);
		}
		else // Online Mode
		{
			Debug.Log("Currently in Online Mode");
			DiscoverWorlds_obj.SetActive(true);
			Chat.SetActive(true);
			ChatBack.enabled = (true);
			offline_btn.SetActive(false); online_btn.SetActive(true);
		}

		//PhotonNetwork.IsMessageQueueRunning = false;
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
		roomOptions.PlayerTtl = 1000;  // 1 second timeout for kicked players

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
			if (PhotonNetwork.CurrentRoom != null)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;
			}

			// Make all clients leave
			photonView.RPC(nameof(RPC_ForceLeaveRoom), RpcTarget.All);
		}
	}

	[PunRPC]
	private void RPC_ForceLeaveRoom()
	{
		Debug.Log("Forced to leave room.");
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom()
	{
		// Load a different scene or return to the main menu
		Debug.Log("Left room, loading main menu...");
		PhotonNetwork.LoadLevel(0); // Replace 0 with your actual menu scene index
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("Master Client disconnected! New Master Client is: " + newMasterClient.NickName);

		// You can perform actions here, such as reassigning roles or handling game logic.

		ConnectionLostScreen.gameObject.SetActive(true);
	}

	public void ReturnOwnWorld()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene(0);
	}

	public void OfflineMode()
	{
		PlayerPrefs.SetInt("OfflineMode", 1); // Save offline mode
		PlayerPrefs.Save();

		if (PhotonNetwork.IsConnected)
		{
			PhotonNetwork.Disconnect();
		}

		PhotonNetwork.OfflineMode = true;
		PhotonNetwork.CreateRoom("OfflineRoom");
		Debug.Log("Switched to Offline Mode");
	}

	public void OnlineMode()
	{
		PlayerPrefs.SetInt("OfflineMode", 0); // Save online mode
		PlayerPrefs.Save();

		if (PhotonNetwork.IsConnected)
		{
			PhotonNetwork.Disconnect();
		}

		PhotonNetwork.OfflineMode = false;
		PhotonNetwork.ConnectUsingSettings();
		Debug.Log("Switched to Online Mode");
	}

}
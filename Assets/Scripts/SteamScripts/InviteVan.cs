using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Steamworks;
using UnityEngine.UI;
using Photon.Realtime;

public class InviteVan : MonoBehaviourPunCallbacks, IInteractable
{
	public GameObject InviteObj;

	[Header("UI Settings")]
	[SerializeField] private GameObject friendButtonPrefab; // Prefab for a button
	[SerializeField] private Transform friendListContainer; // Parent for buttons

	public GameObject roomButtonPrefab; // Assign a UI button prefab
	public Transform roomListContainer; // Assign the container for room buttons
	public Button refreshButton; // Assign the refresh button

	private List<GameObject> roomButtons = new List<GameObject>();

	// Your Steam App ID (Replace this with your actual App ID)
	private static readonly AppId_t GAME_APP_ID = new AppId_t(480); // Example App ID (Replace with your actual one)


	public GameObject InviteFriendsP;


	public void OnInteract()
	{
		InviteObj.SetActive(true);
		DisplayFriends();
	}

	private void Start() {
		InviteFriendsP.SetActive(PhotonNetwork.IsMasterClient);
	}

	private void DisplayFriends()
	{
		// Clear previous buttons
		foreach (Transform child in friendListContainer)
		{
			Destroy(child.gameObject);
		}

		int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		Debug.Log($"Found {friendCount} friends.");

		List<(CSteamID, string, EPersonaState)> onlineFriends = new List<(CSteamID, string, EPersonaState)>();
		List<(CSteamID, string, EPersonaState)> offlineFriends = new List<(CSteamID, string, EPersonaState)>();

		for (int i = 0; i < friendCount; i++)
		{
			CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendSteamId);
			string friendName = SteamFriends.GetFriendPersonaName(friendSteamId);

			// Check if the friend owns the game
			if (!SteamApps.BIsSubscribedApp(GAME_APP_ID))
			{
				Debug.Log($"{friendName} does not own the game. Skipping...");
				continue; // Skip friends who don't have the game
			}

			if (friendState == EPersonaState.k_EPersonaStateOffline)
			{
				offlineFriends.Add((friendSteamId, friendName, friendState));
			}
			else
			{
				onlineFriends.Add((friendSteamId, friendName, friendState));
			}
		}

		Debug.Log($"Online Friends: {onlineFriends.Count}, Offline Friends: {offlineFriends.Count}");

		// Instantiate buttons (Online first, then Offline)
		foreach (var friend in onlineFriends)
		{
			CreateFriendButton(friend.Item1, friend.Item2, friend.Item3, true);
		}

		foreach (var friend in offlineFriends)
		{
			CreateFriendButton(friend.Item1, friend.Item2, friend.Item3, false);
		}
	}

	private void CreateFriendButton(CSteamID friendSteamId, string friendName, EPersonaState friendState, bool isOnline)
	{
		GameObject friendButtonObj = Instantiate(friendButtonPrefab, friendListContainer);
		friendButtonObj.GetComponentInChildren<Text>().text = friendName;

		Button friendButton = friendButtonObj.GetComponent<Button>();
		friendButton.onClick.AddListener(() => SendInvite(friendName, friendSteamId));

		friendButton.interactable = isOnline; // Disable if offline
	}

	private void SendInvite(string friendName, CSteamID friendSteamId)
	{
		Debug.Log($"Sending invite to {friendName} ({friendSteamId}).");

		string roomName = PhotonNetwork.CurrentRoom?.Name;
		if (string.IsNullOrEmpty(roomName))
		{
			Debug.LogError("Cannot send invite. No active Photon room found.");
			return;
		}

		string inviteMessage = $"{SteamFriends.GetPersonaName()} invited you to join their room. Room: {roomName}";

		byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(inviteMessage);
		bool sent = SteamNetworking.SendP2PPacket(friendSteamId, messageBytes, (uint)messageBytes.Length, EP2PSend.k_EP2PSendReliable);

		if (sent)
		{
			Debug.Log($"Invite sent to {friendName}: {inviteMessage}");
		}
		else
		{
			Debug.LogError($"Failed to send invite to {friendName}.");
		}
	}

	public void JoinRoom(string roomName)
	{
		if (PhotonNetwork.InRoom)
		{
			Debug.Log("Leaving current room first...");
			PhotonNetwork.LeaveRoom();
			StartCoroutine(WaitToJoinRoom(roomName));
		}
		else
		{
			PhotonNetwork.JoinRoom(roomName);
		}
	}

	private IEnumerator WaitToJoinRoom(string roomName)
	{
		while (PhotonNetwork.InRoom)
		{
			yield return null;
		}
		PhotonNetwork.JoinRoom(roomName);
	}

	public void OpenRoom()
	{
		if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.IsOpen = true;   // Players can join again
			PhotonNetwork.CurrentRoom.IsVisible = true; // The room appears in the lobby
		}

	}

	public void CloseRoom()
	{
		if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.IsOpen = false;   // No one can join now
			PhotonNetwork.CurrentRoom.IsVisible = false; // The room is hidden from lobby
		}

	}
}

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

	public void OnInteract()
	{												
		// Add your interaction logic here
		InviteObj.SetActive(true);

		DisplayFriends();
	}

	private void Start()
	{
	}

	private void DisplayFriends()
	{
		foreach (GameObject go in  friendListContainer)
		{
			if (friendListContainer.transform.childCount != 0)
			{
				Destroy(go.gameObject);
			}
		}

		int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		Debug.Log($"Found {friendCount} friends.");

		for (int i = 0; i < friendCount; i++)
		{
			CSteamID friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			EPersonaState friendState = SteamFriends.GetFriendPersonaState(friendSteamId);
			string friendName = SteamFriends.GetFriendPersonaName(friendSteamId);

			Debug.Log($"Friend {i + 1}: {friendName} ({friendSteamId})");

			// Instantiate friend button
			GameObject friendButtonObj = Instantiate(friendButtonPrefab, friendListContainer);
			friendButtonObj.GetComponentInChildren<Text>().text = friendName;

			// Add listener to the button
			Button friendButton = friendButtonObj.GetComponent<Button>();
			friendButton.onClick.AddListener(() => SendInvite(friendName, friendSteamId));

			// Check if friend is online or offline
			if (friendState == EPersonaState.k_EPersonaStateOffline)
			{
				friendButton.interactable = false; // Disable button if offline
			}
			else
			{
				friendButton.interactable = true; // Enable button if online
			}
		}
	}

	private void SendInvite(string friendName, CSteamID friendSteamId)
	{
		Debug.Log($"Sending invite to {friendName} ({friendSteamId}).");

		// The room name to send in the invite
		string roomName = PhotonNetwork.CurrentRoom?.Name;
		if (string.IsNullOrEmpty(roomName))
		{
			Debug.LogError("Cannot send invite. No active Photon room found.");
			return;
		}

		// Create the invite message
		string inviteMessage = $"{SteamFriends.GetPersonaName()} invited you to join their room. Room: {roomName}";

		// Convert the message to bytes
		byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(inviteMessage);

		// Send the invite message via Steam P2P
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
			yield return null; // Wait until the player leaves the current room
		}
		PhotonNetwork.JoinRoom(roomName);
	}

	public void OpenRoom()
	{
		PhotonNetwork.CurrentRoom.IsOpen = true;
		PhotonNetwork.CurrentRoom.IsVisible = true;
	}

	public void CloseRoom()
	{
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
	}
}

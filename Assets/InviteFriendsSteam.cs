using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using Steamworks;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class InviteFriendsSteam : MonoBehaviourPunCallbacks
{
	[Header("Settings")]
	private Camera playerCamera; // Assign your camera in the inspector
	[SerializeField] private float maxDistance = 10f; // Maximum distance to detect objects
	[SerializeField] private LayerMask interactableLayer; // Layer mask for interactable objects

	[Header("UI Elements")]
	[SerializeField] private Transform inviteListContainer; // Parent container for invite buttons
	[SerializeField] private GameObject inviteButtonPrefab; // Prefab for invite buttons

	private void Awake()
	{
		playerCamera = Camera.main;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
		{
			CheckForObject();
		}

		CheckIncomingInvites();
	}

	private void CheckForObject()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer))
		{
			// Call a function on the hit object
			var interactable = hit.collider.GetComponent<IInteractable>();
			if (interactable != null)
			{
				interactable.OnInteract();
			}
			else
			{
				Debug.Log("Hit an object, but it doesn't implement IInteractable.");
			}
		}
		else
		{
			Debug.Log("No interactable object detected.");
		}
	}

	public void CheckIncomingInvites()
	{
		uint messageSize;
		while (SteamNetworking.IsP2PPacketAvailable(out messageSize))
		{
			byte[] buffer = new byte[messageSize];
			CSteamID sender;
			uint bytesRead;

			if (SteamNetworking.ReadP2PPacket(buffer, messageSize, out bytesRead, out sender))
			{
				string message = System.Text.Encoding.UTF8.GetString(buffer);
				Debug.Log($"Invite received from {SteamFriends.GetFriendPersonaName(sender)}: {message}");

				if (message.Contains("Room:"))
				{
					string[] parts = message.Split(new[] { "Room:" }, System.StringSplitOptions.None);
					if (parts.Length > 1)
					{
						string roomName = parts[1].Trim();
						CreateInviteUI(SteamFriends.GetFriendPersonaName(sender), roomName);
					}
				}
			}
		}
	}

	private void CreateInviteUI(string inviterName, string roomName)
	{
		GameObject newInviteButton = Instantiate(inviteButtonPrefab, inviteListContainer);
		Text buttonText = newInviteButton.GetComponentInChildren<Text>();
		Button buttonComponent = newInviteButton.GetComponent<InviteCard>().Accept;

		buttonText.text = $"{inviterName} invited you to their game!";
		buttonComponent.onClick.AddListener(() => JoinPhotonRoom(roomName));
	}

	private void JoinPhotonRoom(string roomName)
	{
		n = roomName;

		Debug.Log($"Joining room: {roomName}");
		if (PhotonNetwork.InRoom)
		{
			PhotonNetwork.LeaveRoom();
			StartCoroutine(WaitToJoinRoom("Casino"));
		}
		else
		{
			PhotonNetwork.JoinRoom(roomName);
		}
	}

	string n = null;

	private IEnumerator WaitToJoinRoom(string roomName)
	{
		while (PhotonNetwork.InRoom)
		{
			yield return null; // Wait until the player leaves the current room
		}
		PhotonNetwork.JoinRoom(roomName);
	}
}

public interface IInteractable
{
	void OnInteract();
}
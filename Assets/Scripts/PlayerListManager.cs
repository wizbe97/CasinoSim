using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerListManager : MonoBehaviourPunCallbacks
{
	public Transform playerListParent;  // UI Parent (Vertical Layout Group)
	public GameObject playerItemPrefab; // Prefab for each player (Name + Kick Button)
	private Dictionary<int, GameObject> playerItems = new Dictionary<int, GameObject>();

	private void Start()
	{
		UpdatePlayerList(); // Initialize the list on start
	}

	public override void OnEnable()
	{
		base.OnEnable();
		PhotonNetwork.NetworkingClient.EventReceived += OnPlayerListChanged;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		PhotonNetwork.NetworkingClient.EventReceived -= OnPlayerListChanged;
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdatePlayerList();
		photonView.RPC(nameof(RPC_UpdatePlayerList), RpcTarget.All);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (otherPlayer.ActorNumber == 0)
		{
			PhotonNetwork.LeaveRoom();
			SceneManager.LoadScene(0);
		}

		if (playerItems.ContainsKey(otherPlayer.ActorNumber))
		{
			Destroy(playerItems[otherPlayer.ActorNumber]); // Remove UI
			playerItems.Remove(otherPlayer.ActorNumber);
		}
		UpdatePlayerList();
		photonView.RPC(nameof(RPC_UpdatePlayerList), RpcTarget.All);
	}

	[PunRPC]
	private void RPC_UpdatePlayerList()
	{
		UpdatePlayerList();
	}

	private void UpdatePlayerList()
	{
		// Clear existing UI
		foreach (var item in playerItems.Values)
		{
			Destroy(item);
		}
		playerItems.Clear();

		List<Player> sortedPlayers = new List<Player>(PhotonNetwork.PlayerList);
		sortedPlayers.Sort((a, b) => a.IsMasterClient ? -1 : b.IsMasterClient ? 1 : 0); // Master first

		foreach (Player player in sortedPlayers)
		{
			GameObject playerItem = Instantiate(playerItemPrefab, playerListParent);
			Text playerText = playerItem.transform.GetComponentInChildren<Text>();

			if (playerText != null)
			{
				playerText.text = (player.IsMasterClient ? "[Host] " : "") + player.NickName;
				playerText.color = player.IsMasterClient ? Color.yellow : Color.white; // Highlight host
			}

			// Show Kick Button only if Master Client and not kicking themselves
			UnityEngine.UI.Button kickButton = playerItem.transform.GetComponentInChildren<UnityEngine.UI.Button>();
			kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && !player.IsMasterClient);
			kickButton.onClick.RemoveAllListeners(); // Prevent duplicate listeners
			kickButton.onClick.AddListener(() => KickPlayer(player));

			playerItems[player.ActorNumber] = playerItem;
		}
	}

	private void KickPlayer(Player playerToKick)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.Log($"Sending RPC to kick {playerToKick.NickName}");
			FindFirstObjectByType<PlayerLeaveNotifier>().OnPlayerKicked(playerToKick.NickName);
			photonView.RPC(nameof(ForceDisconnect), playerToKick);


			Chatsystem chatSc = FindFirstObjectByType<Chatsystem>();

			// Deselect the input field when Enter is pressed
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			if (inputField != null)
			{
				inputField.DeactivateInputField();
			}
			EventSystem.current.SetSelectedGameObject(null); // Deselect UI element
			chatSc.canMove = true;

			chatSc.connected_players.SetActive(false);
			EventSystem.current.SetSelectedGameObject(null); // Deselect any UI element
			chatSc._field.DeactivateInputField(); // Stop editing
		}
	}

	[PunRPC]
	private void ForceDisconnect()
	{
		Debug.Log("I was kicked!");
		PlayerPrefs.SetInt("WasKicked", 1); // Save kick status
		PlayerPrefs.Save(); // Ensure it is stored
		PhotonNetwork.LeaveRoom();
	}


	private void OnPlayerListChanged(ExitGames.Client.Photon.EventData obj)
	{
		UpdatePlayerList();
	}
}

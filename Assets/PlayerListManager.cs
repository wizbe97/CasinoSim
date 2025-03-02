using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerListManager : MonoBehaviourPunCallbacks
{
	public Transform playerListParent;  // UI Parent (Vertical Layout Group)
	public GameObject playerItemPrefab; // Prefab for each player (Name + Kick Button)
	private Dictionary<int, GameObject> playerItems = new Dictionary<int, GameObject>();

	private void Start()
	{
		UpdatePlayerList(); // Initialize the list on start
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		UpdatePlayerList();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (playerItems.ContainsKey(otherPlayer.ActorNumber))
		{
			Destroy(playerItems[otherPlayer.ActorNumber]); // Remove UI
			playerItems.Remove(otherPlayer.ActorNumber);
		}
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
			Button kickButton = playerItem.transform.GetComponentInChildren<Button>();
			kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && !player.IsMasterClient);
			kickButton.onClick.AddListener(() => KickPlayer(player));

			playerItems[player.ActorNumber] = playerItem;
		}
	}

	private void KickPlayer(Player playerToKick)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CloseConnection(playerToKick);
			Debug.Log($"Kicked {playerToKick.NickName}");
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PlayerLeaveNotifier : MonoBehaviourPunCallbacks
{
	public Transform messageListParent; // UI Parent for messages
	public GameObject messagePrefab;    // Prefab containing a Text component
	private Dictionary<int, string> playerNames = new Dictionary<int, string>();

	private void Start()
	{
		foreach (Player player in PhotonNetwork.PlayerList)
		{
			if (!playerNames.ContainsKey(player.ActorNumber))
				playerNames[player.ActorNumber] = player.NickName;
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		playerNames[newPlayer.ActorNumber] = newPlayer.NickName;
		photonView.RPC("BroadcastMessage", RpcTarget.All, newPlayer.NickName, "has joined the world", Color.green.r, Color.green.g, Color.green.b);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (playerNames.ContainsKey(otherPlayer.ActorNumber))
		{
			photonView.RPC("BroadcastMessage", RpcTarget.All, playerNames[otherPlayer.ActorNumber], "has left the world", Color.red.r, Color.red.g, Color.red.b);
			playerNames.Remove(otherPlayer.ActorNumber);
		}
	}

	public void OnPlayerKicked(Player kickedPlayer)
	{
		if (playerNames.ContainsKey(kickedPlayer.ActorNumber))
		{
			photonView.RPC("BroadcastMessage", RpcTarget.All, playerNames[kickedPlayer.ActorNumber], "was kicked from the world", 1f, 0.5f, 0f); // Orange
			playerNames.Remove(kickedPlayer.ActorNumber);
		}
	}

	[PunRPC]
	private void BroadcastMessage(string playerName, string action, float r, float g, float b)
	{
		GameObject msgObject = Instantiate(messagePrefab, messageListParent);
		Text messageText = msgObject.GetComponentInChildren<Text>();

		if (messageText != null)
		{
			messageText.text = playerName + " " + action;
			messageText.color = new Color(r, g, b); // Apply received color
		}
		else
		{
			Debug.LogError("Text component not found in messagePrefab!");
		}
	}
}

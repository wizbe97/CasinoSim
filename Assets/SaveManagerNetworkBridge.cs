using Photon.Pun;
using UnityEngine;

public class SaveManagerNetworkBridge : MonoBehaviourPun
{
	public SaveManagerSO saveManagerSO;

	private void Start()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			FindObjectOfType<SaveManagerNetworkBridge>().TryLoadAndSyncAll();
		}
	}

	public void TryLoadAndSyncAll()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		// Load locally from disk
		saveManagerSO.LoadFromDisk();

		// Send JSON to all clients
		string balanceJson = saveManagerSO.SaveBalanceToJson();
		string itemsJson = saveManagerSO.SaveItemsToJson();

		photonView.RPC(nameof(RPC_ReceiveBalance), RpcTarget.Others, balanceJson);
		photonView.RPC(nameof(RPC_ReceiveItems), RpcTarget.Others, itemsJson);
	}

	[PunRPC]
	void RPC_ReceiveBalance(string balanceJson)
	{
		saveManagerSO.LoadBalanceFromJson(balanceJson);
	}

	[PunRPC]
	void RPC_ReceiveItems(string itemsJson)
	{
		saveManagerSO.LoadItemsFromJson(itemsJson);
	}

	// Optional: sync save to disk on all clients
	public void SaveAllAndBroadcast()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		// Save to disk
		saveManagerSO.SaveToDisk();

		// Sync to others
		TryLoadAndSyncAll();
	}
}

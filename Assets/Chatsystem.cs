using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Chatsystem : MonoBehaviour
{
	public GameObject _box = null;
	public Transform container;
	public GameObject _card;

	public TMP_InputField _field = null;


	private void Update()
	{
		// Local Check
		if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		if (Input.GetKeyDown(KeyCode.T))
		{
			//if (_box.activeSelf)
			//{
			//	_box.SetActive(false);
			//}
			//else
			//{
			//	_box.SetActive(true);
			//}

			EventSystem.current.SetSelectedGameObject(_field.gameObject);
			_field.ActivateInputField(); // Makes it ready for typing
		}

		if (!_box.activeSelf) {
			return;
		}

		if (_field.text.Length > 0)
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				SendMessage();
			}
		}
	}

	private void SendMessage()
	{
		GetComponent<PhotonView>().RPC("SendRPC", RpcTarget.AllBuffered, SteamFriends.GetPersonaName() + ": " + _field.text);
	}

	[PunRPC]
	private void SendRPC(string message)
	{
		GameObject _c = Instantiate(_card, container);
		_c.GetComponentInChildren<UnityEngine.UI.Text>().text = message;
		_field.text = null;
	}
}
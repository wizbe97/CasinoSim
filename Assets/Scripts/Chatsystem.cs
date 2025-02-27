using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Chatsystem : MonoBehaviour
{
	public GameObject _box = null;
	public Transform container;
	public GameObject _card;

	public TMP_InputField _field = null;

	public bool canMove = false;

	public Text _header;


	private void Awake()
	{
		// Local Check
		if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		_header = FindObjectOfType<chatBox>()._header;
		_box = FindObjectOfType<chatBox>()._box;
		container = FindObjectOfType<chatBox>().container;
		_field = FindObjectOfType<chatBox>()._field;
	}

	private void Start()
	{
		_header.text = SteamFriends.GetPersonaName() + "'s World";
	}

	private void Update()
	{
		// Local Check
		if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		// Check if the active UI element is an InputField
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			canMove = inputField == null; // Disable movement if an InputField is selected
		}
		else
		{
			canMove = true;
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
		if (GetComponent<PhotonView>().IsMine)
		{
			// Deselect the input field when Enter is pressed
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			inputField.DeactivateInputField();
			EventSystem.current.SetSelectedGameObject(null); // Deselect UI element
			canMove = true;
		}

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
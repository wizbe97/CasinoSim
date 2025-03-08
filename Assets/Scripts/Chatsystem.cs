using UnityEngine;
using Photon.Pun;
using TMPro;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
<<<<<<< HEAD
using System.Collections.Generic;
using UnityEngine.EventSystems;
=======
>>>>>>> ae048306770758776e90ab33b64ba74f8bfb671d

public class Chatsystem : MonoBehaviour
{
	public GameObject _box = null;
	public Transform container;
	public GameObject _card;

	public TMP_InputField _field = null;
	public GameObject Kickbutton;

	public bool canMove = false;

	public Text _header;

	public int maxMessages;

	public GameObject connected_players;

	public bool isTyping = false;

	public EventSystem eventSystem;
	public GraphicRaycaster graphicRaycaster;


	private void Awake()
	{
		_header = FindFirstObjectByType<ChatBox>()._header;
		_box = FindFirstObjectByType<ChatBox>()._box;
		container = FindFirstObjectByType<ChatBox>().container;
		_field = FindFirstObjectByType<ChatBox>()._field;
		connected_players = FindFirstObjectByType<ChatBox>().connected_players;
		eventSystem = FindFirstObjectByType<ChatBox>().eventSystem;
		graphicRaycaster = FindFirstObjectByType<ChatBox>().graphicRaycaster;
	}

	private void Start()
	{
		_header.text = PhotonNetwork.CurrentRoom.Name;
	}

	private void Update()
	{
<<<<<<< HEAD
		// Local Check: Only handle input for the local player
=======
>>>>>>> ae048306770758776e90ab33b64ba74f8bfb671d
		if (!GetComponent<PhotonView>().IsMine)
		{
			return;
		}

		isTyping = EventSystem.current.currentSelectedGameObject == _field.gameObject;

		if (EventSystem.current.currentSelectedGameObject != null)
		{
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			canMove = inputField == null; 
		}
		else
		{
			canMove = true;
		}

		if (Input.GetKeyDown(KeyCode.Escape) && isTyping)
		{
			_field.DeactivateInputField();
			EventSystem.current.SetSelectedGameObject(null); 
			canMove = true;
			connected_players.SetActive(false);
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (isTyping)
			{
				if (_field.text.Length > 0)
				{
					SendMessage();
				}
			}
			else
			{
				if (_field.text.Length > 0)
				{
					connected_players.SetActive(true);
					EventSystem.current.SetSelectedGameObject(_field.gameObject);
					_field.ActivateInputField();

					StartCoroutine(SetCaretToEnd());
				}
				else
				{
					connected_players.SetActive(!connected_players.activeSelf);
					if (connected_players.activeSelf)
					{
						EventSystem.current.SetSelectedGameObject(_field.gameObject);
						_field.ActivateInputField();
					}
				}
			}
		}

		if (EventSystem.current.currentSelectedGameObject != _field.gameObject)
		{
<<<<<<< HEAD
			PointerEventData pointerEventData = new PointerEventData(eventSystem);
			pointerEventData.position = Input.mousePosition;

			List<RaycastResult> results = new List<RaycastResult>();
			graphicRaycaster.Raycast(pointerEventData, results);

			foreach (var result in results)
			{
				if (result.gameObject.transform.tag == "KickBtn")
				{
					return;
				}
			}

=======
>>>>>>> ae048306770758776e90ab33b64ba74f8bfb671d
			connected_players.SetActive(false);
		}
	}

	private IEnumerator SetCaretToEnd()
	{
		yield return new WaitForEndOfFrame();
		_field.caretPosition = _field.text.Length; 
	}


	private void SendMessage()
	{
		if (GetComponent<PhotonView>().IsMine)
		{
			// Select the input field Again when Enter is pressed
			//EventSystem.current.SetSelectedGameObject(_field.gameObject);
			//_field.ActivateInputField(); // Makes it ready for typing

			// Deselect the input field when Enter is pressed
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
			inputField.DeactivateInputField();
			EventSystem.current.SetSelectedGameObject(null); // Deselect UI element
			canMove = true;
		}

		GetComponent<PhotonView>().RPC("SendRPC", RpcTarget.AllBuffered, SteamFriends.GetPersonaName() + ": " + _field.text);
		_field.text = null;
	}

	[PunRPC]
	private void SendRPC(string message)
	{
		GameObject _c = Instantiate(_card, container);
		_c.GetComponentInChildren<UnityEngine.UI.Text>().text = message;

		if (container.childCount > maxMessages)
		{
			Destroy(container.GetChild(0).gameObject); // Remove the oldest message
		}

		Canvas.ForceUpdateCanvases();
	}
}
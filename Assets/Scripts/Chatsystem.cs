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

	public bool canMove = false;

	public Text _header;

	public int maxMessages;

	public GameObject connected_players;

	public bool isTyping = false;


	private void Awake()
	{
		_header = FindFirstObjectByType<ChatBox>()._header;
		_box = FindFirstObjectByType<ChatBox>()._box;
		container = FindFirstObjectByType<ChatBox>().container;
		_field = FindFirstObjectByType<ChatBox>()._field;
		connected_players = FindFirstObjectByType<ChatBox>().connected_players;
	}

	private void Start()
	{
		_header.text = PhotonNetwork.CurrentRoom.Name;
	}

	private void Update()
	{
		// Local Check
		if (!GetComponent<PhotonView>().IsMine) {
			return;
		}

		isTyping = !canMove;

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

        if (Input.GetKeyDown(KeyCode.Escape) && EventSystem.current.currentSelectedGameObject != _field.gameObject)
        {
			// Deselect the input field when Enter is pressed
			TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
		    inputField.DeactivateInputField();
			EventSystem.current.SetSelectedGameObject(null); // Deselect UI element
			canMove = true;

			connected_players.SetActive(false);
			EventSystem.current.SetSelectedGameObject(null); // Deselect any UI element
			_field.DeactivateInputField(); // Stop editing
		}

		if (Input.GetKeyDown(KeyCode.Return) /*&&  _field.text.Length == 0*/)
		{
			if (connected_players.activeSelf)
			{
				connected_players.SetActive(false);
			}
			else
			{
				connected_players.SetActive(true);
				EventSystem.current.SetSelectedGameObject(_field.gameObject);
				_field.ActivateInputField(); // Makes it ready for typing
			}
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
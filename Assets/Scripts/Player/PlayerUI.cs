using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerUI : MonoBehaviourPunCallbacks
{
	[Header("UI Panels")]
	[SerializeField] private GameObject _phonePanel;
	[SerializeField] private GameObject _appPanel;
	[SerializeField] private GameObject _furniturePanel;
	[SerializeField] private GameObject _reticle;
	private TMP_Text _balanceText;

	[Header("Player References")]
	[SerializeField] private PlayerController _playerController;
	[SerializeField] private GameManagerSO _gameManager;

	[Header("Events")]
	[SerializeField] private GameEventSO onBalanceChangedEvent;

	private bool _isPhonePanelActive = false;

	public GameObject kickedUIScreen = null;


	private void Awake()
	{
		if (photonView.IsMine)
		{
			_balanceText = GameObject.FindGameObjectWithTag("BalanceText").GetComponent<TMP_Text>();
			UpdateBalanceUI();

			// Kick Message
			if (PlayerPrefs.GetInt("WasKicked", 0) == 1) // Default is 0 (not kicked)
			{
				Debug.Log("Showing kicked UI screen.");
				kickedUIScreen.SetActive(true); // Show your UI screen

				// Reset the flag so it doesn't appear next time
				PlayerPrefs.SetInt("WasKicked", 0);
				PlayerPrefs.Save();
			}
		}
	}

	public void TogglePhonePanel()
	{
		if (!photonView.IsMine) return;

		_isPhonePanelActive = !_isPhonePanelActive;
		_phonePanel.SetActive(_isPhonePanelActive);
		_reticle.SetActive(!_isPhonePanelActive);

		if (_isPhonePanelActive)
		{
			ShowCursor();
		}
		else
		{
			HideCursor();
		}
	}

	public void ShowAppPanel()
	{
		if (!photonView.IsMine) return;

		_appPanel.SetActive(true);
	}

	public void ShowPhonePanel()
	{
		if (!photonView.IsMine) return;

		_phonePanel.SetActive(true);
		_reticle.SetActive(false);
		ShowCursor();
	}

	public void CloseAppPanel()
	{
		if (!photonView.IsMine) return;

		if (_appPanel == null)
		{
			Debug.LogError("AppPanel is not assigned in PlayerUI.");
			return;
		}
		_appPanel.SetActive(false);
		CloseFurniturePanel();
	}

	public void ShowFurniturePanel()
	{
		if (!photonView.IsMine) return;

		ShowAppPanel();
		_furniturePanel.SetActive(true);
	}

	public void CloseFurniturePanel()
	{
		if (!photonView.IsMine) return;

		_furniturePanel.SetActive(false);
	}

	public void ResetPanelStates()
	{
		if (!photonView.IsMine) return;

		CloseAppPanel();
		CloseFurniturePanel();
		_phonePanel.SetActive(false);
		_reticle.SetActive(true);
		HideCursor();
		_isPhonePanelActive = false;
	}

	public void ShowCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		_playerController.enabled = false;
	}

	public void HideCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		_playerController.enabled = true;
	}

	public void UpdateBalanceUI()
	{
		if (!photonView.IsMine) return;

		_balanceText.text = "Balance: $" + _gameManager.playerBalanceManager.playerBalance.balance;
	}

	public bool IsPhonePanelActive()
	{
		return photonView.IsMine && _phonePanel.activeSelf;
	}
}

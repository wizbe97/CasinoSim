using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
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
    [SerializeField] private PurchaseManager _purchaseManager;

    [Header("Events")]
    [SerializeField] private GameEventSO onBalanceChangedEvent;

    private bool _isPhonePanelActive = false;

    private void Awake()
    {
        _balanceText = GameObject.FindGameObjectWithTag("BalanceText").GetComponent<TMP_Text>();
        UpdateBalanceUI();
    }

    public void TogglePhonePanel()
    {
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
        _appPanel.SetActive(true);
    }

    public void ShowPhonePanel()
    {
        _phonePanel.SetActive(true);
        _reticle.SetActive(false);
        ShowCursor();
    }

    public void CloseAppPanel()
    {
        _appPanel.SetActive(false);
        CloseFurniturePanel();
    }

    public void ShowFurniturePanel()
    {
        ShowAppPanel();
        _furniturePanel.SetActive(true);
    }

    public void CloseFurniturePanel()
    {
        _furniturePanel.SetActive(false);
    }

    public void ResetPanelStates()
    {
        CloseAppPanel();
        CloseFurniturePanel();
        _phonePanel.SetActive(false);
        _reticle.SetActive(true);
        HideCursor();
        _isPhonePanelActive = false;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerController.enabled = false;
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerController.enabled = true;
    }

    public void UpdateBalanceUI()
    {
        _balanceText.text = "Balance: $" + _gameManager.playerBalanceManager.playerBalance.balance;
    }

    public bool IsPhonePanelActive()
    {
        return _phonePanel.activeSelf;
    }

}

using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _phonePanel;
    [SerializeField] private GameObject _appPanel;
    [SerializeField] private GameObject _furniturePanel;
    [SerializeField] private FirstPersonController _firstPersonController;
    [SerializeField] private PlacementManager _placementManager;
    [SerializeField] private GameObject _reticle;
    [SerializeField] private  TMP_Text _balanceText;

    [Header("Events")]
    [SerializeField] private GameEventSO onBalanceChangedEvent;
    [SerializeField] private GameEventSO onItemPurchasedEvent;

    [Header("Game Manager")]
    [SerializeField] private GameManagerSO _gameManager;

    private void Start()
    {
        UpdateBalanceUI();
    }
    public void ShowPhonePanel()
    {
        _phonePanel.SetActive(true);
        _reticle.SetActive(false);
        ShowCursor();
    }

    public void ClosePhonePanel()
    {
        _phonePanel.SetActive(false);
        _reticle.SetActive(true);
        HideCursor();
    }

    public bool IsPhonePanelActive()
    {
        return _phonePanel.activeSelf;
    }

    public void ShowAppPanel()
    {
        _appPanel.SetActive(true);
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

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _firstPersonController.enabled = false;
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _firstPersonController.enabled = true;
    }

    public void ResetPanelStates()
    {
        CloseAppPanel();
        CloseFurniturePanel();
        ClosePhonePanel();
    }

    public void BuyItem(PlaceableItemSO item)
    {
        if (_gameManager.playerBalanceManager.CanAfford(item.Price))
        {
            _gameManager.playerBalanceManager.DeductBalance(item.Price);
            ResetPanelStates();
            // _placementManager.StartPlacement(item);
            onBalanceChangedEvent.Raise();
            onItemPurchasedEvent.Raise(); // spawn the delivery truck
        }
        else
        {
            Debug.Log("Not enough balance to buy this item.");
        }
    }

    public void UpdateBalanceUI()
    {
        _balanceText.text = "Balance: $" + _gameManager.playerBalanceManager.playerBalance.balance;
    }
}

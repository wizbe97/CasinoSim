using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _phonePanel;
    [SerializeField] private GameObject _appPanel;
    [SerializeField] private GameObject _furniturePanel;
    [SerializeField] private TMP_Text _balanceText;

    [Header("Events")]
    [SerializeField] private GameEventSO onBalanceChangedEvent;

    [Header("Game Manager")]
    [SerializeField] private GameManagerSO _gameManager;
    [SerializeField] private DeliveryVehicleManager _deliveryVehicleManager;


    private PlayerInteraction _currentPlayer;

    public void SetCurrentPlayer(PlayerInteraction player)
    {
        _currentPlayer = player;
    }



    private void Start()
    {
        UpdateBalanceUI();
    }

    public void ShowPhonePanel(PlayerInteraction player)
    {
        _phonePanel.SetActive(true);
        player.SetReticleVisibility(false); // Disable only the triggering player's reticle
        ShowCursor(player);
    }

    public void ClosePhonePanel(PlayerInteraction player)
    {
        _phonePanel.SetActive(false);
        player.SetReticleVisibility(true); // Enable only the triggering player's reticle
        HideCursor(player);
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

    public void ResetPanelStates(PlayerInteraction player)
    {
        CloseAppPanel();
        CloseFurniturePanel();
        ClosePhonePanel(player);
    }

    public void BuyItem(PlaceableItemSO item)
    {
        if (_gameManager.playerBalanceManager.CanAfford(item.Price))
        {
            _gameManager.playerBalanceManager.DeductBalance(item.Price);
            onBalanceChangedEvent.Raise();

            _deliveryVehicleManager.SpawnDeliveryVehicle(item.GetCardboardBoxPrefab());

            // Use the current player for resetting panel states
            if (_currentPlayer != null)
            {
                ResetPanelStates(_currentPlayer);
            }
            else
            {
                Debug.LogWarning("Current player is not set!");
            }
        }
    }


    public void UpdateBalanceUI()
    {
        _balanceText.text = "Balance: $" + _gameManager.playerBalanceManager.playerBalance.balance;
    }

    private void ShowCursor(PlayerInteraction player)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.DisableMovement(); // Disable movement for the specific player
    }

    private void HideCursor(PlayerInteraction player)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        player.EnableMovement(); // Enable movement for the specific player
    }
}

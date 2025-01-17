using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;

public class UIManager : NetworkBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _phonePanel;
    [SerializeField] private GameObject _appPanel;
    [SerializeField] private GameObject _furniturePanel;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlacementManager _placementManager;
    [SerializeField] private GameObject _reticle;
    [SerializeField] private TMP_Text _balanceText;

    [Header("Events")]
    [SerializeField] private GameEventSO onBalanceChangedEvent;

    [Header("Game Manager")]
    [SerializeField] private GameManagerSO _gameManager;
    [SerializeField] private DeliveryVehicleManager _deliveryVehicleManager;

    // expose balance changed event
    public GameEventSO OnBalanceChangedEvent => onBalanceChangedEvent;

    public override void OnNetworkSpawn()
    {
        _playerController = GetComponent<PlayerController>();
        _gameManager = GetComponent<GameManagerSO>();
        UpdateBalanceUI();

        base.OnNetworkSpawn();
        if (IsHost)
        {
            _placementManager = GetComponent<PlacementManager>();
            _deliveryVehicleManager = GetComponent<DeliveryVehicleManager>();
        }
    }

    private void Start()
    {
        UpdateBalanceUI();
    }
    public void ShowPhonePanel()
    {
        if (!IsOwner) return;

        _phonePanel.SetActive(true);
        _reticle.SetActive(false);
        ShowCursor();
    }

    public void ClosePhonePanel()
    {
        if (!IsOwner) return;

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
        if (!IsOwner) return;

        _appPanel.SetActive(true);
    }

    public void CloseAppPanel()
    {
        if (!IsOwner) return;

        _appPanel.SetActive(false);
        CloseFurniturePanel();
    }

    public void ShowFurniturePanel()
    {
        if (!IsOwner) return;

        ShowAppPanel();
        _furniturePanel.SetActive(true);
    }

    public void CloseFurniturePanel()
    {
        if (!IsOwner) return;

        _furniturePanel.SetActive(false);
    }

    private void ShowCursor()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerController.enabled = false;
    }

    private void HideCursor()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerController.enabled = true;
    }

    public void ResetPanelStates()
    {
        if (!IsOwner) return;

        CloseAppPanel();
        CloseFurniturePanel();
        ClosePhonePanel();
    }

    public void BuyItem(PlaceableItemSO item)
    {
        if (!IsHost) return;

        if (_gameManager.playerBalanceManager.CanAfford(item.Price))
        {
            _gameManager.playerBalanceManager.DeductBalance(item.Price);
            ResetPanelStates();
            onBalanceChangedEvent.Raise();

            _deliveryVehicleManager.SpawnDeliveryVehicle(item.GetCardboardBoxPrefab());
        }
    }


    public void UpdateBalanceUI()
    {
        if (!IsOwner) return;
        _balanceText.text = "Balance: $" + _gameManager.playerBalanceManager.playerBalance.balance;
    }
}

using UnityEngine;
using System.Collections;

public class BlackjackDealer : MonoBehaviour
{
    [Header("Blackjack Shoe")]
    [SerializeField] private BlackJackShoeSO blackjackShoe; // Reference to the Blackjack Shoe Scriptable Object

    [Header("Player Interaction")]
    private PlayerInputHandler _inputHandler; // Reference to the player input handler
    private PlayerInteraction _playerInteraction; // Reference to the player interaction script
    private PlayerController _playerController;

    [SerializeField] private float _interactionDistance = 5f; // Distance for raycasting to interact with objects
    [SerializeField] private float cardGap = 0.01f; // Gap between cards when dealing

    private BlackjackTable blackjackTable; // Dynamically assigned reference to the table
    private bool cardsDealt = false; // Flag to prevent multiple deals without finishing a round
    private bool isDealer = false; // Tracks whether the player has joined the table as the dealer
    private Transform dealerSpot; // Reference to the dealer's spot at the table
    private float cardDealingDelay;
    private RectTransform _reticleUI;
    [SerializeField] private float zoomSpeed = 2f; // Speed of zoom
    [SerializeField] private float minZoom = 20f; // Minimum field of view for zooming
    [SerializeField] private float maxZoom = 60f;
    private float targetFOV;

    private int currentPlayerIndex = 0; // Tracks the current player's turn
    private bool isAwaitingDealerInput = false; // Wait for the dealer to press D

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _reticleUI = _playerInteraction.ReticleUI;
    }

    private void SubscribeDealerInputActions()
    {
        Debug.Log("BlackjackDealer: Subscribing to dealer input actions.");
        _inputHandler.OnDealCard += HandleDealInput; // Adjusted to handle turn logic
        _inputHandler.OnCancel += LeaveDealerSpot;
        _inputHandler.OnZoom += HandleZoom;
    }

    private void UnsubscribeDealerInputActions()
    {
        Debug.Log("BlackjackDealer: Unsubscribing from dealer input actions.");
        _inputHandler.OnDealCard -= HandleDealInput;
        _inputHandler.OnCancel -= LeaveDealerSpot;
        _inputHandler.OnZoom -= HandleZoom;
    }

    private void HandleDealInput()
    {
        if (!cardsDealt)
        {
            DealCards(); // Initial card dealing
        }
        else
        {
            // Advance the game flow (deal card or move to the next player)
            isAwaitingDealerInput = false;
        }
    }

    private void HandleZoom(float scrollInput)
    {
        targetFOV -= scrollInput * zoomSpeed;
        targetFOV = Mathf.Clamp(targetFOV, minZoom, maxZoom);

        float lerpSpeed = Mathf.Max(10f, Mathf.Abs(_playerInteraction.PlayerCamera.fieldOfView - targetFOV) * 10f);
        _playerInteraction.PlayerCamera.fieldOfView = Mathf.Lerp(
            _playerInteraction.PlayerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * lerpSpeed
        );
    }

    private void ResetZoom()
    {
        targetFOV = maxZoom;
        _playerInteraction.PlayerCamera.fieldOfView = maxZoom;
    }

    private void OnEnable()
    {
        _inputHandler.OnJoinTable += JoinTable;
    }

    private void OnDisable()
    {
        UnsubscribeDealerInputActions();
        _inputHandler.OnJoinTable -= JoinTable;
    }

    private void JoinTable()
    {
        if (isDealer)
        {
            Debug.LogWarning("Already acting as dealer.");
            return;
        }

        Ray ray = _playerInteraction.PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _playerInteraction.ReticleUI.position));
        Debug.DrawRay(ray.origin, ray.direction * _interactionDistance, Color.red, 2f);

        RaycastHit[] hits = Physics.RaycastAll(ray, _interactionDistance);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PlacedObjects"))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log($"Hit object: {hitObject.name}, Layer: {LayerMask.LayerToName(hitObject.layer)}");

                BlackjackTable table = hitObject.GetComponentInParent<BlackjackTable>();

                if (table != null)
                {
                    blackjackTable = table;
                    dealerSpot = table.dealerCardSpot;

                    if (dealerSpot != null)
                    {
                        TeleportToDealerSpot(table.dealerSeat);
                    }
                    else
                    {
                        Debug.LogWarning("No DealerSpot found for the blackjack table.");
                    }

                    return;
                }
                else
                {
                    Debug.LogWarning("No BlackjackTable component found on this object.");
                }
            }
        }
    }

    private void TeleportToDealerSpot(Transform dealerSeat)
    {
        if (dealerSeat == null) return;

        transform.position = dealerSeat.position;
        transform.rotation = dealerSeat.rotation;
        cardDealingDelay = blackjackTable.CardDealingDelay;

        _playerController.CanMove = false;

        UnsubscribeInteractionEvents();
        SubscribeDealerInputActions();

        isDealer = true;
    }

    private void LeaveDealerSpot()
    {
        if (!isDealer) return;

        _playerController.CanMove = true;

        SubscribeInteractionEvents();
        UnsubscribeDealerInputActions();
        ResetZoom();

        isDealer = false;
        blackjackTable = null;
    }

    public void DealCards()
    {
        if (cardsDealt)
        {
            Debug.LogWarning("Cards have already been dealt. End the round first.");
            return;
        }

        if (!isDealer)
        {
            Debug.LogWarning("You must join a table as the dealer before dealing cards.");
            return;
        }

        if (blackjackTable == null)
        {
            Debug.LogWarning("No table is currently assigned. Join a table first.");
            return;
        }

        if (!blackjackTable.RefreshOccupiedChairs())
        {
            Debug.LogWarning("Cannot deal cards. No players are seated at the table.");
            return;
        }

        blackjackShoe.InitializeShoe();
        cardsDealt = true;
        StartCoroutine(DealCardsWithDelay());
    }

    private IEnumerator DealCardsWithDelay()
    {
        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        DealCardToSpot(blackjackTable.dealerCardSpot);
        yield return new WaitForSeconds(cardDealingDelay);

        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        DealCardToSpot(blackjackTable.dealerCardSpot, faceDown: true);

        Debug.Log("Hands dealt. Game ready to begin player turns.");
        StartCoroutine(HandlePlayerTurns());
    }

    private IEnumerator HandlePlayerTurns()
    {
        while (currentPlayerIndex < blackjackTable.occupiedChairs.Count)
        {
            Chair chair = blackjackTable.occupiedChairs[currentPlayerIndex];
            NPCBlackjack player = chair.GetComponentInChildren<NPCBlackjack>();

            if (player == null || player.isTurnOver)
            {
                currentPlayerIndex++;
                continue;
            }

            Debug.Log($"Player in Chair {currentPlayerIndex + 1} is taking their turn.");

            int dealerUpCardValue = blackjackTable.GetDealerCardValue();

            if (!player.isWaitingForDealer)
            {
                player.TakeTurn(dealerUpCardValue, this);
            }

            if (player.isWaitingForDealer)
            {
                Debug.Log("Waiting for dealer input (Press D).");
                isAwaitingDealerInput = true;
                while (isAwaitingDealerInput) yield return null;
                DealCardToPlayer(player);
                player.isWaitingForDealer = false;
            }

            if (player.isTurnOver)
            {
                Debug.Log($"Player {currentPlayerIndex + 1}'s turn is over.");
                currentPlayerIndex++;
            }
        }

        Debug.Log("All players have made their choices.");
    }


    private void DealCardToSpot(Transform spot, bool faceDown = false)
    {
        PlayingCardSO card = blackjackShoe.DrawCard();
        if (card == null)
        {
            Debug.LogWarning("No more cards in the shoe!");
            return;
        }

        Vector3 cardOffset = new Vector3(0, 0, -cardGap * spot.childCount);

        GameObject cardObject = Instantiate(card.prefab, spot.position + cardOffset, Quaternion.identity);
        cardObject.transform.SetParent(spot);
        cardObject.transform.localPosition = cardOffset;
        cardObject.transform.localRotation = Quaternion.Euler(0, 90, faceDown ? 180 : 0);

        Debug.Log($"Dealt card: {card.GetCardName()} to {spot.name} with offset {cardOffset}");

        // Update NPC card value only once
        Chair chair = spot.GetComponentInParent<Chair>();
        if (chair != null)
        {
            NPCBlackjack npc = chair.GetComponentInChildren<NPCBlackjack>();
            if (npc != null)
            {
                bool isAce = card.value == 1;
                npc.AddCardValue(card.value, isAce); // Update card value for NPC
            }
        }
        else if (spot == blackjackTable.dealerCardSpot)
        {
            // Update dealer card values
            blackjackTable.AddDealerCard(card);
        }
    }




    private void DealCardToPlayer(NPCBlackjack player)
    {
        PlayingCardSO card = blackjackShoe.DrawCard();
        if (card == null)
        {
            Debug.LogWarning("No more cards in the shoe!");
            return;
        }

        bool isAce = card.value == 1;

        // Add card value to NPC
        player.AddCardValue(card.value, isAce);

        // Instantiate card at the NPC's seatCardSpot
        Transform seatCardSpot = player.GetComponentInParent<Chair>().seatCardSpot;
        if (seatCardSpot != null)
        {
            // Calculate the offset for the new card
            Vector3 cardOffset = new Vector3(0, 0, -cardGap * seatCardSpot.childCount);
            GameObject cardObject = Instantiate(card.prefab, seatCardSpot.position + cardOffset, Quaternion.identity);

            // Parent and position the card
            cardObject.transform.SetParent(seatCardSpot);
            cardObject.transform.localPosition = cardOffset;
            cardObject.transform.localRotation = Quaternion.Euler(0, 90, 0);

            Debug.Log($"Dealt {card.GetCardName()} to Chair {currentPlayerIndex + 1}. Total: {player.totalCardValue}");
        }
        else
        {
            Debug.LogWarning("No seatCardSpot found for this player.");
        }
    }

    private void UnsubscribeInteractionEvents()
    {
        _inputHandler.OnRotatePreviewOrOpenBox -= _playerInteraction.HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace -= _playerInteraction.HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell -= _playerInteraction.HandleBoxOrSell;
        _inputHandler.OnPhoneMenu -= _playerInteraction.TogglePhoneMenu;
        _inputHandler.OnCancel -= _playerInteraction.HandleCancelPlacement;
    }

    private void SubscribeInteractionEvents()
    {
        _inputHandler.OnRotatePreviewOrOpenBox += _playerInteraction.HandleRotateOrOpenBox;
        _inputHandler.OnPickupOrPlace += _playerInteraction.HandlePickupOrPlace;
        _inputHandler.OnBoxOrSell += _playerInteraction.HandleBoxOrSell;
        _inputHandler.OnPhoneMenu += _playerInteraction.TogglePhoneMenu;
        _inputHandler.OnCancel += _playerInteraction.HandleCancelPlacement;
        _playerInteraction.ResetState();
    }
}

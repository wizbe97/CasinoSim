using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackjackDealer : MonoBehaviour
{
    [Header("Blackjack Shoe")]
    [SerializeField] private BlackJackShoeSO blackjackShoe; // Reference to the Blackjack Shoe Scriptable Object

    [Header("Player Interaction")]
    [HideInInspector] public PlayerInputHandler _inputHandler; // Reference to the player input handler
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
    private BlackjackTable assignedTable;
    private bool allPlayersFinished = false; // Tracks if all players have hit or stood
    private List<GameObject> cardsOnTable = new List<GameObject>(); // List to track all card objects
    private DealerScoreUI dealerScoreUI;





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

    private void HandleDealInput()
    {
        // Handle player input during their turn
        if (isAwaitingDealerInput)
        {
            Debug.Log("Dealing card to the current player or dealer.");
            isAwaitingDealerInput = false; // Clear the flag to proceed in the coroutine
            return;
        }

        // Start or continue the dealer's turn after revealing the card
        if (cardsDealt && allPlayersFinished && blackjackTable.IsDealerCardRevealed())
        {
            int dealerScore = blackjackTable.GetDealerCardValue();
            if (dealerScore < 17)
            {
                Debug.Log($"Dealer score is {dealerScore}. Starting dealer's turn.");
                StartCoroutine(HandleDealerTurn());
            }
            else
            {
                Debug.Log($"Dealer score is {dealerScore}. No further action needed.");
                DetermineWinners();
            }
        }
        else if (!cardsDealt)
        {
            Debug.Log("Dealing initial cards.");
            DealCards();
        }
        else if (!allPlayersFinished)
        {
            Debug.LogWarning("All players must finish their turns before proceeding.");
        }
        else if (!blackjackTable.IsDealerCardRevealed())
        {
            Debug.LogWarning("You must reveal the dealer's hidden card before proceeding.");
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
        assignedTable = blackjackTable;

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
        assignedTable = null; // Clear the reference
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
        _inputHandler.OnCancel -= LeaveDealerSpot;
    }

    private IEnumerator DealCardsWithDelay()
    {
        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        // Deal the first card to the dealer (up card)
        PlayingCardSO dealerUpCard = DealCardToSpot(blackjackTable.dealerCardSpot);
        if (dealerUpCard != null)
        {
            blackjackTable.SetDealerUpCardValue(dealerUpCard.value); // Set the up card value
            Debug.Log($"Dealer up card set to: {dealerUpCard.value}");
        }
        yield return new WaitForSeconds(cardDealingDelay);

        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        // Deal the second (face-down) card to the dealer
        DealCardToSpot(blackjackTable.dealerCardSpot, faceDown: true);


        Debug.Log("Hands dealt. Game ready to begin player turns.");
        _inputHandler.OnCancel += LeaveDealerSpot;
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


            int dealerUpCardValue = blackjackTable.GetDealerUpCardValue();

            if (!player.isWaitingForDealer)
            {
                player.TakeTurn(dealerUpCardValue, this);
            }

            if (player.isWaitingForDealer)
            {
                isAwaitingDealerInput = true;
                while (isAwaitingDealerInput) yield return null; // Wait for `D` to be pressed
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
        allPlayersFinished = true; // Mark that all players have finished
        _inputHandler.OnRevealDealerCard += HandleRevealCardInput; // Enable revealing the dealer card
    }


    private PlayingCardSO DealCardToSpot(Transform spot, bool faceDown = false)
    {
        PlayingCardSO card = blackjackShoe.DrawCard();
        if (card == null)
        {
            Debug.LogWarning("No more cards in the shoe!");
            return null;
        }

        Vector3 cardOffset = new Vector3(0, 0, -cardGap * spot.childCount);

        // Instantiate the card
        GameObject cardObject = Instantiate(card.prefab, spot.position + cardOffset, Quaternion.identity);
        cardObject.transform.SetParent(spot);
        cardObject.transform.localPosition = cardOffset;
        cardObject.transform.localRotation = Quaternion.Euler(0, 90, faceDown ? 180 : 0);

        // Track the card for cleanup later
        cardsOnTable.Add(cardObject);

        // Update NPC or dealer values
        Chair chair = spot.GetComponentInParent<Chair>();
        if (chair != null)
        {
            NPCBlackjack npc = chair.GetComponentInChildren<NPCBlackjack>();
            if (npc != null)
            {
                bool isAce = card.value == 1;
                npc.AddCardValue(card.value, isAce);
            }
        }
        else if (spot == blackjackTable.dealerCardSpot)
        {
            blackjackTable.AddDealerCard(card);

            if (!faceDown) // Only update the score UI if the card is face-up
            {
                if (dealerScoreUI == null)
                {
                    dealerScoreUI = blackjackTable.GetComponentInChildren<DealerScoreUI>();
                }

                dealerScoreUI.UpdateDealerScore(); // Update visible dealer score
            }
        }

        return card; // Return the card dealt
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


            // Track the card in the `cardsOnTable` list
            cardsOnTable.Add(cardObject);
        }
        else
        {
            Debug.LogWarning("No seatCardSpot found for this player.");
        }
    }


    private void HandleRevealCardInput()
    {
        Ray ray = _playerInteraction.PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _reticleUI.position));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Ensure we're interacting with the dealer's second card
            if (hitObject.transform.IsChildOf(blackjackTable.dealerCardSpot) &&
                hitObject.transform.GetSiblingIndex() == 1) // Index 1 for the second card
            {
                blackjackTable.RevealDealerCard();
                dealerScoreUI.UpdateDealerScore();

            }
            else
            {
                Debug.Log("Not targeting the hidden dealer card.");
            }
        }
    }

    private IEnumerator HandleDealerTurn()
    {
        while (blackjackTable.GetDealerCardValue() < 17)
        {
            Debug.Log("Dealer score is less than 17. Dealing a card to the dealer.");

            // Deal a card immediately on the first iteration
            DealCardToSpot(blackjackTable.dealerCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);

            // Check if the dealer still needs to hit
            if (blackjackTable.GetDealerCardValue() < 17)
            {
                Debug.Log("Dealer score is still less than 17. Press 'D' to deal another card.");
                isAwaitingDealerInput = true;

                // Wait for the dealer to press 'D' for the next card
                while (isAwaitingDealerInput) yield return null;
            }
        }

        int dealerScore = blackjackTable.GetDealerCardValue();
        Debug.Log($"Dealer's turn ended with a score of {dealerScore}.");

        // Determine winners
        DetermineWinners();
    }


    private void DetermineWinners()
    {
        blackjackTable.DetermineWinners();
        _inputHandler.OnResetRound += ResetRound; // Enable round reset
    }

    public void ResetRound()
    {
        // Destroy all cards on the table
        foreach (GameObject card in cardsOnTable)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }
        cardsOnTable.Clear(); // Clear the list for the next round

        // Reset dealer's score
        if (blackjackTable != null)
        {
            blackjackTable.ResetDealer(); // Reset the dealer's score
        }

        // Reset all players' scores and states
        if (assignedTable != null)
        {
            Debug.Log("Assigned table found. Resetting players and dealer.");

            foreach (Chair chair in assignedTable.occupiedChairs)
            {
                NPCBlackjack player = chair.GetComponentInChildren<NPCBlackjack>();
                if (player != null)
                {
                    Debug.Log($"Resetting player in Chair: {chair.name}, Current Total: {player.totalCardValue}");
                    player.ResetPlayer(); // Reset each player's score
                    Debug.Log($"Player in Chair: {chair.name} has been reset. New Total: {player.totalCardValue}");
                }
            }

            Debug.Log("Resetting dealer state...");
            assignedTable.ResetDealerCardRevealed();
            Debug.Log("Dealer card revealed state reset.");

            assignedTable.ResetDealer();
            Debug.Log("Dealer has been reset.");
            dealerScoreUI.UpdateDealerScore();
        }
        else
        {
            Debug.LogWarning("Assigned table is null. Skipping reset for players and dealer.");
        }



        currentPlayerIndex = 0;
        cardsDealt = false;
        allPlayersFinished = false; // Reset player turn tracking
        Debug.Log("Round reset. Dealer and player scores reset. Ready to deal again.");

        // Unsubscribe actions
        _inputHandler.OnRevealDealerCard -= HandleRevealCardInput;
        _inputHandler.OnResetRound -= ResetRound;
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
        _inputHandler.OnRevealDealerCard -= HandleRevealCardInput;
        _inputHandler.OnResetRound -= ResetRound;
    }
}

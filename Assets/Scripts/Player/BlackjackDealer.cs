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

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start() {
        _reticleUI = _playerInteraction.ReticleUI;
    }

    private void SubscribeDealerInputActions()
    {
        Debug.Log("BlackjackDealer: Subscribing to dealer input actions.");
        _inputHandler.OnDealCard += DealCards;
        _inputHandler.OnCancel += LeaveDealerSpot;
    }

    private void UnsubscribeDealerInputActions()
    {
        Debug.Log("BlackjackDealer: Unsubscribing from dealer input actions.");
        _inputHandler.OnDealCard -= DealCards;
        _inputHandler.OnCancel -= LeaveDealerSpot;
    }

    private void OnEnable()
    {
        _inputHandler.OnJoinTable += JoinTable;
    }
    private void OnDisable()
    {
        UnsubscribeDealerInputActions();
        _inputHandler.OnJoinTable += JoinTable;
    }


    private void JoinTable()
    {
        if (isDealer)
        {
            Debug.LogWarning("Already acting as dealer.");
            return;
        }

        // Cast a ray from the player camera through the reticle
        Ray ray = _playerInteraction.PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _playerInteraction.ReticleUI.position));
        Debug.DrawRay(ray.origin, ray.direction * _interactionDistance, Color.red, 2f); // Debug the ray

        // Use RaycastAll to get all hits
        RaycastHit[] hits = Physics.RaycastAll(ray, _interactionDistance);

        // Filter hits to find the first object on the PlacedObjects layer
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PlacedObjects"))
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log($"Hit object: {hitObject.name}, Layer: {LayerMask.LayerToName(hitObject.layer)}");

                // Search for a BlackjackTable component in the hit object or its parents
                BlackjackTable table = hitObject.GetComponentInParent<BlackjackTable>();

                if (table != null)
                {
                    blackjackTable = table; // Dynamically assign the table
                    dealerSpot = table.dealerCardSpot;

                    if (dealerSpot != null)
                    {
                        TeleportToDealerSpot(table.dealerSeat); // Pass the dealer seat to the teleport method
                    }
                    else
                    {
                        Debug.LogWarning("No DealerSpot found for the blackjack table.");
                    }

                    return; // Exit after finding the first valid table
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
        if (dealerSeat == null)
        {
            return;
        }

        // Teleport the player to the dealer seat
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
        if (!isDealer)
            return;

        _playerController.CanMove = true;

        SubscribeInteractionEvents();
        UnsubscribeDealerInputActions();

        isDealer = false;
        blackjackTable = null;
    }



    public void DealCards()
    {
        if (cardsDealt == true)
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
        cardsDealt = false;

        // Start the coroutine to deal cards with a delay
        StartCoroutine(DealCardsWithDelay());
    }

    private IEnumerator DealCardsWithDelay()
    {
        // First round of cards to all players
        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        // Dealer's first card
        DealCardToSpot(blackjackTable.dealerCardSpot);
        yield return new WaitForSeconds(cardDealingDelay);

        // Second round of cards to all players
        foreach (Chair chair in blackjackTable.occupiedChairs)
        {
            DealCardToSpot(chair.seatCardSpot);
            yield return new WaitForSeconds(cardDealingDelay);
        }

        // Dealer's second card (face down)
        DealCardToSpot(blackjackTable.dealerCardSpot, faceDown: true);

        cardsDealt = true;
        Debug.Log("Hands dealt.");
    }


    private void DealCardToSpot(Transform spot, bool faceDown = false)
    {
        PlayingCardSO card = blackjackShoe.DrawCard();
        if (card == null)
        {
            Debug.LogWarning("No more cards in the shoe!");
            return;
        }

        // Calculate the card's position offset based on the cardGap and number of children at the spot
        Vector3 cardOffset = new Vector3(0, 0, -cardGap * spot.childCount); // Offset in local Z-axis

        // Instantiate the card at the spot's position plus the offset
        GameObject cardObject = Instantiate(card.prefab, spot.position, Quaternion.identity);

        // Parent the card to the spot
        cardObject.transform.SetParent(spot);

        // Adjust the card's local position to add the offset
        cardObject.transform.localPosition = cardOffset;

        // Set the card's local rotation
        cardObject.transform.localRotation = Quaternion.Euler(0, 90, faceDown ? 180 : 0);

        Debug.Log($"Dealt card: {card.GetCardName()} to {spot.name} with offset {cardOffset}");
    }



    public void EndRound()
    {
        if (blackjackTable == null)
        {
            Debug.LogWarning("No table is currently assigned. Cannot end the round.");
            return;
        }

        blackjackTable.RefreshOccupiedChairs();
        cardsDealt = false;
        Debug.Log("Round ended. Ready for next deal.");
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

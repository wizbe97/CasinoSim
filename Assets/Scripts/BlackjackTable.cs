using UnityEngine;
using System.Collections.Generic;

public class BlackjackTable : MonoBehaviour
{
    [Header("Card Spots")]
    public Transform dealerCardSpot; // Transform for the dealer's card spot
    public Transform dealerSeat;

    [Header("Chair Management")]
    public Chair[] chairs; // Array of Chair components representing the seats
    public List<Chair> occupiedChairs = new List<Chair>(); // List of occupied chairs
    public Transform DealerSeat;

    [SerializeField] private float cardDealingDelay = 0.5f; // Delay between dealing cards

    public float CardDealingDelay => cardDealingDelay;

    private int dealerTotalValue = 0; // Total value of the dealer's cards
    private int dealerAceCount = 0; // Number of Aces in the dealer's hand

    private bool dealerCardRevealed = false;
    private int dealerUpCardValue = 0; // Store the dealer's up card value



    private void Awake()
    {
        if (dealerCardSpot == null)
            Debug.LogError("DealerCardSpot is not assigned in BlackjackTable!");
        if (chairs == null || chairs.Length == 0)
            Debug.LogError("Chairs are not assigned in BlackjackTable!");
    }

    public bool GameCanStart(out List<Chair> occupiedChairs)
    {
        occupiedChairs = new List<Chair>();

        foreach (Chair chair in chairs)
        {
            if (chair != null && chair.IsOccupied)
            {
                occupiedChairs.Add(chair);
            }
        }

        return occupiedChairs.Count > 0;
    }

    public Chair FindFreeChair()
    {
        foreach (Chair chair in chairs)
        {
            if (chair != null && !chair.IsOccupied)
            {
                return chair;
            }
        }

        return null;
    }

    public bool RefreshOccupiedChairs()
    {
        occupiedChairs.Clear();

        foreach (Chair chair in chairs)
        {
            if (chair != null && chair.IsOccupied)
            {
                occupiedChairs.Add(chair);
            }
        }

        return occupiedChairs.Count > 0;
    }

    /// <summary>
    /// Updates the dealer's total card value when a card is dealt.
    /// </summary>
    public void AddDealerCard(PlayingCardSO card)
    {
        if (card == null)
        {
            Debug.LogWarning("Trying to add a null card to the dealer.");
            return;
        }

        // Check if the card is an Ace
        if (card.value == 1)
        {
            dealerAceCount++;
        }

        dealerTotalValue += card.value;
        Debug.Log($"Dealer received card: {card.GetCardName()}. Current total: {GetDealerCardValue()}");
    }

    /// <summary>
    /// Resets the dealer's total value and Ace count for a new round.
    /// </summary>
    public void ResetDealer()
    {
        dealerTotalValue = 0;
        dealerAceCount = 0;
    }

    /// <summary>
    /// Calculates and returns the dealer's current card value, accounting for soft totals with Aces.
    /// </summary>
    public int GetDealerCardValue()
    {
        int softValue = dealerTotalValue;
        if (dealerAceCount > 0 && dealerTotalValue + 10 <= 21)
        {
            softValue = dealerTotalValue + 10; // Treat one Ace as 11 if it doesn't bust
        }

        return softValue;
    }

    public void RevealDealerCard()
    {
        // Assuming the second dealer card is always the second child of dealerCardSpot
        if (dealerCardSpot.childCount > 1)
        {
            Transform hiddenCard = dealerCardSpot.GetChild(1); // Second card
            hiddenCard.localRotation = Quaternion.Euler(0, 0, 0); // Rotate to reveal (front-facing)
            Debug.Log($"Revealed dealer's hidden card. Total dealer value: {GetDealerCardValue()}");

            dealerCardRevealed = true; // Mark the card as revealed
        }
        else
        {
            Debug.LogWarning("No hidden card to reveal!");
        }
    }


    public bool IsDealerCardRevealed()
    {
        return dealerCardRevealed;
    }
    public void ResetDealerCardRevealed()
    {
        dealerCardRevealed = false;

    }

    public void SetDealerUpCardValue(int value)
    {
        dealerUpCardValue = value;
    }

    public int GetDealerUpCardValue()
    {
        return dealerUpCardValue;
    }

}

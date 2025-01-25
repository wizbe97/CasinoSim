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
        dealerUpCardValue = 0; // Ensure reset
        Debug.Log("ResetDealer called. dealerUpCardValue reset to 0.");
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

        if (dealerTotalValue >= 17)
        {
            DetermineWinners();
            BlackjackDealer dealer = FindAnyObjectByType<BlackjackDealer>();
            if (dealer != null && dealer._inputHandler != null)
            {
                dealer._inputHandler.OnResetRound += dealer.ResetRound;
            }
        }
    }

    public void DetermineWinners()
    {
        int dealerScore = GetDealerCardValue();

        foreach (Chair chair in occupiedChairs)
        {
            NPCBlackjack player = chair.GetComponentInChildren<NPCBlackjack>();
            if (player == null) continue;

            // Retrieve the player's UI
            NPCBlackjackUI ui = player.GetComponent<NPCBlackjackUI>();
            if (ui == null)
            {
                Debug.LogWarning($"No UI found for player at Chair {chair.name}");
                continue;
            }

            // Determine the result and update UI
            if (player.IsBusted())
            {
                Debug.Log($"Player at Chair {chair.name} busted with {player.totalCardValue}. Dealer wins.");
                ui.UpdateDecisionColor(Color.red); // Red for losing
            }
            else if (dealerScore > 21 || player.totalCardValue > dealerScore)
            {
                Debug.Log($"Player at Chair {chair.name} wins with {player.totalCardValue} against dealer's {dealerScore}.");
                ui.UpdateDecisionColor(Color.green); // Green for winning
            }
            else if (player.totalCardValue == dealerScore)
            {
                Debug.Log($"Player at Chair {chair.name} ties with the dealer. Score: {player.totalCardValue}.");
                ui.UpdateDecisionColor(Color.yellow); // Yellow for tie
            }
            else
            {
                Debug.Log($"Player at Chair {chair.name} loses with {player.totalCardValue} to dealer's {dealerScore}.");
                ui.UpdateDecisionColor(Color.red); // Red for losing
            }
        }

        Debug.Log("All winners and losers determined.");
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
        Debug.Log($"SetDealerUpCardValue called. dealerUpCardValue set to: {dealerUpCardValue}");
    }



    public int GetDealerUpCardValue()
    {
        Debug.Log($"GetDealerUpCardValue called. Returning: {dealerUpCardValue}");
        return dealerUpCardValue;
    }



}

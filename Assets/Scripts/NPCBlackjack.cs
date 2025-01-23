using UnityEngine;

public class NPCBlackjack : MonoBehaviour
{
    public float walkSpeed = 2f; // Speed at which the NPC walks
    private BlackjackTable targetTable;
    private Chair targetChair; // Now stores the Chair object instead of just the Transform
    private bool isSeated = false;

    // Blackjack-specific properties
    public int totalCardValue = 0; // Total value of the NPC's cards
    private int aceCount = 0; // Number of Aces for soft total calculations
    public bool isTurnOver = false; // Tracks whether the NPC has finished their turn
    public bool isWaitingForDealer = false; // Tracks if waiting for the dealer to deal a card

    private NPCBlackjackUI ui; // Reference to the NPC UI script

    private void Start()
    {
        targetTable = FindObjectOfType<BlackjackTable>();
        ui = GetComponent<NPCBlackjackUI>(); // Initialize the UI reference
    }

    private void Update()
    {
        if (isSeated || targetTable == null) return;

        MoveTowardsTable();

        if (Vector3.Distance(transform.position, targetTable.transform.position) < 3f)
        {
            SitAtTable();
        }
    }

    private void MoveTowardsTable()
    {
        Vector3 direction = (targetTable.transform.position - transform.position).normalized;
        transform.position += direction * walkSpeed * Time.deltaTime;
    }

    private void SitAtTable()
    {
        targetChair = targetTable.FindFreeChair();
        if (targetChair != null)
        {
            targetChair.SitNPC(transform);
            isSeated = true;
        }
    }

    public void AddCardValue(int cardValue, bool isAce = false)
    {
        if (isAce) aceCount++;
        totalCardValue += cardValue;

        if (ui != null)
        {
            ui.AddCardValue(cardValue, isAce);
        }
    }

    public bool IsBusted()
    {
        return totalCardValue > 21;
    }

    public bool DecideToHit(int dealerUpCardValue)
    {
        int hardTotal = totalCardValue;
        int softTotal = (aceCount > 0 && totalCardValue + 10 <= 21) ? totalCardValue + 10 : totalCardValue;

        // Check for natural blackjack
        if (hardTotal == 21 || softTotal == 21)
        {
            Debug.Log($"Player has a natural blackjack (21). Automatically standing.");
            UpdateUIBasedOnDecision(false, Color.yellow);
            return false; // Automatically stand on blackjack
        }

        bool shouldHit = false;

        // Decision logic based on dealer's up card
        if (softTotal >= 17 && softTotal <= 20 && dealerUpCardValue != 1)
        {
            // Always stand on soft 17, 18, 19, or 20 unless dealer has an Ace
            Debug.Log($"- Decision: Stand on Soft Total {softTotal} against Dealer Card {dealerUpCardValue}");
            shouldHit = false;
        }
        else if (dealerUpCardValue >= 7)
        {
            // Dealer shows 7 or higher
            shouldHit = hardTotal <= 16; // Hit on hard 16 or less
        }
        else if (dealerUpCardValue >= 4 && dealerUpCardValue <= 6)
        {
            // Dealer shows 4-6 (weak hand)
            if (hardTotal >= 12 && hardTotal <= 20)
            {
                // Stand on hard 13-20 vs 4-6
                shouldHit = false;
                Debug.Log($"- Decision: Stand on Hard Total {hardTotal} against Dealer's 4-6");
            }
            else
            {
                shouldHit = hardTotal <= 11 || softTotal <= 16; // Hit on hard 11 or less, or soft 16 or less
                Debug.Log($"- Decision: {(shouldHit ? "Hit" : "Stand")} on Hard Total {hardTotal} or Soft Total {softTotal} vs Dealer's 4-6");
            }
        }
        else if (dealerUpCardValue == 2 || dealerUpCardValue == 3)
        {
            // Dealer shows 2-3
            if (hardTotal >= 13)
            {
                // Stand on hard 13+ vs 2-3
                shouldHit = false;
                Debug.Log($"- Decision: Stand on Hard Total {hardTotal} against Dealer's 2-3");
            }
            else
            {
                shouldHit = hardTotal <= 12 || softTotal <= 16; // Hit on hard 12 or less, or soft 16 or less
                Debug.Log($"- Decision: {(shouldHit ? "Hit" : "Stand")} on Hard Total {hardTotal} or Soft Total {softTotal} vs Dealer's 2-3");
            }
        }
        else if (dealerUpCardValue == 1)
        {
            // Dealer shows Ace
            shouldHit = hardTotal <= 15 || softTotal <= 18; // Hit on hard 15 or less, or soft 18 or less
            Debug.Log($"- Decision: {(shouldHit ? "Hit" : "Stand")} on Hard Total {hardTotal} or Soft Total {softTotal} vs Dealer's Ace");
        }

        Debug.Log($"[DecideToHit] Dealer Up Card: {dealerUpCardValue}, Hard Total: {hardTotal}, Soft Total: {softTotal}, Decision: {(shouldHit ? "Hit" : "Stand")}");

        // Update UI based on decision
        UpdateUIBasedOnDecision(shouldHit);

        return shouldHit;
    }

    private void UpdateUIBasedOnDecision(bool shouldHit)
    {
        if (ui != null)
        {
            if (IsBusted())
            {
                ui.PlayerBusted();
            }
            else if (totalCardValue == 21 || (aceCount > 0 && totalCardValue + 10 == 21))
            {
                // Perfect hand: 21
                ui.UpdateDecisionColor(Color.yellow); // Highlight yellow for blackjack
            }
            else if (shouldHit)
            {
                ui.UpdateDecisionColor(Color.green); // Highlight green for hit
            }
            else
            {
                ui.UpdateDecisionColor(Color.red); // Highlight red for stand
            }
        }
    }

    // Overloaded method to specify a custom color
    private void UpdateUIBasedOnDecision(bool shouldHit, Color customColor)
    {
        if (ui != null)
        {
            ui.UpdateDecisionColor(customColor);
        }
    }

    public void TakeTurn(int dealerUpCardValue, BlackjackDealer dealer)
    {
        if (IsBusted())
        {
            Debug.Log($"Player in Chair {targetChair.name} has busted with {totalCardValue}.");
            isTurnOver = true;
            UpdateUIBasedOnDecision(false);
            return;
        }

        if (DecideToHit(dealerUpCardValue))
        {
            Debug.Log($"Player in Chair {targetChair.name} decided to hit.");
            isWaitingForDealer = true;
        }
        else
        {
            Debug.Log($"Player in Chair {targetChair.name} decided to stand with {totalCardValue}.");
            isTurnOver = true;
        }
    }

    public void ResetPlayer()
    {
        totalCardValue = 0;
        aceCount = 0;
        isTurnOver = false;
        isWaitingForDealer = false;

        if (ui != null)
        {
            ui.ResetCardValue();
        }
    }
}

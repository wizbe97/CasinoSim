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
        Debug.Log($"DecideToHit called. dealerUpCardValue: {dealerUpCardValue}");

        int hardTotal = totalCardValue;
        int softTotal = (aceCount > 0 && totalCardValue + 10 <= 21) ? totalCardValue + 10 : totalCardValue;

        // Always stand on 17, 18, 19, 20, or 21 (soft or hard)
        if (softTotal >= 17 || hardTotal >= 17)
        {
            UpdateUIBasedOnDecision(false, Color.red); // Stand
            return false; // Stand
        }

        // Stand on 12 or higher if dealer's card is explicitly 2, 3, 4, 5, or 6
        if ((dealerUpCardValue == 2 || dealerUpCardValue == 3 || dealerUpCardValue == 4 ||
             dealerUpCardValue == 5 || dealerUpCardValue == 6) && hardTotal >= 12)
        {
            UpdateUIBasedOnDecision(false, Color.red); // Stand
            return false; // Stand
        }

        // Hit on 16 or lower with dealer showing 7, 8, 9, or 10
        if (dealerUpCardValue == 7 || dealerUpCardValue == 8 || dealerUpCardValue == 9 || dealerUpCardValue == 10)
        {
            if (hardTotal <= 16 || softTotal <= 16)
            {
                UpdateUIBasedOnDecision(true, Color.green); // Hit
                return true; // Hit
            }

            // Stand on 17 or higher
            UpdateUIBasedOnDecision(false, Color.red); // Stand
            return false; // Stand
        }

        // Otherwise, decide to hit
        UpdateUIBasedOnDecision(true, Color.green); // Hit
        return true; // Hit
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
            isTurnOver = true;
            UpdateUIBasedOnDecision(false);
            return;
        }

        if (DecideToHit(dealerUpCardValue))
        {
            isWaitingForDealer = true;
        }
        else
        {
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

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

    private void Start()
    {
        // Find the first BlackjackTable in the scene
        targetTable = FindObjectOfType<BlackjackTable>();
    }

    private void Update()
    {
        if (isSeated || targetTable == null) return;

        // Move towards the table
        MoveTowardsTable();

        // Check if close enough to the table to sit down
        if (Vector3.Distance(transform.position, targetTable.transform.position) < 3f)
        {
            SitAtTable();
        }
    }

    private void MoveTowardsTable()
    {
        // Walk towards the table's position
        Vector3 direction = (targetTable.transform.position - transform.position).normalized;
        transform.position += direction * walkSpeed * Time.deltaTime;
    }

    private void SitAtTable()
    {
        // Find a free chair at the table
        targetChair = targetTable.FindFreeChair();

        if (targetChair != null)
        {
            // Sit at the chair
            targetChair.SitNPC(transform);
            isSeated = true; // Disable movement
        }
    }

    public void AddCardValue(int cardValue, bool isAce = false)
    {
        if (isAce) aceCount++;
        totalCardValue += cardValue;

        // Update the UI
        NPCBlackjackUI ui = GetComponent<NPCBlackjackUI>();
        if (ui != null)
        {
            ui.AddCardValue(cardValue, isAce);
        }
    }

    public bool IsBusted()
    {
        // Check if the total card value exceeds 21
        return totalCardValue > 21;
    }

    public bool DecideToHit(int dealerUpCardValue)
    {
        // Calculate hard and soft totals
        int hardTotal = totalCardValue;
        int softTotal = (aceCount > 0 && totalCardValue + 10 <= 21) ? totalCardValue + 10 : totalCardValue;

        // Use Blackjack strategy chart logic
        if (softTotal <= 21)
        {
            // Use the soft total for decision-making
            if (softTotal >= 17 || (softTotal >= 13 && dealerUpCardValue <= 6))
            {
                Debug.Log($"[Soft Total] Player stands on {softTotal}");
                return false; // Stand
            }
        }

        if (hardTotal <= 11 || (hardTotal == 12 && dealerUpCardValue >= 4 && dealerUpCardValue <= 6))
        {
            Debug.Log($"[Hard Total] Player hits on {hardTotal}");
            return true; // Hit
        }

        bool shouldHit = hardTotal <= 16 && dealerUpCardValue >= 7;
        Debug.Log($"[Hard Total] Player {(shouldHit ? "hits" : "stands")} on {hardTotal}");
        return shouldHit;
    }

    public void TakeTurn(int dealerUpCardValue, BlackjackDealer dealer)
    {
        if (IsBusted())
        {
            Debug.Log($"Player in Chair {targetChair.name} has busted with {totalCardValue}.");
            isTurnOver = true;
            return;
        }

        if (DecideToHit(dealerUpCardValue))
        {
            Debug.Log($"Player in Chair {targetChair.name} decided to hit.");
            isWaitingForDealer = true; // Wait for the dealer to deal a card
        }
        else
        {
            Debug.Log($"Player in Chair {targetChair.name} decided to stand with {totalCardValue}.");
            isTurnOver = true; // End the turn
        }
    }

    public void ResetPlayer()
    {
        totalCardValue = 0;
        aceCount = 0;
        isTurnOver = false;
        isWaitingForDealer = false;

        // Reset the UI
        NPCBlackjackUI ui = GetComponent<NPCBlackjackUI>();
        if (ui != null)
        {
            ui.ResetCardValue();
        }
    }

}

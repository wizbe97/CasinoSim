using UnityEngine;
using TMPro;

public class NPCBlackjackUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI valueText; // Text element for card value
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // Offset for the text position

    private int totalCardValue = 0; // Total value of the cards
    private int aceCount = 0; // Number of Aces in the hand
    private Transform npcTransform;

    private void Start()
    {
        npcTransform = transform;

        // Initialize the UI text as hidden
        valueText.text = "";
        valueText.enabled = false;
    }

    private void LateUpdate()
    {
        // Ensure the UI stays above the NPC
        if (valueText != null && npcTransform != null)
        {
            valueText.transform.position = npcTransform.position + offset;
        }
    }

    public void AddCardValue(int cardValue, bool isAce = false)
    {
        if (isAce)
        {
            aceCount++; // Track Aces
        }
        totalCardValue += cardValue; // Add card value (Ace initially as 1)

        UpdateCardValueDisplay();
    }

    public void ResetCardValue()
    {
        totalCardValue = 0;
        aceCount = 0;

        // Hide the text when no cards are present
        valueText.enabled = false;
        valueText.text = "";
        valueText.color = Color.white; // Reset color to default
    }

    private void UpdateCardValueDisplay()
    {
        // If no cards, hide the UI
        if (totalCardValue == 0)
        {
            valueText.enabled = false;
            return;
        }

        // Show the text
        valueText.enabled = true;

        // Calculate the soft and hard values
        int softValue = totalCardValue;
        if (aceCount > 0)
        {
            softValue = totalCardValue + 10; // Treat one Ace as 11
        }

        // Check if the total is exactly 21
        if (softValue == 21 || totalCardValue == 21)
        {
            valueText.text = "21"; // Perfect score
            valueText.color = Color.yellow; // Highlight the text
            return;
        }

        // Display logic for hands with Aces
        if (aceCount > 0 && softValue <= 21 && softValue != totalCardValue)
        {
            // Show both hard and soft values if softValue is valid and different
            valueText.text = $"{totalCardValue}/{softValue}";
        }
        else
        {
            // Show only the hard value
            valueText.text = $"{totalCardValue}";
        }

        // Reset color to default for non-perfect scores
        valueText.color = Color.white;
    }
}

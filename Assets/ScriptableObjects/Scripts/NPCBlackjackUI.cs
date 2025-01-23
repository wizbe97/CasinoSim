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
        valueText.fontStyle = FontStyles.Normal; // Reset font style
    }

    private void UpdateCardValueDisplay()
    {
        if (totalCardValue == 0)
        {
            valueText.enabled = false;
            return;
        }

        valueText.enabled = true;

        int softValue = totalCardValue;
        if (aceCount > 0 && totalCardValue + 10 <= 21)
        {
            softValue = totalCardValue + 10;
        }

        if (softValue == 21 || totalCardValue == 21)
        {
            valueText.text = "21";
            valueText.color = Color.yellow; // Highlight the text for a perfect score
            return;
        }

        if (aceCount > 0 && softValue != totalCardValue)
        {
            valueText.text = $"{totalCardValue}/{softValue}";
        }
        else
        {
            valueText.text = $"{totalCardValue}";
        }

        valueText.color = Color.white; // Default color
    }

    public void UpdateDecisionColor(Color color)
    {
        if (valueText != null)
        {
            valueText.color = color; // Update color based on decision
        }
    }

    public void PlayerBusted()
    {
        if (valueText != null)
        {
            valueText.fontStyle = FontStyles.Strikethrough; // Apply strikethrough for bust
            valueText.color = Color.gray; // Set color to gray
        }

        DespawnCards();
    }

    private void DespawnCards()
    {
        // Logic to remove or hide the NPC's cards
        Transform seatCardSpot = GetComponentInParent<Chair>()?.seatCardSpot;
        if (seatCardSpot != null)
        {
            foreach (Transform child in seatCardSpot)
            {
                Destroy(child.gameObject); // Remove all card objects
            }
        }
    }
}

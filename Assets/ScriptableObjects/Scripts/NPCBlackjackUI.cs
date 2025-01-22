using UnityEngine;
using TMPro;

public class NPCBlackjackUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI valueText; // Text element for card value
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // Offset for the text position

    private int totalCardValue = 0; // Total value of the cards
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

    // Method to add a card value dynamically
    public void AddCardValue(int cardValue)
    {
        totalCardValue += cardValue;

        // Show the text only when there is at least one card
        if (totalCardValue > 0)
        {
            valueText.enabled = true;
            valueText.text = totalCardValue.ToString();
        }
    }

    // Method to reset the card value (e.g., at the end of a round)
    public void ResetCardValue()
    {
        totalCardValue = 0;

        // Hide the text when no cards are present
        valueText.enabled = false;
        valueText.text = "";
    }
}

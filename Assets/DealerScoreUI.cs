using UnityEngine;
using TMPro;

public class DealerScoreUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI dealerScoreText; // Reference to the TextMeshProUGUI component

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white; // Default color
    [SerializeField] private Color bustedColor = Color.red; // Color when the dealer busts
    [SerializeField] private Color activeColor = new Color(1f, 0.75f, 0.8f); // Pink color when dealer is active and not busted

    private BlackjackTable blackjackTable;

    private void Start()
    {
        blackjackTable = GetComponentInParent<BlackjackTable>();

        if (dealerScoreText == null)
        {
            Debug.LogError("DealerScoreUI: dealerScoreText is not assigned in the inspector!");
        }

        if (blackjackTable == null)
        {
            Debug.LogError("DealerScoreUI: No BlackjackTable found on parent!");
        }

        // Initialize the UI
        UpdateDealerScore();
    }

    /// <summary>
    /// Updates the dealer's score text and color.
    /// </summary>
    public void UpdateDealerScore()
    {
        if (blackjackTable == null || dealerScoreText == null) return;

        // Get the dealer's total score
        int dealerScore = blackjackTable.GetDealerCardValue();

        // Update the text
        dealerScoreText.text = $"Dealer: {dealerScore}";

        // Check if the dealer has busted
        if (dealerScore > 21)
        {
            dealerScoreText.color = bustedColor;
        }
        else
        {
            dealerScoreText.color = activeColor;
        }
    }
}

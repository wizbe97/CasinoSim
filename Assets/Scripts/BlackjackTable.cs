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

    private void Awake()
    {
        if (dealerCardSpot == null)
        {
            Debug.LogError("DealerCardSpot is not assigned in BlackjackTable!");
        }

        if (chairs == null || chairs.Length == 0)
        {
            Debug.LogError("Chairs are not assigned in BlackjackTable!");
        }
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

        Debug.LogWarning("No free chairs found at the table.");
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
}

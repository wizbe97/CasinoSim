using UnityEngine;

public class Chair : MonoBehaviour
{
    [Header("Chair Settings")]
    public bool IsOccupied = false; // Whether the chair is occupied
    public Transform seatCardSpot; // The spot where cards will be placed for this chair

    public void SitNPC(Transform npc)
    {
        // Parent the NPC to the chair
        npc.SetParent(transform);

        // Position the NPC correctly
        npc.localPosition = new Vector3(0, 2, 0); // Adjust for seating height
        npc.localRotation = Quaternion.identity;

        // Mark the chair as occupied
        IsOccupied = true;

        Debug.Log($"{npc.name} is now seated at {name}.");
    }

    public void LeaveNPC()
    {
        // Unparent the NPC
        foreach (Transform child in transform)
        {
            if (child.CompareTag("NPC")) // Or however NPCs are identified
            {
                child.SetParent(null);
            }
        }

        // Mark the chair as unoccupied
        IsOccupied = false;

        Debug.Log($"Chair {name} is now unoccupied.");
    }
}

using UnityEngine;

public class Chair : MonoBehaviour
{
    public bool IsOccupied = false; // Tracks if the chair is occupied

    public void SitNPC(Transform npc)
    {
        // Parent the NPC to the chair
        npc.SetParent(transform);

        // Set NPC position to sit properly
        npc.localPosition = new Vector3(0, 2, 0); // Adjust Y position for seating height
        npc.localRotation = Quaternion.identity;

        // Mark the chair as occupied
        IsOccupied = true;

        Debug.Log($"{npc.name} is now seated at {name}.");
    }

    public void LeaveNPC(Transform npc)
    {
        // Unparent the NPC
        npc.SetParent(null);

        // Mark the chair as unoccupied
        IsOccupied = false;

        Debug.Log($"{npc.name} has left {name}.");
    }
}

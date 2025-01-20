using UnityEngine;

public class BlackjackTable : MonoBehaviour
{
    public Transform[] chairs; // Assign all chair transforms in the inspector

    public Transform FindFreeChair()
    {
        // Loop through chairs and find the first free one
        foreach (Transform chair in chairs)
        {
            Chair chairScript = chair.GetComponent<Chair>();
            if (chairScript != null && !chairScript.IsOccupied)
            {
                return chair;
            }
        }
        return null; // No free chairs found
    }
}

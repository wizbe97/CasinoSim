using UnityEngine;

public class NPCBlackjack : MonoBehaviour
{
    public float walkSpeed = 2f; // Speed at which the NPC walks
    private BlackjackTable targetTable;
    private Chair targetChair; // Now stores the Chair object instead of just the Transform
    private bool isSeated = false;

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
}

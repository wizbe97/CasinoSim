using UnityEngine;

public class PlacedTable : MonoBehaviour
{
    private bool canBePickedUp = false;

    public void SetPickupCooldown(float cooldown)
    {
        canBePickedUp = false;
        Invoke(nameof(EnablePickup), cooldown);
    }

    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    public void EnablePickup()
    {
        canBePickedUp = true;
    }
}

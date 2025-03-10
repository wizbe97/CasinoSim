using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBalanceManager", menuName = "Game/PlayerBalanceManager")]
public class PlayerBalanceManagerSO : ScriptableObject
{
    public PlayerBalanceSO playerBalance;
    public GameEventSO onBalanceChangedEvent;

    public bool CanAfford(int amount)
    {
        return playerBalance.balance >= amount;
    }

    public void DeductBalance(int amount)
    {
        if (CanAfford(amount))
        {
            playerBalance.balance -= amount;
            if (onBalanceChangedEvent != null)
                onBalanceChangedEvent.Raise();

        }
    }

    public void AddBalance(int amount)
    {
        playerBalance.balance += amount;

        if (onBalanceChangedEvent != null)
            onBalanceChangedEvent.Raise();
    }

    public void ClearBalance()
    {
        Debug.Log("Starting Empty");
        playerBalance.balance = playerBalance.startingBalace;
        if (onBalanceChangedEvent != null)
            onBalanceChangedEvent.Raise();
    }
}

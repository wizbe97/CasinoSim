using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BlackJackShoe", menuName = "Card Game/Blackjack Shoe")]
public class BlackJackShoeSO : ScriptableObject
{
    [Header("Packs of Cards")]
    public List<PackOfCardsSO> packs;

    private List<PlayingCardSO> shoeCards = new List<PlayingCardSO>();

    public void InitializeShoe()
    {
        shoeCards.Clear();

        foreach (PackOfCardsSO pack in packs)
        {
            if (pack != null && pack.cards != null)
            {
                shoeCards.AddRange(pack.cards);
            }
        }

        ShuffleShoe();
    }

    public void ShuffleShoe()
    {
        for (int i = 0; i < shoeCards.Count; i++)
        {
            PlayingCardSO temp = shoeCards[i];
            int randomIndex = Random.Range(0, shoeCards.Count);
            shoeCards[i] = shoeCards[randomIndex];
            shoeCards[randomIndex] = temp;
        }

        Debug.Log("Blackjack shoe shuffled.");
    }

    public PlayingCardSO DrawCard()
    {
        if (shoeCards.Count == 0)
        {
            Debug.LogWarning("The shoe is empty!");
            return null;
        }

        PlayingCardSO drawnCard = shoeCards[0];
        shoeCards.RemoveAt(0);
        Debug.Log("Card withdrawn: " + drawnCard.GetCardName());
        return drawnCard;
    }


    public int CardsRemaining()
    {
        return shoeCards.Count;
    }
}

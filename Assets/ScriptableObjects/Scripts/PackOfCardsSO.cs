using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PackOfCards", menuName = "Card Game/Pack of Cards")]
public class PackOfCardsSO : ScriptableObject
{
    public List<PlayingCardSO> cards;
    public void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            PlayingCardSO temp = cards[i];
            int randomIndex = Random.Range(0, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    public PlayingCardSO DrawCard()
    {
        if (cards.Count == 0)
        {
            Debug.LogWarning("The deck is empty!");
            return null;
        }

        PlayingCardSO drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }
}

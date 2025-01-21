using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Game/Card")]
public class PlayingCardSO : ScriptableObject
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public Suit suit;          // Card's suit
    public int value;          // Card's value
    public GameObject prefab;  // Prefab of the card for instantiation

    public string GetCardName()
    {
        return $"{suit} {value}";
    }
}

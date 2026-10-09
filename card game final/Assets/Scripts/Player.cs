using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    public List<CardData> hand = new List<CardData>();

    public void SortHand()
    {
        if (hand.Count != 0)
        {
            hand.Sort((x, y) => x.rank.CompareTo(y.rank));
        }
    }

    
    public CardData PlayCard(CardData lastPlayed)
    {
        foreach (CardData card in hand) // Sorted low-high
        {
            
            //Debug.Log(card.rank);
            if (card.rank > lastPlayed.rank)
            {
                hand.Remove(card);
                return card; // Put down on top of the placed card
            }
        }
        return null; // Nothing beats the placed card, pass
    }
}

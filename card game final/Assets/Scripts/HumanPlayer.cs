using UnityEngine;

public class HumanPlayer : Player
{

    public override CardData PlayCard(CardData lastPlayed)
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

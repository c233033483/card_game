using UnityEngine;

public class AIPlayer : Player
{
    public override CardData PlayCard()
    {
        foreach (CardData card in hand) // Sorted low-high
        {
            if (Pile.instance.IsCardValid(card))
            {
                hand.Remove(card);
                return card; // Put down on top of the placed card
            }
        }
        return null; // Nothing beats the placed card, pass
    }
}

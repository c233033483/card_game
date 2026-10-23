using System.Linq;
using UnityEngine;

public class HumanPlayer : Player
{

    public override CardData PlayCard()
    {
        foreach (var card in hand.Where(card => Pile.instance.IsCardValid(card)))
        {
            hand.Remove(card);
            return card; // Put down on top of the placed card
        }

        return null; // Nothing beats the placed card, pass
    }
}

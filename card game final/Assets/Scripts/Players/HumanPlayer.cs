using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class HumanPlayer : Player
{
    public override IEnumerator PlayCard(Action<CardData> onDecided)
    {
        var input = HumanTurnInput.instance;
        input.Clear();
        
        while (true)
        {
            yield return new WaitUntil(() => input.clickedCard != null || input.passClicked);

            if (input.passClicked)
            {
                onDecided(null);
                yield break;
            }

            var card = input.clickedCard;
            if (hand.Contains(card) && Pile.instance.IsCardValid(card))
            {
                onDecided(card);
                yield break;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanPlayer : Player
{
    public override IEnumerator PlayCard(Move currentPlay, Action<Move> onDecided)
    {
        var input = HumanTurnInput.Instance;
        input.Clear();
        
        while (true)
        {
            yield return new WaitUntil(() => input.clickedCard != null || input.passClicked);

            if (input.passClicked)
            {
                onDecided(Move.Pass());
                yield break;
            }

            CardData card = input.clickedCard;
            var move = new Move(new List<CardData> { card });

            if (hand.Contains(card) && Rules.IsLegal(move, currentPlay))
            {
                onDecided(move);
                yield break;
            }

            input.Clear(); // clear the invalid click and wait for the next one
        }
    }
}

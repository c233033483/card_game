using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanPlayer : Player
{
    public override IEnumerator PlayCard(Move currentPlay, bool mustMatch, Action<Move> onDecided)
    {
        var input = HumanTurnInput.Instance;
        input.Clear();
        
        while (true)
        {
            yield return new WaitUntil(() => input.playClicked || input.passClicked);

            if (input.passClicked)
            {
                input.Clear();
                input.ClearSelection();
                
                onDecided(Move.Pass());
                yield break;
            }

            input.playClicked = false;
            if (input.Selected.Count == 0) continue;
            
            var move = new Move(new List<CardData>(input.Selected));
            var inHand = move.Cards.TrueForAll(c => hand.Contains(c));
            bool legal = mustMatch ? Rules.IsSkip(move, currentPlay) : Rules.IsLegal(move, currentPlay);
            
            if (inHand && legal)
            {
                input.ClearSelection();
                
                onDecided(move);
                yield break;
            }

            input.Clear(); // clear the invalid click and wait for the next one
        }
    }
}

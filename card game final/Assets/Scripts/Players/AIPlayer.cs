using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    public override IEnumerator PlayCard(Move currentPlay, bool mustMatch, Action<Move> onDecided)
    {
        yield return new WaitForSeconds(0.5f);
        
        onDecided(ChooseMove(currentPlay, mustMatch));
    }

    private Move ChooseMove(Move currentPlay, bool mustMatch)
    {
        bool isLeading = currentPlay == null || currentPlay.IsPass;

        // hand is sorted, group by rank
        var groups = hand.GroupBy(c => c.rank);

        foreach (var group in groups)
        {
            var cards = group.ToList();

            // if leading, size of group, if following, same as pile
            int size = isLeading ? Mathf.Min(cards.Count, GameRules.MAX_CARDS_IN_MOVE) : currentPlay.Cards.Count;

            if (cards.Count < size) continue;
            
            var move = new Move(cards.GetRange(0, size));

            bool legal = mustMatch ? Rules.IsSkip(move, currentPlay) : Rules.IsLegal(move, currentPlay);
            if (legal)
            {
                return move;
            }
        }
        
        return Move.Pass();
    }
}

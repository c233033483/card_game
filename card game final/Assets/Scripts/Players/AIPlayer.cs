using System;
using System.Collections;
using UnityEngine;

public class AIPlayer : Player
{
    public override IEnumerator PlayCard(Move currentPlay, bool mustMatch, Action<Move> onDecided)
    {
        yield return new WaitForSeconds(0.5f);
        
        bool isLeading = currentPlay == null || currentPlay.IsPass;
        int size = isLeading ? 1 : currentPlay.Cards.Count; // defaut 1 card for now until rules updates in future
        
        for (int i = 0; i + size <= hand.Count; i++)
        {
            // Just picks the first card that can be played,
            // will be moved to a different class soon when pairs and triples can be placed
            
            var move = new Move(hand.GetRange(i, size));
            
            bool legal = mustMatch ? Rules.IsSkip(move, currentPlay) : Rules.IsLegal(move, currentPlay);
            if (legal)
            {
                onDecided(move);
                yield break;
            }
        }
        
        onDecided(Move.Pass()); // Put down cards in Move
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    [Header("Difficulty")]
    public bool isSmart = false;

    [Tooltip("Non-smart AI only: chance to pass on a turn it could have played (never when leading).")]
    [Range(0f, 1f)] public float passChance = 0.15f;
    
    
    public override void SortHand()
    {
        if (!isSmart) return; // dumb AI keeps its hand in dealt order
        
        if (hand.Count != 0)
        {
            hand.Sort((x, y) => x.rank.CompareTo(y.rank));
        }
    }
    
    public override IEnumerator PlayCard(Move currentPlay, bool mustMatch, Action<Move> onDecided)
    {
        yield return new WaitForSeconds(1.2f);
        
        onDecided(ChooseMove(currentPlay, mustMatch));
    }

    private Move ChooseMove(Move currentPlay, bool mustMatch)
    {
        bool isLeading = currentPlay == null || currentPlay.IsPass;

        // a dumb AI sometimes just passes instead of playing at a random value under the passchance
        if (!isSmart && !isLeading && !mustMatch && UnityEngine.Random.value < passChance)
            return Move.Pass();
        
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
        
        // If player has 2 of hearts and nothing else that they can play (over a pair or triple, persay), then play it!
        if (!isLeading && !mustMatch)
        {
            CardData twoOfHearts = hand.FirstOrDefault(c => c.rank == Ranks.Two && c.suit == Suits.Hearts);
            if (twoOfHearts != null)
            {
                var move = new Move(new List<CardData> { twoOfHearts });
                if (Rules.IsLegal(move, currentPlay))
                    return move;
            }
        }
        
        return Move.Pass();
    }
}

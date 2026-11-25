using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanPlayer : Player
{
    public override void SortHand()
    {
        if (hand.Count != 0)
        {
            hand.Sort((x, y) => x.rank.CompareTo(y.rank));
        }
    }

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

            input.InvalidPlay(WhyIllegal(move, currentPlay, mustMatch));
            input.Clear(); // clear the invalid click and wait for the next one
        }
    }
    
    
    
    
    /// <summary>
    /// Helper to for the players feedback to be able to say why a card was invalid
    /// </summary>
    /// <param name="move"></param>
    /// <param name="pile"></param>
    /// <param name="mustMatch"></param>
    /// <returns></returns>
    public string WhyIllegal(Move move, Move pile, bool mustMatch)
    {
        if (move.Count > GameRules.MAX_CARDS_IN_MOVE) return "You can play at most " + GameRules.MAX_CARDS_IN_MOVE + " cards";
        foreach (var c in move.Cards)
            if (c.rank != move.Rank) return "Cards in a play must all be the same rank";
        if (pile == null || pile.IsPass) return "That isn't a valid play";
        if (mustMatch) return "You're being skipped, match the pile exactly or pass";
        if (move.Count != pile.Count) return "You need to play exactly " + pile.Count + " card" + (pile.Count > 1 ? "s" : "");
        return "That doesn't beat the pile";
    }
}

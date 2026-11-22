

using System.Collections.Generic;

public class GameRules
{
    
    /// <summary>
    /// Returns whether or not the card(s) supplied are a legal move.
    /// </summary>
    /// <param name="move"></param>
    /// <param name="currentlyDown"></param>
    /// <returns></returns>
    
    public bool IsLegal(Move move, Move currentlyDown)
    {
        if (move == null || move.IsPass) return true;
        
        foreach (var c in move.Cards)
            if (c.rank != move.Rank) return false;  // make sure they're all same rank, if not, not legal
        
        if (currentlyDown == null || currentlyDown.IsPass) return true; // checking if this move is a leading one, if yes, they can be played
        
        // check if this will skip the next player
        if (IsSkip(move, currentlyDown)) return true;
        
        // finally return true if the count matches and if the rank is higher 
        return move.Cards.Count == currentlyDown.Count && move.Rank >= currentlyDown.Rank;
    }

    /// <summary>
    /// Returns whether or not the move played will skip the next player
    /// </summary>
    /// <param name="move"></param>
    /// <param name="currentlyDown"></param>
    /// <returns></returns>
    public bool IsSkip(Move move, Move currentlyDown)
    {
        if (move == null || move.IsPass) return false;
        if (currentlyDown == null || currentlyDown.IsPass) return false;
        if (move.Count != currentlyDown.Count) return false;

        foreach (var c in move.Cards)
            if (c.rank != move.Rank) return false;

        return move.Rank == currentlyDown.Rank;
    }

    /// <summary>
    /// Returns whether a player will be skipped based on if they can play the same card or not
    /// </summary>
    /// <param name="move"></param>
    /// <param name="currentlyDown"></param>
    /// <returns></returns>
    public bool IsMatch(List<CardData> hand, Move currentlyDown)
    {
        if (currentlyDown == null || currentlyDown.IsPass) return false;
        int n = 0;
        foreach (var c in hand)
            if (c.rank == currentlyDown.Rank) n++;
        return n >= currentlyDown.Count;
    }
}

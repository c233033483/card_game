

public class GameRules
{
    
    public bool IsLegal(Move move, Move currentlyDown)
    {
        if (move == null || move.IsPass) return true;
        
        foreach (var c in move.Cards)
            if (c.rank != move.Rank) return false;  // make sure they're all same rank, if not, not legal
        
        if (currentlyDown == null || currentlyDown.IsPass) return true; // checking if this move is a leading one, if yes, they can be played
        
        // finally return true if the count matches and if the rank is higher 
        return move.Cards.Count == currentlyDown.Count && move.Rank > currentlyDown.Rank;
    }
}

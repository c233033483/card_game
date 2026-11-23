using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Playing,
    SetOver,
    RoundOver,
}

public enum Titles
{
    President = 0,
    VicePresident = 1,
    ViceAsshole = 2,
    Asshole = 3
}

/// <summary>
/// This script is a plain C# class that owns the rules of turn flow. GameManager keeps everything Unity specific,
/// but this script is called by the GM and calls events for other scripts to subscribe to (like GameplayUI).
/// </summary>
public class RoundController
{
    // turn order, set/ round flow, skips, finish order, title assignments, table state
    //Read-only states
    public GameState State { get; private set; }
    public Move CurrentPlay { get; private set; }
    public IReadOnlyList<Player> FinishOrder => _finishOrder;
    public Player CurrentPlayer => _players[_currentPlayerIndex];
    public Player LastSetWinner {get; private set;}
    public GameRules Rules { get; } = new GameRules();

    private Player _lastPlayer;
    
    private bool _nextPlayerSkipped;
    public bool MustMatch => _nextPlayerSkipped;

    //Events
    public event Action<Player> OnSetOver;
    public event Action<Player> OnSetClosedOut;
    public event Action<Player> OnRoundOver;
    public event Action<Player> OnPlayerPass;
    public event Action<Player> OnTurnChange;
    public event Action<Player, Move> OnCardsPlayed;
    public event Action<Player, int> OnPlayerOut; // player, place (1 = first out)
    public event Action<List<Player>> OnTitlesAssigned;
    public event Action<Player> OnPlayerSkipped;
    
    //Private data
    private readonly List<Player> _players;
    private readonly HashSet<Player> _passed =  new HashSet<Player>();
    private List<Player> _finishOrder = new List<Player>();
    
    private int _currentPlayerIndex;

    private bool _turnInProgress;
    private bool _autoTurns;

    private int _sameRankRun;
    private Ranks _runRank;
    private const int CLOSEOUT_COUNT = 4;
    
    public RoundController(List<Player> players)
    {
        _players = players;
    }

    public bool HasPassed(Player p) => _passed.Contains(p);
    
    // Called by GM after hands are dealt and sorted
    public void RoundStart()
    {
        Pile.instance.ClearPile();
        
        _finishOrder.Clear();
        _lastPlayer = null;
        _nextPlayerSkipped = false;
        
        _currentPlayerIndex = WhoGoesFirst();
        
        SetStart();
    }

    public void SetStart()
    {
        _passed.Clear();
        CurrentPlay = null;
        _sameRankRun = 0;
        _nextPlayerSkipped = false;
        
        State = GameState.Playing;
        OnTurnChange?.Invoke(CurrentPlayer);
    }

    public bool PlayMove(Move move)
    {      
        // Check if false
        if (State != GameState.Playing) return false;
        if (move == null || move.IsPass || !Rules.IsLegal(move, CurrentPlay)) return false;

        var p = CurrentPlayer;
        foreach (var c in move.Cards)
            if (!p.hand.Contains(c)) return false;

        bool isSkip = Rules.IsSkip(move, CurrentPlay);
        if (_nextPlayerSkipped && !isSkip) return false; // when being skipped you can only match
        
        foreach (var c in move.Cards)
            p.hand.Remove(c);
        
        // track run of same-rank cards for the four-of-a-kind closeout
        bool continuesRun = CurrentPlay != null && !Rules.IsTwoOfHearts(move) && move.Rank == _runRank && _sameRankRun > 0;
        _sameRankRun = continuesRun ? _sameRankRun + move.Count : move.Count;
        _runRank = move.Rank;
        
        CurrentPlay = move;
        _nextPlayerSkipped = isSkip;
        _lastPlayer = p;
        OnCardsPlayed?.Invoke(p, move);
        
        if (p.hand.Count == 0) 
        {
            _finishOrder.Add(p);
            OnPlayerOut?.Invoke(p, _finishOrder.Count);
        }
        
        AfterTurn();
        return true;
    }

    public bool Pass()
    {
        if (State != GameState.Playing || CurrentPlay == null) return false;
        
        if (_nextPlayerSkipped)
        {
            // declining a skip just loses the turn, they are not out for the set
            _nextPlayerSkipped = false;
            OnPlayerSkipped?.Invoke(CurrentPlayer);
            AfterTurn();
            return true;
        }
        
        _passed.Add(CurrentPlayer);
        OnPlayerPass?.Invoke(CurrentPlayer);
        AfterTurn();
        return true;
    }

    private void AfterTurn()
    {
        if (_finishOrder.Count == 3)
        {
            EndRound();
            return;
        }
        
        if (_sameRankRun >= CLOSEOUT_COUNT)
        {
            OnSetClosedOut?.Invoke(_lastPlayer);
            EndSet();
            return;
        }
        
        if (IsSetOver())
        {
            EndSet();
            return;
        }
        
        AdvanceTurn();
    }

    private void AdvanceTurn()
    {
        FindNextPlayer();
        
        // if a skipped player can't match, they lose the turn automatically
        while (_nextPlayerSkipped && !Rules.IsMatch(CurrentPlayer.hand, CurrentPlay))
        {
            _nextPlayerSkipped = false;
            OnPlayerSkipped?.Invoke(CurrentPlayer);
            FindNextPlayer();
        }
        
        // checks if the skips carried the turn all the way back to the last player, nobody could beat them so turn over
        if (CurrentPlayer == _lastPlayer)
        {
            EndSet();
            return;
        }   
        
        OnTurnChange?.Invoke(CurrentPlayer);
    }

    private void FindNextPlayer()
    {
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        while (CurrentPlayer.hand.Count == 0 || _passed.Contains(CurrentPlayer)); 
    }

    private void EndSet()
    {
        Player winner = _lastPlayer;
        CurrentPlay = null;
        LastSetWinner = winner;
        
        // Sets currentPlayerIndex to the index of the winner
        _currentPlayerIndex = _players.IndexOf(winner);

        // If the winner played their last card, the lead goes to the next player still in
        while (_players[_currentPlayerIndex].hand.Count == 0)
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        
        State = GameState.SetOver;
        OnSetOver?.Invoke(winner);
    }

    private void EndRound()
    {
        foreach (Player p in _players)
            if (p.hand.Count > 0) _finishOrder.Add(p); // adds last place
        
        State = GameState.RoundOver;
        OnRoundOver?.Invoke(_finishOrder[0]);
        
        AssignTitles();
    }

    /// <summary>
    ///  1st: President
    ///  2nd: Vice President
    ///  3rd: Vice Asshole
    ///  4th: Asshole
    /// </summary>
    private void AssignTitles()
    {
        for (int i = 0; i < _finishOrder.Count; i++)
        {
            _finishOrder[i].title = (Titles)i;
            Debug.Log(_finishOrder[i].playerName + " is the " + (Titles)i);
        }
        
        OnTitlesAssigned?.Invoke(_finishOrder);
    }


    /// <summary>
    /// Helpers for debugging
    /// </summary>
    /// <returns></returns>

    private int WhoGoesFirst()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            foreach (var card in _players[i].hand)
            {
                if (card.rank == Ranks.Two && card.suit == Suits.Hearts)
                    return i;
            }
        }

        return 1;
    }
    
    // True when nobody except the last player to play can still act
    private bool IsSetOver()
    {
        foreach (var p in _players)
        {
            if (p == _lastPlayer) continue;
            if (p.hand.Count > 0 && !_passed.Contains(p)) return false;
        }
        return true;
    }
}

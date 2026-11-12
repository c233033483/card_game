using System;
using System.Collections.Generic;

public enum GameState
{
    Playing,
    SetOver,
    RoundOver,
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
    public CardData CurrentCard { get; private set; }
    public IReadOnlyList<Player> FinishOrder => _finishOrder;
    public Player CurrentPlayer => _players[_currentPlayerIndex];
    public Player LastSetWinner {get; private set;}
    
    private Player _lastPlayer;

    //Events
    public event Action<Player> OnSetOver;
    public event Action<Player> OnRoundOver;
    public event Action<Player> OnPlayerPass;
    public event Action<Player> OnTurnChange;
    public event Action<Player, CardData> OnCardsPlayed;
    public event Action<Player, int> OnPlayerOut; // player, place (1 = first out)

    //Private data
    private readonly List<Player> _players;
    private readonly HashSet<Player> _passed =  new HashSet<Player>();
    private List<Player> _finishOrder = new List<Player>();
    
    private const int PLAYERS = 4;
    private int _currentPlayerIndex;

    private bool _turnInProgress;
    private bool _autoTurns;

    public RoundController(List<Player> players)
    {
        _players = players;
    }

    public bool HasPassed(Player p) => _passed.Contains(p);
    
    // Called by GM after hands are dealt and sorted
    public void RoundStart()
    {
        _finishOrder.Clear();
        _lastPlayer = null;
        
        SetStart();
    }

    public void SetStart()
    {
        _passed.Clear();
        CurrentCard = null;
        
        State = GameState.Playing;
        OnTurnChange?.Invoke(CurrentPlayer);
    }

    public bool PlayCard(CardData cardPlayed)
    {      
        if (State != GameState.Playing) return false;
        var p = CurrentPlayer;
        if (!p.hand.Contains(cardPlayed) || !IsLegal(cardPlayed)) return false;

        p.hand.Remove(cardPlayed);
        CurrentCard = cardPlayed;
        _lastPlayer = p;
        OnCardsPlayed?.Invoke(p, cardPlayed);
        
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
        if (State != GameState.Playing || CurrentCard == null) return false;
        
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
        if (IsSetOver())
        {
            EndSet();
            return;
        }
        
        AdvanceTurn();
    }

    private void AdvanceTurn()
    {
        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        while (CurrentPlayer.hand.Count == 0 || _passed.Contains(CurrentPlayer)); 

        OnTurnChange?.Invoke(CurrentPlayer);
    }

    private void EndSet()
    {
        Player winner = _lastPlayer;
        CurrentCard = null;
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
        
        // assign titles
    }

    /// <summary>
    /// Helpers for debugging
    /// </summary>
    /// <returns></returns>

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
    
    private bool IsLegal(CardData card)
    {
        return CurrentCard == null || card.rank > CurrentCard.rank;
    }
}

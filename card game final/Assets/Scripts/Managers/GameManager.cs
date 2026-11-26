using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 25/09/26 For Managing turns in rounds, ends of sets, games. Setting places.
/// </summary>
public class GameManager : MonoBehaviour
{
    public HandManager handManager;
    private Deck _deck;
    private RoundController _round;
    [SerializeField] private GameplayUI gameplayUI;

    private readonly List<Player> _players = new List<Player>();
    [SerializeField] private bool[] smartAI = new bool[PLAYERS];
    private List<Player> _playersActive;

    [SerializeField] private float dealTime;
    [SerializeField] private bool turnInProgress;
    [SerializeField] private float cardPlayDuration = 0.5f;
    [SerializeField] private int roundIndex;

    private const int PLAYERS = 4;
    private Coroutine _turnRoutine;
    
    private bool _autoTurns;
    private bool _gettingSkipped;

    [SerializeField] private TMP_Text[] debugTexts = new TMP_Text[4];
    [SerializeField] private bool showDebugText;
    
    private void Start()
    {
        roundIndex = 0;
        _autoTurns = true;
        
        CreatePlayers();
        
        _round = new RoundController(_players);
        _round.OnCardsPlayed += OnCardPlayed;
        _round.OnSetOver += OnSetEnded;
        _round.OnRoundOver += OnRoundEnded;
        _round.OnTurnChange += _ => UpdateDebugHandsText();
        _round.OnPlayerPass += _ => UpdateDebugHandsText();
        _round.OnPlayerSkipped += _ => UpdateDebugHandsText();
        
        gameplayUI.Init(_round, cardPlayDuration);
    }

    private void Update()
    {
        if (!_autoTurns && Input.GetKeyDown(KeyCode.Space))
            StartNextTurn();
        else if (Input.GetKeyDown(KeyCode.T))
        {
            showDebugText = !showDebugText;
            UpdateDebugHandsText();
        }
    }

    private void CreatePlayers()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            Player p;
            
            if (i == 0) p = new HumanPlayer();
            else
            {
                bool smart = smartAI != null && i < smartAI.Length && smartAI[i];
                p = new AIPlayer { isSmart = smart };
            }
            
            p.playerName = "Player " + (i + 1);
            _players.Add(p);
        }
    }

    private void Deal()
    {
        foreach (Player p in _players)
        {
            p.hand.Clear();
        }
        
        StartCoroutine(DealAll());
    }

    IEnumerator DealAll()
    {
        for (int i = 0; i < _deck.gameDeck.Count; i++)
        {
            if (roundIndex == 0)
            {
                if (_deck.gameDeck[i].rank == Ranks.Two && _deck.gameDeck[i].suit == Suits.Hearts)
                {
                    _players[0].hand.Add(_deck.gameDeck[i]);
                    continue;
                }
            }
            
            _players[i % PLAYERS].hand.Add(_deck.gameDeck[i]);

            if (i % PLAYERS == 0)
            {
                handManager.AddCard(_deck.gameDeck[i]);
            }
            
            yield return new WaitForSeconds(dealTime);
        }
        
        foreach (Player p in _players)
        {
            p.SortHand();
        }
        
        _round.RoundStart();
        UpdateDebugHandsText();
        TryAutoNextTurn();
    }

    public void StartRound()
    {
        handManager.ClearHand();
        
        _deck = new Deck();
        _deck.BuildDeck();
        _deck.RandomiseDeck();
        Deal();
    }

    
    public void StartSet()
    {
        Pile.instance.ClearPile();
        _round.SetStart();
        TryAutoNextTurn();
    }


    private void StartNextTurn()
    {
        if (_round.State != GameState.Playing || turnInProgress) return;

        Player p = _round.CurrentPlayer;
        turnInProgress = true;
        
        HumanTurnInput.Instance.SetSelectionEnabled(p is HumanPlayer);

        _turnRoutine = StartCoroutine(p.PlayCard(_round.CurrentPlay, _round.MustMatch, OnCardDecided));
    }

    private void OnCardDecided(Move move)
    {
        HumanTurnInput.Instance.SetSelectionEnabled(false);
        
        _turnRoutine = null;
        turnInProgress = false;

        if (!move.IsPass)
            _round.PlayMove(move);
        else
            _round.Pass();

        TryAutoNextTurn();
    }

    private void OnCardPlayed(Player p, Move move)
    {
        Pile.instance.AddCard(move, cardPlayDuration);
        if (p is HumanPlayer) handManager.RemoveCard(move.Cards, cardPlayDuration);
    }

    public void PlayerPasses()
    {
        if (!_round.Pass()) return; // ignored if the human is leading
        
        HumanTurnInput.Instance.SetSelectionEnabled(false);
        
        if (_turnRoutine != null)
        {
            StopCoroutine(_turnRoutine);
            _turnRoutine = null;
        }
        
        turnInProgress = false;
        TryAutoNextTurn();
    }

    private void OnSetEnded(Player winner)
    {
        Debug.Log("Set Over, " +  winner.playerName + " wins!");
        
        Pile.instance.ClearPile();
        UpdateDebugHandsText();
    }

    private void OnRoundEnded(Player winner)
    {
        Debug.Log("Round End, " + winner.playerName + " wins!");

        roundIndex++;
        
        Pile.instance.ClearPile();   
        handManager.ClearHand();
    }

    /// <summary>
    /// Helpers for debugging
    /// </summary>
    /// <returns></returns>

    private void TryAutoNextTurn()
    {
        if (_autoTurns && !turnInProgress)
            StartNextTurn();
    }

    private static string HandToString(List<CardData> cd)
    {
        var sb = new System.Text.StringBuilder();
        foreach (CardData card in cd)
        {
            sb.AppendLine(card.rank + " of " + card.suit);
        }
        return sb.ToString();
    }

    private void UpdateDebugHandsText()
    {
        for (int i = 0; i < _players.Count; i++)
        {
            debugTexts[i].gameObject.SetActive(showDebugText);
            if (!showDebugText) continue;
            
            debugTexts[i].text = HandToString(_players[i].hand);

            if (_round.HasPassed(_players[i]))
                debugTexts[i].color = Color.grey;
            else if (_players[i] == _round.LastSetWinner)
                debugTexts[i].color = Color.yellow;
            else
                debugTexts[i].color = Color.white;
        }
    }

    public void ToggleAutoTurns()
    {
        _autoTurns = !_autoTurns;
        Debug.Log("Auto turns " + _autoTurns);
    }
}

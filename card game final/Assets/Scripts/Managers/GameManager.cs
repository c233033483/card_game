using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState
{
    Playing,
    NamingBit,
    GameOver
}

/// <summary>
/// 25/09/26 For Managing turns in rounds, ends of sets, games. Setting places.
/// </summary>
public class GameManager : MonoBehaviour
{
    public HandManager handManager;

    private Deck _deck;
    private GameState _gameState;
    
    private CardData _currentCardInSet =  new CardData();
    private Player _lastPlayer = new Player();
    
    private readonly List<Player> _players = new List<Player>();
    private List<Player> _playersActive;
    
    private int currentPlayerIndex;
    private int playersOut;
    private int setIndex;

    public TMP_Text placedText;
    public TMP_Text skipsText;
    
    public TMP_Text texthand1;
    public TMP_Text texthand2;
    public TMP_Text texthand3;
    public TMP_Text texthand4;

    private void Start()
    {
        _deck = new  Deck();

        CreatePlayers();
        
        _deck.BuildDeck();
        _deck.RandomiseDeck();
        Deal();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartNextTurn();
        }
    }
    

    private void CreatePlayers()
    {
        for (int i = 0; i < 4; i++)
        {
            Player p = new Player();
            string playerName = "Player " + (i + 1);
            p.playerName = playerName;
            
            _players.Add(p);
            
            Debug.Log(HandToString(p.hand));
        }
    }
    
    private void Deal()
    {
        //in future, have a list of decks from the player class instead.

        for (int i = 0; i < _deck.gameDeck.Count; i++)
        {
            _players[i % 4].hand.Add(_deck.gameDeck[i]);

            if (i % 4 == 0)
            {
                handManager.AddCard(_deck.gameDeck[i]); // Debug for now
            }
        }

        foreach (Player p in _players)
        {
            p.SortHand();
        }
        
        _playersActive = new List<Player>(_players);
        UpdateDebugHandsText();
    }

    private void StartNextTurn()
    {
        if (!_playersActive.Contains(_players[currentPlayerIndex]))
        {
            Debug.Log(_players[currentPlayerIndex].playerName + " is out of this round");
            return; 
        }
        
        Debug.Log("Turn Starting for " + _players[currentPlayerIndex].playerName);
        
        if (_currentCardInSet == null)
            _currentCardInSet = new CardData();
        CardData cardPlayed = _players[currentPlayerIndex].PlayCard(_currentCardInSet);

        if (cardPlayed != null)
        {
            _currentCardInSet = cardPlayed;
            placedText.text = cardPlayed.rank + " of " + cardPlayed.suit;
            _lastPlayer =  _players[currentPlayerIndex];
            
/*
            if (currentPlayerIndex == 0)
            {
                handManager.RemoveCard(cardPlayed);
            }*/
            if (_players[currentPlayerIndex].hand.Count == 0)
            {
                EndRound(_players[currentPlayerIndex]);
                return;
            }
        }
        else // Player has no cards that can be played
        {
            //playersOut++;

            _playersActive.Remove(_playersActive[currentPlayerIndex]);
            Debug.Log("CardPlayed is null, " + _players[currentPlayerIndex].playerName + " skipped");
            placedText.text = _players[currentPlayerIndex].playerName + " skipped";

            /*
            if (playersOut >= 3)
            {
                EndSet();
                return;
            }*/

            if (_playersActive.Count <= 1)
            {
                EndSet();
                return;
            }
            
        }
        currentPlayerIndex =  (currentPlayerIndex + 1) % 4;
        
        skipsText.text = "Skips in a row: " + playersOut;
        
        UpdateDebugHandsText();
    }

    private void EndSet()
    {
        Debug.Log("Set Over, " +  _lastPlayer.playerName + " wins!");
        
        playersOut = 0;
        setIndex++;

        _playersActive = new List<Player>(_players);

        _currentCardInSet = null;
        placedText.text = "";
        
        _lastPlayer = _players[currentPlayerIndex];
    }

    private void EndRound(Player winner)
    {
        Debug.Log("Round End, " + winner.playerName + " wins!");
    }
    
    
    /// <summary>
    /// Helper for debugging
    /// </summary>
    /// <returns></returns>
    
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
        texthand1.text = HandToString(_players[0].hand);
        texthand2.text = HandToString(_players[1].hand);
        texthand3.text = HandToString(_players[2].hand);
        texthand4.text = HandToString(_players[3].hand);
    }
}


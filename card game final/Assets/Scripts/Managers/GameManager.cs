using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameState
{
    Playing,
    NamingBit,
    GameOver
}


public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 25/09/26 For Managing turns in rounds, ends of sets, games. Setting places.
    /// </summary>
    /// <returns></returns>

    private Deck _deck;
    private GameState _gameState;
    private CardData _currentCardInSet =  new CardData();
    
    private readonly List<Player> _players = new List<Player>();
    private int currentPlayerIndex;
    
    
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
        
        StartNextTurn();
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
        //in future, have a list of decks from the player class.

        for (int i = 0; i < _deck.gameDeck.Count; i++)
        {
            _players[i % 4].hand.Add(_deck.gameDeck[i]); //players[i%players.count].Add(gamedeck[i]);
        }

        foreach (Player p in _players)
        {
            p.SortHand();
        }
        
        texthand1.text = HandToString(_players[0].hand);
        texthand2.text = HandToString(_players[1].hand);
        texthand3.text = HandToString(_players[2].hand);
        texthand4.text = HandToString(_players[3].hand);
    }

    public void StartNextTurn()
    {
        Debug.Log("Turn Starting");
        _players[currentPlayerIndex % 4].PlayCard(_currentCardInSet);
        currentPlayerIndex++;
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
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private Player _lastPlayer;
    private Player _setWinner;
    
    private readonly List<Player> _players = new List<Player>();
    private List<Player> _playersActive;

    [SerializeField] private float dealTime;
    private int currentPlayerIndex;
    private int setIndex;

    public TMP_Text placedText;
    public TMP_Text skipsText;
    
    public TMP_Text texthand1;
    public TMP_Text texthand2;
    public TMP_Text texthand3;
    public TMP_Text texthand4;
    public Button nextSetButton;
    public Button nextRoundButton;

    private void Start()
    {
        _deck = new  Deck();
        
        nextSetButton.gameObject.SetActive(false);
        nextRoundButton.gameObject.SetActive(true);
        
        CreatePlayers();
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
            Player p;
            if (i == 0) p = new HumanPlayer();
            else p = new AIPlayer();
            string playerName = "Player " + (i + 1);
            p.playerName = playerName;
            
            _players.Add(p);
            
            Debug.Log(HandToString(p.hand));
        }
    }
    
    private void Deal()
    {
        for (int i = 0; i < _deck.gameDeck.Count; i++)
        {
            StartCoroutine(DealCard(i));
        }

        foreach (Player p in _players)
        {
            p.SortHand();
        }
        
        _playersActive = new List<Player>(_players);
        UpdateDebugHandsText();
    }

    IEnumerator DealCard(int i)
    {
        YieldInstruction wait = new WaitForSeconds(dealTime);
        _players[i % 4].hand.Add(_deck.gameDeck[i]);

        if (i % 4 == 0)
        {
            handManager.AddCard(_deck.gameDeck[i]); // Debug for now
        }

        yield break;
    }

    public void StartSet()
    {
        _gameState =  GameState.Playing;
        UpdateDebugHandsText();
    }

    public void StartRound()
    {
        nextRoundButton.gameObject.SetActive(false);
        nextSetButton.gameObject.SetActive(false);
        
        handManager.ClearHand();
        
        _currentCardInSet = null;
        _setWinner = null;
        setIndex = 0;
        placedText.text = "";
        skipsText.text = "Skips in a row: 0";
        
        _deck = new Deck();
        _deck.BuildDeck();
        _deck.RandomiseDeck();
        Deal();
        
        StartSet();
    }

    private void StartNextTurn()
    {
        if (_gameState != GameState.Playing)
        {
            Debug.Log("Gamestate is " +  _gameState);
            return;
        }
        
        if (!_playersActive.Contains(_players[currentPlayerIndex]))
        {
            Debug.Log(_players[currentPlayerIndex].playerName + " is out of this round");
            currentPlayerIndex = (currentPlayerIndex + 1) % 4;
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
            
            if (currentPlayerIndex == 0)
            {
                handManager.RemoveCard(_currentCardInSet); // Debug for now
            }
            
            if (_players[currentPlayerIndex].hand.Count == 0)
            {
                EndRound(_players[currentPlayerIndex]);
                return;
            }
        }
        else // Player has no cards that can be played
        {
            _playersActive.Remove(_players[currentPlayerIndex]);
            Debug.Log("CardPlayed is null, " + _players[currentPlayerIndex].playerName + " skipped");

            if (_playersActive.Count <= 1)
            {
                UpdateDebugHandsText();
                EndSet();
                
                return;
            }
            
        }
        currentPlayerIndex =  (currentPlayerIndex + 1) % 4;
        
        skipsText.text = "Skips in a row: " + (_players.Count - _playersActive.Count);
        
        UpdateDebugHandsText();
    }

    private void EndSet()
    {
        Debug.Log("Set Over, " +  _lastPlayer.playerName + " wins!");
        
        _setWinner = _lastPlayer;
        setIndex++;
        _playersActive = new List<Player>(_players);
        _currentCardInSet = null;
        placedText.text = "";
        
        currentPlayerIndex = _players.IndexOf(_lastPlayer); // Sets currentPlayerIndex to the index of _lastPlayer
        _lastPlayer = _players[currentPlayerIndex];
        
        nextSetButton.gameObject.SetActive(true);
        _gameState = GameState.NamingBit;
    }

    private void EndRound(Player winner)
    {
        Debug.Log("Round End, " + winner.playerName + " wins!");
        _gameState = GameState.GameOver;
        
        nextSetButton.gameObject.SetActive(false);
        nextRoundButton.gameObject.SetActive(true);
    }
    
    
    /// <summary>
    /// Helpers for debugging
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
        TMP_Text[] debugTexts = { texthand1, texthand2, texthand3, texthand4 };
        
        for (int i = 0; i < _players.Count; i++)
        {
            debugTexts[i].text = HandToString(_players[i].hand);

            if (!_playersActive.Contains(_players[i]))
                debugTexts[i].color = Color.grey;      // skipped this set
            else if (_players[i] == _setWinner)
                debugTexts[i].color = Color.yellow;    // won the previous set
            else
                debugTexts[i].color = Color.white;
        }
    }
}


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
        
        private CardData _currentCardInSet;
        private Player _lastPlayer;
        private Player _setWinner;
        
        private readonly List<Player> _players = new List<Player>();
        private List<Player> _playersActive;

        [SerializeField] private float dealTime;
        
        private const int PLAYERS = 4; 
        private int _playersOutIndex; // players who finished their hand in a round
        private int _currentPlayerIndex;
        private Player CurrentPlayer => _players[_currentPlayerIndex];

        public TMP_Text placedText;
        
        public TMP_Text texthand1;
        public TMP_Text texthand2;
        public TMP_Text texthand3;
        public TMP_Text texthand4;
        public Button nextSetButton;
        public Button nextRoundButton;

        private void Start()
        {
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
            for (int i = 0; i < PLAYERS; i++)
            {
                Player p;
                if (i == 0) p = new HumanPlayer();
                else p = new AIPlayer();
                string playerName = "Player " + (i + 1);
                p.playerName = playerName;
                
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
                _players[i % PLAYERS].hand.Add(_deck.gameDeck[i]);

                if (i % PLAYERS == 0)
                {
                    handManager.AddCard(_deck.gameDeck[i]); // Debug for now
                }
                
                yield return new WaitForSeconds(dealTime);
            }
            
            foreach (Player p in _players)
            {
                p.SortHand();
            }
            
            _playersActive = new List<Player>(_players);
            UpdateDebugHandsText();
            StartSet();
        }
        
        public void StartRound()
        {
            nextRoundButton.gameObject.SetActive(false);
            nextSetButton.gameObject.SetActive(false);
            
            handManager.ClearHand();
            
            _currentCardInSet = null;
            _setWinner = null;
            placedText.text = "";
            
            _deck = new Deck();
            _deck.BuildDeck();
            _deck.RandomiseDeck();
            Deal();
        }
        
        public void StartSet()
        {
            _gameState =  GameState.Playing;
            UpdateDebugHandsText();
        }
        
        private void StartNextTurn()
        {
            // 1. Check state
            if (_gameState != GameState.Playing)
            {
                Debug.Log("Game state is " +  _gameState);
                return;
            }
            
            // 2. Check if player active
            if (IsPlayerActive(CurrentPlayer))
            {
                Debug.Log("Turn Starting for " + CurrentPlayer.playerName);
            
                // 3. Check if current card is null and set card played
                if (_currentCardInSet == null)
                    _currentCardInSet = new CardData();
                CardData cardPlayed = CurrentPlayer.PlayCard(); // receive the card
            
                if (cardPlayed != null)
                    HandleTurn(cardPlayed);
                else
                    HandleSkip();
            }
            else
            {
                Debug.Log(CurrentPlayer.playerName + " is out of this round");
            }
        }

        private bool IsPlayerActive(Player player)
        {
            return _playersActive.Contains(player);
        }

        private void HandleTurn(CardData cardPlayed)
        {
            _currentCardInSet = cardPlayed;
            placedText.text = cardPlayed.rank + " of " + cardPlayed.suit;
            _lastPlayer =  CurrentPlayer;
                
            // Update pile
            Pile.instance.AddCard(cardPlayed);
                
            if (_currentPlayerIndex == 0) // If it's the human players turn, update the hand
            {
                handManager.RemoveCard(cardPlayed); // Debug for now
            }
                
            if (CurrentPlayer.hand.Count == 0) // If this players hand is empty
            {
                _playersOutIndex++;
                if (_playersOutIndex >= 3)
                    EndRound(CurrentPlayer);
            }
            
            AdvanceTurn();
        }

        private void HandleSkip()
        {
            _playersActive.Remove(CurrentPlayer);
            Debug.Log("CardPlayed is null, " + CurrentPlayer.playerName + " skipped");

            AdvanceTurn();
            
            if (_playersActive.Count <= 1)
            {
                UpdateDebugHandsText();
                EndSet();
            }
        }

        private void AdvanceTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % PLAYERS;
            UpdateDebugHandsText();
        }

        private void EndSet()
        {
            Debug.Log("Set Over, " +  _lastPlayer.playerName + " wins!");
            
            _setWinner = _lastPlayer;
            _playersActive = new List<Player>(_players);
            _currentCardInSet = null;
            placedText.text = "";

            Pile.instance.ClearPile();
            
            _currentPlayerIndex = _players.IndexOf(_lastPlayer); // Sets currentPlayerIndex to the index of _lastPlayer
            _lastPlayer = CurrentPlayer;
            
            nextSetButton.gameObject.SetActive(true);
            _gameState = GameState.NamingBit;
        }

        private void EndRound(Player winner)
        {
            Debug.Log("Round End, " + winner.playerName + " wins!");
            _gameState = GameState.GameOver;
            _playersOutIndex = 0;
            
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
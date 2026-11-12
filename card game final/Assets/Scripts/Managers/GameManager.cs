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
        
        private Player _lastPlayer;
        private readonly List<Player> _players = new List<Player>();
        private List<Player> _playersActive;

        [SerializeField] private float dealTime;
        [SerializeField] private bool _turnInProgress;
        
        private const int PLAYERS = 4; 
        
        [Header("Debug / UI")]
        private bool _autoTurns;
        
        [SerializeField] private TMP_Text[] debugTexts = new TMP_Text[4];
        [SerializeField] private bool showDebugText;
        
        public Button nextSetButton;
        public Button nextRoundButton;
        public Button skipButton;

        private void Start()
        {
            nextSetButton.gameObject.SetActive(false);
            nextRoundButton.gameObject.SetActive(true);
            skipButton.gameObject.SetActive(false);
            _autoTurns = true;
            
            CreatePlayers();
            
            _round = new RoundController(_players);
            _round.OnCardsPlayed += OnCardPlayed;
            _round.OnSetOver += OnSetEnded;
            _round.OnRoundOver += OnRoundEnded;
            _round.OnTurnChange += _ => UpdateDebugHandsText();
            _round.OnPlayerPass += _ => UpdateDebugHandsText();
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
            nextRoundButton.gameObject.SetActive(false);
            nextSetButton.gameObject.SetActive(false);
            
            handManager.ClearHand();
            
            _deck = new Deck();
            _deck.BuildDeck();
            _deck.RandomiseDeck();
            Deal();
        }
        
        public void StartSet()
        {
            nextSetButton.gameObject.SetActive(false);
            _round.SetStart();
            TryAutoNextTurn();
        }
        
        private void StartNextTurn()
        {
            if (_round.State != GameState.Playing || _turnInProgress) return;

            Player p = _round.CurrentPlayer;
            skipButton.gameObject.SetActive(p is HumanPlayer);
            _turnInProgress = true;

            StartCoroutine(p.PlayCard(OnCardDecided));
        }
        
        private void OnCardDecided(CardData cardPlayed)
        {
            _turnInProgress = false;

            if (cardPlayed != null)
                _round.PlayCard(cardPlayed);
            else
                _round.Pass();

            TryAutoNextTurn();
        }

        private void OnCardPlayed(Player p, CardData card)
        {
            Pile.instance.AddCard(card);
            if (p is HumanPlayer) handManager.RemoveCard(card);
        }
        
        public void PlayerSkips()
        {
            if (!_round.Pass()) return; // ignored if the human is leading
            
            _turnInProgress = false;
            TryAutoNextTurn();
            skipButton.gameObject.SetActive(false);
        }

        private void OnSetEnded(Player winner)
        {
            Debug.Log("Set Over, " +  winner.playerName + " wins!");
            
            Pile.instance.ClearPile();
            nextSetButton.gameObject.SetActive(true);
            UpdateDebugHandsText();
        }

        private void OnRoundEnded(Player winner)
        {
            Debug.Log("Round End, " + winner.playerName + " wins!");
            
            Pile.instance.ClearPile();
            nextSetButton.gameObject.SetActive(false);
            nextRoundButton.gameObject.SetActive(true);        
        }

        /// <summary>
        /// Helpers for debugging
        /// </summary>
        /// <returns></returns>

        private void TryAutoNextTurn()
        {
            if (_autoTurns && !_turnInProgress)
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
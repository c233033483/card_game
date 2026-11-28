using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button nextSetButton;
    [SerializeField] private Button nextRoundButton;

    [Header("Text")]
    [SerializeField] private TMP_Text turnText;      // whose turn it is
    [SerializeField] private TMP_Text feedbackText;  // what just happened

    public float cardPlayingDelay;
    private RoundController _round;
    private bool _skipHandleCardPlayed;

    // remembered so skip / out / closeout messages can say who played what
    private Player _lastMover;
    private Move _lastMove;
    private bool _closedOut;

    public void Init(RoundController round, float delay)
    {
        _round = round;
        cardPlayingDelay = delay;
        _skipHandleCardPlayed = false;

        _round.OnTurnChange += HandleTurnChange;
        _round.OnCardsPlayed += HandleCardsPlayed;
        _round.OnPlayerPass += HandlePass;
        _round.OnPlayerSkipped += HandleSkipped;
        _round.OnPlayerOut += HandlePlayerOut;
        _round.OnSetClosedOut += HandleSetClosedOut;
        _round.OnSetOver += HandleSetOver;
        _round.OnRoundOver += HandleRoundOver;
        _round.OnTitlesAssigned += HandleTitlesAssigned;

        if (HumanTurnInput.Instance != null)
            HumanTurnInput.Instance.OnInvalidPlay += Feedback;
        else
            Debug.LogWarning("GameplayUI: HumanTurnInput.Instance is null, invalid play messages won't show");

        // start state
        SetTurnButtons(false);
        nextSetButton.gameObject.SetActive(false);
        nextRoundButton.gameObject.SetActive(true);
        turnText.text = "";
        Feedback("Press Start Round to deal");

        // buttons hide themselves when clicked
        nextRoundButton.onClick.AddListener(HideNextRoundButtons);
        nextSetButton.onClick.AddListener(HideNextSetButton);
    }

    private void OnDestroy()
    {
        if (_round != null)
        {
            _round.OnTurnChange -= HandleTurnChange;
            _round.OnCardsPlayed -= HandleCardsPlayed;
            _round.OnPlayerPass -= HandlePass;
            _round.OnPlayerSkipped -= HandleSkipped;
            _round.OnPlayerOut -= HandlePlayerOut;
            _round.OnSetClosedOut -= HandleSetClosedOut;
            _round.OnSetOver -= HandleSetOver;
            _round.OnRoundOver -= HandleRoundOver;
            _round.OnTitlesAssigned -= HandleTitlesAssigned;
        }

        if (HumanTurnInput.Instance != null)
            HumanTurnInput.Instance.OnInvalidPlay -= Feedback;

        nextRoundButton.onClick.RemoveListener(HideNextRoundButtons);
        nextSetButton.onClick.RemoveListener(HideNextSetButton);
    }

    // ---------- button helpers ----------

    private void HideNextRoundButtons()
    {
        nextRoundButton.gameObject.SetActive(false);
        nextSetButton.gameObject.SetActive(false);
        Feedback("Dealing...");
    }

    private void HideNextSetButton()
    {
        nextSetButton.gameObject.SetActive(false);
    }

    // waits for the last card to land on the pile before offering the next button
    private IEnumerator ShowButtonAfterDelay(Button b)
    {
        yield return new WaitForSeconds(cardPlayingDelay);
        b.gameObject.SetActive(true);
    }

    private void SetTurnButtons(bool on)
    {
        playButton.gameObject.SetActive(on);
        passButton.gameObject.SetActive(on);
    }

    // ---------- event handlers ----------

    private void HandleTurnChange(Player p)
    {
        bool human = p is HumanPlayer;
        turnText.text = human ? "Your turn" : p.playerName + "'s turn";

        // only show buttons on the human's turn, and no pass when leading
        playButton.gameObject.SetActive(human);
        passButton.gameObject.SetActive(human && _round.CurrentPlay != null);

        if (human && _round.MustMatch)
        {
            _skipHandleCardPlayed = true;
            
            string by = _lastMover != null && _lastMove != null
                ? Who(_lastMover) + " played " + Describe(_lastMove) + " and is skipping you! "
                : "You're being skipped! ";
            Feedback(by + "Match it with " + Describe(_round.CurrentPlay) + " or pass");
        }
        else if (human && _round.CurrentPlay == null)
            Feedback("You're leading. Select 1 to 3 cards of the same rank");
    }

    private void HandleCardsPlayed(Player p, Move move)
    {
        _skipHandleCardPlayed = false;
        
        _lastMover = p;
        _lastMove = move;

        SetTurnButtons(false);
        StartCoroutine(ShowAfterDelay(Who(p) + " played " + Describe(move)));
    }

    private void HandlePass(Player p)
    {
        Feedback(Who(p) + " passed");
    }

    // "Player 2 played a Jack and skipped Player 3"
    private void HandleSkipped(Player skipped)
    {
        _skipHandleCardPlayed = true;
        
        if (_lastMover == null || _lastMove == null)
        {
            Feedback(Who(skipped) + (skipped is HumanPlayer ? " were" : " was") + " skipped");
            return;
        }

        Feedback(Who(_lastMover) + " played " + Describe(_lastMove) + " and skipped " + Whom(skipped));
    }

    private void HandlePlayerOut(Player p, int place)
    {
        _skipHandleCardPlayed = true;
        
        string played = _lastMove != null ? " played " + Describe(_lastMove) + " and" : "";
        Feedback(Who(p) + played + " went out in " + Ordinal(place) + " place!");
    }

    private void HandleSetClosedOut(Player p)
    {
        _closedOut = true;
    }

    private void HandleSetOver(Player winner)
    {
        SetTurnButtons(false);
        turnText.text = "";

        if (_closedOut && _lastMove != null)
        {
            Feedback(Who(winner) + " played four " + Plural(_lastMove.Rank.ToString()) + " and closed out the set!");
            _closedOut = false;
        }
        else
        {
            Feedback(Who(winner) + (winner is HumanPlayer ? " win" : " wins") + " the set");
        }

        StartCoroutine(ShowButtonAfterDelay(nextSetButton));
    }

    private void HandleRoundOver(Player winner)
    {
        SetTurnButtons(false);
        turnText.text = "";
        nextSetButton.gameObject.SetActive(false);

        Feedback("Round over! " + Who(winner) + (winner is HumanPlayer ? " are" : " is") + " the President");
        StartCoroutine(ShowButtonAfterDelay(nextRoundButton));
    }

    // fires right after OnRoundOver, so this replaces its message with the full ranking
    private void HandleTitlesAssigned(List<Player> order)
    {
        var sb = new System.Text.StringBuilder("Round over!");
        for (int i = 0; i < order.Count; i++)
        {
            sb.Append("\n").Append(Ordinal(i + 1)).Append(": ")
              .Append(Who(order[i])).Append(" (").Append(TitleName(i)).Append(")");
        }
        Feedback(sb.ToString());
    }

    // ---------- text helpers ----------

    private void Feedback(string msg) { feedbackText.text = msg; }

    private static string Who(Player p) => p is HumanPlayer ? "You" : p.playerName;
    private static string Whom(Player p) => p is HumanPlayer ? "you" : p.playerName;

    private IEnumerator ShowAfterDelay(string msg)
    {
        yield return new WaitForSeconds(cardPlayingDelay);
        if (!_skipHandleCardPlayed) feedbackText.text = msg;
    }
    
    private string Describe(Move m)
    {
        if (_round.Rules.IsTwoOfHearts(m)) return "the 2 of Hearts";

        string rank = m.Rank.ToString();
        switch (m.Count)
        {
            case 1: return "a " + rank;
            case 2: return "a pair of " + Plural(rank);
            default: return "three " + Plural(rank);
        }
    }

    private static string Plural(string s) => s.EndsWith("x") || s.EndsWith("s") ? s + "es" : s + "s";

    private static string Ordinal(int n)
    {
        switch (n)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return n + "th";
        }
    }

    private static string TitleName(int place)
    {
        switch (place)
        {
            case 0: return "President";
            case 1: return "Vice President";
            case 2: return "Vice Asshole";
            default: return "Asshole";
        }
    }
}
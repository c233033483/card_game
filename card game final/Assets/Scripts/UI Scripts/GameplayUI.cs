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

    private RoundController _round;
    
    public void Init(RoundController round)
    {
        _round = round;

        _round.OnTurnChange += HandleTurnChange;
        _round.OnCardsPlayed += HandleCardsPlayed;
        _round.OnPlayerPass += p => Feedback(p.playerName + " passed");
        _round.OnPlayerSkipped += HandleSkipped;
        _round.OnPlayerOut += (p, place) => Feedback(p.playerName + " is out! Finished " + Ordinal(place));
        _round.OnSetOver += HandleSetOver;
        _round.OnRoundOver += HandleRoundOver;

        HumanTurnInput.Instance.OnInvalidPlay += msg => Feedback(msg);

        // start state
        SetTurnButtons(false);
        nextSetButton.gameObject.SetActive(false);
        nextRoundButton.gameObject.SetActive(true);
        turnText.text = "";
        Feedback("Press Start Round to deal");

        // buttons hide themselves when clicked
        nextRoundButton.onClick.AddListener(() =>
        {
            nextRoundButton.gameObject.SetActive(false);
            nextSetButton.gameObject.SetActive(false);
            Feedback("Dealing...");
        });
        nextSetButton.onClick.AddListener(() => nextSetButton.gameObject.SetActive(false));
    }

    private void HandleTurnChange(Player p)
    {
        bool human = p is HumanPlayer;
        turnText.text = human ? "Your turn" : p.playerName + "'s turn";

        // only show buttons on the human's turn, and no pass when leading
        playButton.gameObject.SetActive(human);
        passButton.gameObject.SetActive(human && _round.CurrentPlay != null);

        if (human && _round.MustMatch)
            Feedback("You're being skipped! Play a matching " + Describe(_round.CurrentPlay) + " or pass");
        else if (human && _round.CurrentPlay == null)
            Feedback("You're leading. Select 1 to 3 cards of the same rank");
    }

    private void HandleCardsPlayed(Player p, Move move)
    {
        SetTurnButtons(false);
        string who = p is HumanPlayer ? "You" : p.playerName;
        Feedback(who + " played " + Describe(move));
    }

    private void HandleSkipped(Player p)
    {
        string who = p is HumanPlayer ? "You were" : p.playerName + " was";
        Feedback(who + " skipped");
    }

    private void HandleSetOver(Player winner)
    {
        SetTurnButtons(false);
        turnText.text = "";
        Feedback(winner.playerName + " wins the set");
        nextSetButton.gameObject.SetActive(true);
    }

    private void HandleRoundOver(Player winner)
    {
        SetTurnButtons(false);
        turnText.text = "";
        Feedback("Round over! " + winner.playerName + " is the President");
        nextSetButton.gameObject.SetActive(false);
        nextRoundButton.gameObject.SetActive(true);
    }

    private void SetTurnButtons(bool on)
    {
        playButton.gameObject.SetActive(on);
        passButton.gameObject.SetActive(on);
    }

    private void Feedback(string msg) { feedbackText.text = msg; }

    private static string Describe(Move m)
    {
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
}

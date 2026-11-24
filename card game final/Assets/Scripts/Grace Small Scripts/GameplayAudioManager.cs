using System;
using System.Collections.Generic;
using UnityEngine;


public class GameplayAudioManager : MonoBehaviour
{
    [Serializable]
    public class Cue
    {
        public AudioClip clip;
        
        public AudioSource source;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Default Source")]
   
    [SerializeField] private AudioSource defaultSource;

    [Header("Turn")]
    [SerializeField] private Cue humanTurnStart;
    [SerializeField] private Cue aiTurnStart;

    [Header("Cards")]
    [SerializeField] private Cue cardsPlayed;
    [SerializeField] private Cue playerPass;
    [SerializeField] private Cue playerSkipped;
    [SerializeField] private Cue playerOut;

    [Header("Set")]
    [SerializeField] private Cue setOver;
    [SerializeField] private Cue setClosedOut;

    [Header("Round")]
    [SerializeField] private Cue roundWon;
    [SerializeField] private Cue roundLost;
    [SerializeField] private Cue titlesAssigned;

    [Header("Mix")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;

    private RoundController _round;

    private void Awake()
    {
        if (defaultSource == null)
        {
            defaultSource = GetComponent<AudioSource>();
            if (defaultSource == null) defaultSource = gameObject.AddComponent<AudioSource>();
            defaultSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        RoundController.Created += Subscribe;
        if (RoundController.Current != null) Subscribe(RoundController.Current);
    }

    private void OnDisable()
    {
        RoundController.Created -= Subscribe;
        Unsubscribe();
    }

    private void Subscribe(RoundController round)
    {
        Unsubscribe();
        _round = round;

        _round.OnTurnChange += HandleTurnChange;
        _round.OnCardsPlayed += HandleCardsPlayed;
        _round.OnPlayerPass += HandlePass;
        _round.OnPlayerSkipped += HandleSkipped;
        _round.OnSetOver += HandleSetOver;
        _round.OnSetClosedOut += HandleSetClosedOut;
        _round.OnPlayerOut += HandlePlayerOut;
        _round.OnRoundOver += HandleRoundOver;
        _round.OnTitlesAssigned += HandleTitlesAssigned;
    }

    private void Unsubscribe()
    {
        if (_round == null) return;

        _round.OnTurnChange -= HandleTurnChange;
        _round.OnCardsPlayed -= HandleCardsPlayed;
        _round.OnPlayerPass -= HandlePass;
        _round.OnPlayerSkipped -= HandleSkipped;
        _round.OnSetOver -= HandleSetOver;
        _round.OnSetClosedOut -= HandleSetClosedOut;
        _round.OnPlayerOut -= HandlePlayerOut;
        _round.OnRoundOver -= HandleRoundOver;
        _round.OnTitlesAssigned -= HandleTitlesAssigned;
        _round = null;
    }

    // ---- Handlers ----

    private void HandleTurnChange(Player p) => Play(p is HumanPlayer ? humanTurnStart : aiTurnStart);
    private void HandleCardsPlayed(Player p, Move move) => Play(cardsPlayed);
    private void HandlePass(Player p) => Play(playerPass);
    private void HandleSkipped(Player p) => Play(playerSkipped);
    private void HandlePlayerOut(Player p, int place) => Play(playerOut);

    private void HandleSetOver(Player winner) => Play(setOver);
    private void HandleSetClosedOut(Player p) => Play(setClosedOut);

    private void HandleRoundOver(Player winner) => Play(winner is HumanPlayer ? roundWon : roundLost);
    private void HandleTitlesAssigned(List<Player> order) => Play(titlesAssigned);

    // ---- Helper ----

    private void Play(Cue cue)
    {
        if (cue == null || cue.clip == null) return;

        AudioSource source = cue.source != null ? cue.source : defaultSource;
        if (source == null) return;

        source.PlayOneShot(cue.clip, cue.volume * masterVolume);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class HumanTurnInput : MonoBehaviour
{
    public static HumanTurnInput Instance;
    private void Awake() { Instance = this; }
    
    private readonly List<CardData> _selected = new List<CardData>();
    public IReadOnlyList<CardData> Selected => _selected;
    
    public CardData clickedCard;
    public bool playClicked;
    public bool passClicked;
    
    public bool IsSelected(CardData card) => _selected.Contains(card);
    public bool SelectionEnabled { get; private set; }

    public event Action OnSelectionChanged; // HandManager subscribes to lift/lower cards
    public event Action<string> OnInvalidPlay;

    public void CardClicked(CardData card)
    {
        if (!SelectionEnabled) return;
        
        if (!_selected.Remove(card))
        {
            if (_selected.Count >= GameRules.MAX_CARDS_IN_MOVE) return; // can't select more than a move allows
            _selected.Add(card);
        }
        OnSelectionChanged?.Invoke();
    }  // called by card UI

    public void PlayClicked() { playClicked = true; } // called by play button 
    public void PassClicked() { passClicked = true; } // called by pass button

    public void InvalidPlay(string msg) { OnInvalidPlay?.Invoke(msg); }
    
    public void SetSelectionEnabled(bool on)
    {
        SelectionEnabled = on;
        if (!on) ClearSelection();
    }
    
    public void Clear()
    {
        playClicked = false;
        passClicked = false;
    }

    public void ClearSelection()
    {
        if (_selected.Count == 0) return;
        
        _selected.Clear();
        OnSelectionChanged?.Invoke();
    }
}

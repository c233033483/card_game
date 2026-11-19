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

    public event Action OnSelectionChanged; // HandManager subscribes to lift/lower cards

    public void CardClicked(CardData card)
    {
        if (!_selected.Remove(card))
        {
            if (_selected.Count >= GameRules.MAX_CARDS_IN_MOVE) return; // can't select more than a move allows
            _selected.Add(card);
        }
        OnSelectionChanged?.Invoke();
    }  // called by card UI

    public void PlayClicked()
    {
        Debug.Log("PlayClicked called\n" + System.Environment.StackTrace);
        playClicked = true;
    } // called by play button 
    public void PassClicked() { passClicked = true; } // called by pass button

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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
    public string playerName;
    public List<CardData> hand = new List<CardData>();

    public void SortHand()
    {
        if (hand.Count != 0)
        {
            hand.Sort((x, y) => x.rank.CompareTo(y.rank));
        }
    }

    // Will call onDecided (callback) when a card is played, or null for a pass
    public abstract IEnumerator PlayCard(Action<CardData> onDecided);  
}

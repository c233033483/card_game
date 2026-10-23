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


    public abstract CardData PlayCard(); 
}

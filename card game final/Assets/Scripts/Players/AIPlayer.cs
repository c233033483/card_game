using System;
using System.Collections;
using UnityEngine;

public class AIPlayer : Player
{
    public override IEnumerator PlayCard(Action<CardData> onDecided)
    {
        yield return new WaitForSeconds(0.5f); 
        
        foreach (CardData card in hand) // Sorted low-high
        {
            if (Pile.instance.IsCardValid(card))
            {
                onDecided(card); // Put down on top of the placed card
                yield break;
            }
        }
        
        onDecided(null); // nothing valid, so pass
    }
}

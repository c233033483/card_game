using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    public List<CardData> hand = new List<CardData>();
    
    public GameManager gameManager;

    public void SortHand()
    {
        if (hand.Count != 0)
        {
            hand.Sort((x, y) => x.rank.CompareTo(y.rank));
        }
    }

    
    public void PlayCard(CardData lastPlayed)
    {
        foreach (CardData card in hand) // Sorted low-high
        {
            Debug.Log(card.rank);
            if (card.rank > lastPlayed.rank)
            {
                lastPlayed.rank = card.rank; //needs to go back to the game manager, should i reference it ??
                Debug.Log("Card put down: " + lastPlayed.rank);
            }
        }
        
        //gameManager.StartNextTurn();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>

public class Deck
{
    internal readonly List<CardData> gameDeck = new List<CardData>(); 
    
    public void BuildDeck()
    {
        var suits = Enum.GetValues(typeof(Suits));
        var ranks = Enum.GetValues(typeof(Ranks));


        foreach (Suits suit in suits)
        {
            foreach (Ranks rank in ranks)
            {
                var card = new CardData();
                card.suit = suit;
                card.rank = rank;
                gameDeck.Add(card);
            }
        }

        Debug.Log("Deck built:  " + gameDeck.Count);
    }

    /// <summary>
    /// Created with the Fisher-Yates idea in mind
    /// </summary>
    public void RandomiseDeck()
    {
        Debug.Log("Randomising deck....");

        for (int i = (gameDeck.Count - 1) ; i > 0; i--)
        {
            int newIndex = UnityEngine.Random.Range(0, i + 1);
            CardData temp = gameDeck[newIndex];
            gameDeck[newIndex] = gameDeck[i];
            gameDeck[i] = temp;
        }
        
        /*
        for (int i = 0; i < gameDeck.Count; i++)
        {
            CardData card = gameDeck[i];

            int newIndex = UnityEngine.Random.Range(0, gameDeck.Count-1);
            CardData temp = gameDeck[newIndex];
            gameDeck[newIndex] = card;
            gameDeck[i] = temp;
        }
        */
    }
}
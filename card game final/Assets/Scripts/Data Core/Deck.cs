using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<CardData> gameDeck = new List<CardData>();
    
    public TMP_Text texthand1;
    public TMP_Text texthand2;
    public TMP_Text texthand3;
    public TMP_Text texthand4;

    private void Start()
    {
        BuildDeck();
    }


    private void BuildDeck()
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
        
        for (int i = 0; i < 3; i++)
            RandomiseDeck();
        
        Deal();
    }

    /// <summary>
    /// Created with the Fisher-Yates idea in mind
    /// </summary>
    private void RandomiseDeck()
    {
        Debug.Log("Randomising deck....");

        for (int i = (gameDeck.Count - 1) ; i > 0; i--)
        {
            int newIndex = UnityEngine.Random.Range(0, gameDeck.Count-1);
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

    private void Deal()
    {
        List<List<CardData>> hands = new List<List<CardData>>(); //in future, have a list of decks from the player class.

        for (int i = 0; i < 4; i++)
        {
            hands.Add(new List<CardData>());
        }

        for (int i = 0; i < 52; i++)
        {
            hands[i % 4].Add(gameDeck[i]); //players[i%players.count].Add(gamedeck[i]);
        }
        
        texthand1.text = HandToString(hands[0]);
        texthand2.text = HandToString(hands[1]);
        texthand3.text = HandToString(hands[2]);
        texthand4.text = HandToString(hands[3]);
    }
    
    
    
    /// <summary>
    /// Helper for debugging
    /// </summary>
    /// <returns></returns>
    
    private static string HandToString(List<CardData> cd)
    {
        var sb = new System.Text.StringBuilder();
        foreach (CardData card in cd)
        {
            sb.AppendLine(card.rank + " of " + card.suit);
        }
        return sb.ToString();
    }   
}
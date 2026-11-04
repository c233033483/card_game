using System;
using Mono.Cecil;

[Serializable]
public class CardData
{
    public Suits suit;
    public Ranks rank;

    public string FindFace()
    {
        var face = rank switch
        {
            Ranks.Ace => "A",
            Ranks.Two => "2",
            Ranks.Three => "3",
            Ranks.Four => "4",
            Ranks.Five => "5",
            Ranks.Six => "6",
            Ranks.Seven => "7",
            Ranks.Eight => "8",
            Ranks.Nine => "9",
            Ranks.Ten => "10",
            Ranks.Jack => "J",
            Ranks.Queen => "Q",
            Ranks.King => "K",
            _ => ""
        };
        return face;
    }
}

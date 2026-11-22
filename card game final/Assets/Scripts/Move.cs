using System.Collections.Generic;

public class Move
{
    public readonly List<CardData> Cards;
    public Move(List<CardData> cards) { Cards = cards; }
    public bool IsPass => Cards.Count == 0;
    public int Count => Cards.Count;
    public Ranks Rank => Cards[0].rank;

    public static Move Pass() => new Move(new List<CardData>());
}

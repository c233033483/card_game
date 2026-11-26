using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardFace", menuName = "Scriptable Objects/CardFace")]
public class CardPrefabAsset : ScriptableObject
{
    [System.Serializable]
    public struct CardEntry
    {
        public Suits suit;
        public Ranks rank;
        public GameObject prefab;
    }

    public CardEntry[] cardEntries;
    
    private Dictionary<(Suits, Ranks), GameObject> _lookup; // Key is the data, value is the prefab gameobject
    
    public GameObject GetPrefab(CardData data)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<(Suits, Ranks), GameObject>();
            foreach (var e in cardEntries)
                _lookup[(e.suit, e.rank)] = e.prefab;
        }

        _lookup.TryGetValue((data.suit, data.rank), out var prefab);
        return prefab;
    }
    
}

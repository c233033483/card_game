using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardPrefabAsset cardPrefab;
    [SerializeField] private Transform[] topCardsPositions; 
    
    private readonly List<GameObject> _spawned = new List<GameObject>();
    
    public static Pile instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        ClearPile();
    }
    
    public void AddCard(Move move, float delay)
    {
        if (move == null || move.IsPass) return;

        StartCoroutine(AddAfterDelay(move, delay));
    }

    private IEnumerator AddAfterDelay(Move move, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var card in move.Cards)
        {
            GameObject prefab =  cardPrefab.GetPrefab(card);
            if (prefab == null)
            {
                Debug.LogError($"No prefab for {card.rank} of {card.suit}");
                continue;
            }

            GameObject go = Instantiate(prefab, transform);
            CardObject co = go.GetComponent<CardObject>();
            
            if (co == null) co = go.AddComponent<CardObject>();
            co.cardData = card;

            _spawned.Add(go);
        }

        // keep only the last N cards (N = number of slots)
        while (_spawned.Count > topCardsPositions.Length)
        {
            Destroy(_spawned[0]);
            _spawned.RemoveAt(0);
        }
        
        ReorderPile();
    }

    // newest card goes in slot 0 (top), older cards push down toward the last slot
    private void ReorderPile()
    {
        int count = _spawned.Count;

        for (int i = 0; i < count; i++)
        {
            Transform slot = topCardsPositions[count - 1 - i];
            _spawned[i].transform.SetPositionAndRotation(
                slot.position,
                slot.rotation * Quaternion.Euler(0f, 0f, 0f));
        }
    }

    public void ClearPile()
    {
        StopAllCoroutines();
        
        foreach (var go in _spawned)
            if (go) Destroy(go);

        _spawned.Clear();
    }
}

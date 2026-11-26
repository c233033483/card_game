using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardPrefabAsset cardPrefab;
    [SerializeField] private Transform[] topCardsPositions; 
    private Vector3 _baseTopCardPosition;
    private Coroutine _moveCoroutine;
    
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

        // a newer move replaces any one still waiting to land
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(AddAfterDelay(move, delay));
    }

    private IEnumerator AddAfterDelay(Move move, float delay)
    {
        yield return new WaitForSeconds(delay);
        _moveCoroutine = null;
        
        if (move.Count > topCardsPositions.Length)
            Debug.LogWarning("Pile has " + topCardsPositions.Length + " slots but the move has " + move.Count + " cards");

        DestroySpawned();
        
        for (int i = 0; i < move.Count; i++)
        {
            /*
            bool active = i < move.Count;
            topCardsPositions[i].gameObject.SetActive(active);
            if (!active) continue;
            */

            var card = move.Cards[i];

            GameObject prefab = cardPrefab.GetPrefab(card);
            if (prefab == null)
            {
                Debug.LogError($"No prefab for {card.rank} of {card.suit}");
                continue;
            }

            GameObject go = Instantiate(prefab, transform);
            
            Vector3 cardOffset = new Vector3(0.4f, 0f, -0.01f);
            float spread = i - (move.Count - 1) * 0.5f;
            go.transform.localPosition = _baseTopCardPosition + cardOffset * spread;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            CardObject co = go.GetComponent<CardObject>();
            if (co == null) co = go.AddComponent<CardObject>();
            co.cardData = card;

            _spawned.Add(go);
        }
    }

    public void ClearPile()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        DestroySpawned();
    }
    
    private void DestroySpawned()
    {
        foreach (var go in _spawned)
            if (go != null) Destroy(go);

        _spawned.Clear();
    }
}

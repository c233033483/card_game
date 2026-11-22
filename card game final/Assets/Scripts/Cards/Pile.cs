using System.Collections;
using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardObject[] topCards; 
    private Vector3 _baseTopCardPosition;
    private Coroutine _moveCoroutine;
    
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
        
        if (move.Count > topCards.Length)
            Debug.LogWarning("Pile has " + topCards.Length + " slots but the move has " + move.Count + " cards");

        for (int i = 0; i < topCards.Length; i++)
        {
            bool active = i < move.Count;
            topCards[i].gameObject.SetActive(active);
            if (!active) continue;

            Vector3 cardOffset = new Vector3(0.4f, 0f, -0.01f);
            float spread = i - (move.Count - 1) * 0.5f;
            topCards[i].transform.localPosition = _baseTopCardPosition + cardOffset * spread;

            CardData card = move.Cards[i];
            topCards[i]._cardData = card;
            topCards[i].UpdateText(card.FindFace());
            topCards[i].UpdateSprite();
        }
    }

    public void ClearPile()
    {
        foreach (var card in topCards)
        {
            card._cardData = null;
            card.gameObject.SetActive(false);
        }
    }
}

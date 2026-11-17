using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardObject[] topCards; 
    private Vector3 _baseTopCardPosition;
    
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
    
    public void AddCard(Move move)
    {
        if (move == null || move.IsPass) return;
        if (move.Count >= topCards.Length)
            Debug.LogWarning("Pile has " + topCards.Length + " slots but the move has " + move.Count + " cards");

        for (int i = 0; i < topCards.Length; i++)
        {
            //set card object active state based on how many cards there are compared to 
            bool active = i < move.Count;
            topCards[i].gameObject.SetActive(active);
            if (!active) continue;

            // Centre the group so a single card stays at the base position
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

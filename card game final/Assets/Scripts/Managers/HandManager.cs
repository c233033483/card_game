using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Manages the visual representation of the human player's hand
/// Cards are spawned at the spawnPoint and animated along a spline 
/// </summary>
public class HandManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;
    
    [Header("Layout")]
    [SerializeField] private int maxCards = 13;
    [SerializeField] private float minSpacing = 0.07f;
    [SerializeField] private float maxSpacing = 0.7f; // total t range allowed (0.7 = 0.15 to 0.85)
    [SerializeField] private float maxTiltDegrees = 60f; // z tilt at t = 0 / 1 (scaled by (0.5 - t) * 2)
    [SerializeField] private float zStep = 0.01f; // small depth offset per card so they layer
    
    [Header("Card Prefabs")]
    [Tooltip("0=Clubs,1=Spades,2=Diamonds,3=Hearts")]
    [SerializeField] private GameObject[] cardPrefabs;  
    
    private readonly List<CardObject> _cards = new List<CardObject>();

    

    /// <summary>
    /// Spawns a card using the given data at the spawn point and animates the hand into place
    /// </summary>
    /// <param name="cardData">The card to display.</param>
    public void AddCard(CardData cardData)
    {
        GameObject go = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        CardObject co = go.GetComponent<CardObject>();
        co._cardData.rank = cardData.rank;
        co._cardData.suit = cardData.suit;
        
        co.cardText.text = co._cardData.rank + " of " +  co._cardData.suit;

        go.GetComponent<SelectableCard>().Init(cardData);
        
        _cards.Add(co);
        UpdateCardPositions();
    }

    /// <summary>
    /// Removes and destroys the visual card matching the given data, then rejigs the hand
    /// Does nothing if there is no matching card
    /// </summary>
    /// <param name="card">The card to remove.</param>
    public void RemoveCard(CardData card)
    {
        CardObject match = _cards.Find(c => c._cardData.rank == card.rank && c._cardData.suit == card.suit);
        if (!match) return;

        _cards.Remove(match);
        match.transform.DOKill();
        Destroy(match.gameObject);
        UpdateCardPositions();
    }

    public void ClearHand()
    {
        foreach (CardObject c in _cards)
        {
            c.transform.DOKill();
            Destroy(c.gameObject);
        }
        _cards.Clear();
    }
    
    /// <summary>
    /// Animates each card into its slot along the spline's tangent
    /// </summary>
    public void UpdateCardPositions()
    {
        int n = _cards.Count;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < n; i++)
        {
            Transform c = _cards[i].transform;
            float t = GetT(i, n);
            
            Vector3 local = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(local);
            worldPos += Vector3.back * (zStep * i);

            Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t));
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);

            c.DOKill();
            c.DOMove(worldPos, 0.25f);
            c.DORotateQuaternion(rot, 0.25f);
        }
    }
    

    private float GetT(int index, int count)
    {
        if (count <= 1) return 0.5f;

        float spacing = Mathf.Min(minSpacing, maxSpacing / (count - 1));
        float offsetFromCenter = index - (count - 1) * 0.5f;
        return 0.5f + offsetFromCenter * spacing;
    }
}


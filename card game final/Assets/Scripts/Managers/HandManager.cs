using System;
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
    [Header("References")] [SerializeField]
    private SplineContainer splineContainer;
    [SerializeField] private CardPrefabAsset cardPrefabs;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform playPoint;

    [SerializeField] private bool isHuman;
    
    [Header("Layout")] 
    [SerializeField] private int maxCards = 13;
    [SerializeField] private float minSpacing = 0.07f;
    [SerializeField] private float maxSpacing = 0.7f; // total t range allowed (0.7 = 0.15 to 0.85)
    [SerializeField] private float maxTiltDegrees = 60f; // z tilt at t = 0 / 1 (scaled by (0.5 - t) * 2)
    [SerializeField] private float zStep = 0.01f; // small depth offset per card so they layer

    [Header("Card Prefabs")] 
    [Tooltip("0=Clubs,1=Spades,2=Diamonds,3=Hearts")] 
    [SerializeField] private float selectLift = 0.3f;

    private readonly List<CardObject> _cards = new List<CardObject>();

    private void Start()
    {
        if (isHuman)
          HumanTurnInput.Instance.OnSelectionChanged += UpdateCardPositions;
    }

    private void OnDestroy()
    {
        if (isHuman)
            if (HumanTurnInput.Instance != null)
                HumanTurnInput.Instance.OnSelectionChanged -= UpdateCardPositions;
    }
    

    /// <summary>
    /// Spawns a card using the given data at the spawn point and animates the hand into place
    /// </summary>
    /// <param name="cardData">The card to display.</param>
    public void AddCard(CardData cardData)
    {
        GameObject cardPrefab = cardPrefabs.GetPrefab(cardData);
        Debug.Log("Card added: " + cardData.rank + " of " + cardData.suit);
        if (cardPrefab == null)
        {
            Debug.LogError($"No prefab for {cardData.rank} of {cardData.suit}");
            return;
        }
        
        GameObject go = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        
        CardObject co = go.GetComponent<CardObject>();
        if (!co) co = go.AddComponent<CardObject>();
        co.UpdateCardData(cardData);

        if (isHuman)
        {
            SelectableCard selectable = go.GetComponent<SelectableCard>();
            if (selectable == null) selectable = go.AddComponent<SelectableCard>();
            selectable.Init(cardData);
        }

        _cards.Add(co);
        UpdateCardPositions();
    }

    /// <summary>
    /// Removes and destroys the visual card matching the given data, then rejigs the hand
    /// Does nothing if there is no matching card
    /// </summary>
    /// <param name="cards">The card(s) to remove.</param>
    public void RemoveCard(List<CardData> cards, float delay)
    {
        foreach (var card in cards)
        {
            CardObject match = _cards.Find(c => c.cardData.rank == card.rank && c.cardData.suit == card.suit); 
            if (!match) continue;
            
            _cards.Remove(match);
            MoveToPlay(match, delay);
        }
        
        UpdateCardPositions();
    }

    private void MoveToPlay(CardObject co, float delay)
    {
        GameObject go = co.gameObject;
        Transform t = co.transform;

        // stop it being clickable while it flies
        var selectable = go.GetComponent<SelectableCard>();
        if (selectable) selectable.enabled = false;
        foreach (var col in go.GetComponentsInChildren<BoxCollider>()) col.enabled = false;

        t.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Join(t.DOMove(playPoint.position, delay).SetEase(Ease.OutQuad));
        seq.Join(t.DORotateQuaternion(playPoint.rotation * Quaternion.Euler(0f, 180f, 0f), delay));
        seq.SetLink(go); // kills the tween if the object is destroyed early
        seq.OnComplete(() => Destroy(go));
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
    private void UpdateCardPositions()
    {
        int n = _cards.Count;
        Spline spline = splineContainer.Spline;
        Transform sc = splineContainer.transform;

        for (int i = 0; i < n; i++)
        {
            Transform c = _cards[i].transform;
            float t = GetT(i, n);

            Vector3 worldPos = sc.TransformPoint(spline.EvaluatePosition(t));
            worldPos += sc.TransformDirection(Vector3.back) * (zStep * i);

            if (isHuman && HumanTurnInput.Instance.IsSelected(_cards[i].cardData))
                worldPos += sc.TransformDirection(Vector3.up) * selectLift;

            Vector3 tangent = spline.EvaluateTangent(t); // local space
            float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
            
            Quaternion rot = sc.rotation * Quaternion.Euler(0f, 0f, angle) * Quaternion.Euler(0f, 180f, 0f);

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
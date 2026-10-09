using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class HandManager : MonoBehaviour
{
    [SerializeField] private int maxCards = 13;
    [SerializeField] private float minSpacing = 0.07f;
    [SerializeField] private float maxSpacing = 0.7f; // total t range allowed (0.7 = 0.15 to 0.85)
    [SerializeField] private float maxTiltDegrees = 60f; // z tilt at t = 0 / 1 (scaled by (0.5 - t) * 2)
    [SerializeField] private float zStep = 0.01f; // small depth offset per card so they layer

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Transform spawnPoint;

    private List<CardObject> cards = new List<CardObject>();

    public void AddCard(CardData cardData)
    {
        GameObject go = Instantiate(cardPrefab, spawnPoint.position, spawnPoint.rotation);
        CardObject co = go.GetComponent<CardObject>();
        co._cardData.rank = cardData.rank;
        co._cardData.suit = cardData.suit;
        
        co.cardText.text = co._cardData.rank + " of " +  co._cardData.suit;
            
            //Debug.LogWarning("No TMP_Text found on card prefab", go);

        cards.Add(co);
        UpdateCardPositions();
    }

    public void RemoveCard(CardObject card)
    {
        if (cards.Remove(card))
            UpdateCardPositions();
    }

    public void UpdateCardPositions()
    {
        int n = cards.Count;
        Spline spline = splineContainer.Spline;

        for (int i = 0; i < n; i++)
        {
            Transform c = cards[i].transform;
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




/*
private void UpdateCardPositions()
{
    if (cards.Count == 0) return;

    float cardSpacing = 1f / maxCards;
    float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;

    Spline spline = splineContainer.Spline;
    for (int i = 0; i < cards.Count; i++)
    {
        float pos = firstCardPosition + i * cardSpacing;

        Vector3 localPoint = spline.EvaluatePosition(pos);
        Vector3 worldPoint = splineContainer.transform.TransformPoint(localPoint);

        // t runs from -1 (leftmost card) to 1 (rightmost card), 0 for a lone card
        float t = cards.Count > 1 ? (cards.Count - 1) / (float)(maxCards - 1) * 2f - 1f : 0f;

        // negative so the left cards lean left (counterclockwise looking down +Z)
        Quaternion rotation = Quaternion.Euler(0f, 0f, -t * maxTiltDegrees);

        cards[i].transform.DOMove(worldPoint, 0.25f);
        cards[i].transform.DORotateQuaternion(rotation, 0.25f);
    }
}



private void UpdateCardPositions()
{
    if (cards.Count == 0) return;

    var cardSpacing = 1f / 10f;
    var firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2;

    Spline spline = splineContainer.Spline;
    for (int i = 0; i < cards.Count; i++)
    {
        float pos = firstCardPosition + i * cardSpacing;
        Vector3 splinePosition = spline.EvaluatePosition(pos);
        Vector3 forward = spline.EvaluateTangent(pos);
        Vector3 up = spline.EvaluateUpVector(pos);
        Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);

        cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, 0.25f);
        cards[i].transform.DORotate(rotation.eulerAngles, 0.25f);

    }
}*/


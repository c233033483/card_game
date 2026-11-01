using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SelectableCard : MonoBehaviour
{
    public CardData data;

    public void Init(CardData cardData)
    {
        data = cardData;
    }

    private void OnMouseDown()
    {
        HumanTurnInput.instance.CardClicked(data);
    }
}

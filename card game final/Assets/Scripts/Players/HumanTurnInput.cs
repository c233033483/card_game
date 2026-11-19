using UnityEngine;

public class HumanTurnInput : MonoBehaviour
{
    public static HumanTurnInput Instance;
    private void Awake() { Instance = this; }
    
    public CardData clickedCard;
    public bool passClicked;

    public void CardClicked(CardData card)
    {
        clickedCard = card;
        Debug.Log("Card clicked");
    }  // called by card UI

    public void PassClicked()
    {
        passClicked = true;
    }               // called by pass button

    public void Clear()
    {
        clickedCard = null; passClicked = false;
    }
}

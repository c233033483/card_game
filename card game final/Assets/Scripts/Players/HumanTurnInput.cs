using UnityEngine;

public class HumanTurnInput : MonoBehaviour
{
    public static HumanTurnInput instance;
    private void Awake() { instance = this; }
    
    public CardData clickedCard;
    public bool passClicked;

    public void CardClicked(CardData card)
    {
        clickedCard = card;
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

using System.Linq;
using TMPro;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    public SpriteRenderer renderer;
    public CardData _cardData;
    [SerializeField] private TMP_Text[] cardTexts =  new TMP_Text[3];

    [Tooltip("0=Clubs,1=Spades,2=Diamonds,3=Hearts")]
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite[] cardSprites;

    public void UpdateCardData(CardData data)
    {
        _cardData = data;     
    }
    
    public void UpdateText(string face)
    {
        print(face);
        foreach (var text in cardTexts)
        {
            text.text = face;
        }
    }

    public void UpdateSprite()
    {
        switch (_cardData.suit)
        {
            case Suits.Clubs:
                currentSprite = cardSprites[0];
                return;
            case Suits.Spades:
                currentSprite = cardSprites[1];
                return;
            case Suits.Diamonds:
                currentSprite = cardSprites[2];
                return;
            case Suits.Hearts:
                currentSprite = cardSprites[3];
                return;
            default:
                return;
        }

        renderer.sprite = currentSprite;
    }
}

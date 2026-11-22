using System.Linq;
using TMPro;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    public SpriteRenderer renderer;
    public CardData _cardData;
    [SerializeField] private TMP_Text[] cardTexts =  new TMP_Text[3];
    private static readonly Color RedSuit = new Color32(0xC6, 0x27, 0x26, 0xFF);
    
    
    [Tooltip("0=Clubs,1=Spades,2=Diamonds,3=Hearts")]
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite[] cardSprites;

    public void UpdateCardData(CardData data)
    {
        _cardData = data;     
    }
    
    public void UpdateText(string face)
    {
        bool isRed = _cardData.suit == Suits.Diamonds || _cardData.suit == Suits.Hearts;
        Color textColor = isRed ? RedSuit : Color.black;
        
        foreach (var text in cardTexts)
        {
            text.text = face;
            text.color = textColor;
        }
    }

    public void UpdateSprite()
    {
        switch (_cardData.suit)
        {
            case Suits.Clubs:
                currentSprite = cardSprites[0];
                break;
            case Suits.Spades:
                currentSprite = cardSprites[1];
                break;
            case Suits.Diamonds:
                currentSprite = cardSprites[2];
                break;
            case Suits.Hearts:
                currentSprite = cardSprites[3];
                break;
            default:
                return;
        }

        renderer.sprite = currentSprite;
    }
}

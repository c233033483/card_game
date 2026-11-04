using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardObject topCard; 
    private CardData _topCardData; 
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
    
    public void AddCard(CardData card)
    {
        if (!topCard.gameObject.activeInHierarchy)
        {
            topCard.gameObject.SetActive(true);
        }
        
        _topCardData = card;
        topCard._cardData = card;
        var face = topCard._cardData.FindFace();
        topCard.UpdateText(face);
        topCard.UpdateSprite();
    }

    public void ClearPile()
    {
        _topCardData = null;
        topCard._cardData = null;

        topCard.gameObject.SetActive(false);
    }

    public bool IsCardValid(CardData card)
    {
        if (_topCardData == null)
            return true; // this means the pile is empty, so anything can be played
        return card.rank > _topCardData.rank;
    }
}

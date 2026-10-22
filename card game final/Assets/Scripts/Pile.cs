using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] private CardObject topCard;

    #region Singleton
    public static Pile instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
    
    #endregion

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

        var cd = topCard._cardData;
        cd = card; 
        topCard.cardText.text = cd.rank + " of " +  cd.suit;
    }

    private void ClearPile()
    {
        topCard = null;
        topCard.gameObject.SetActive(false);
    }
}

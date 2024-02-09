using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();

        Card.OnCardPressed += Card_OnCardPressed;
    }

    private void OnDestroy()
    {
        Card.OnCardPressed -= Card_OnCardPressed;
    }

    private void Card_OnCardPressed(object sender, Player player)
    {
        Card card = sender as Card;
        image.sprite = card.Sprite;
    }
}

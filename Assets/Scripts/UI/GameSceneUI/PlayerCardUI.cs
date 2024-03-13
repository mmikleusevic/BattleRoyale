using UnityEngine;
using UnityEngine.UI;

public class PlayerCardUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    private void Awake()
    {
        Hide();
    }

    public void Instantiate(Card card)
    {
        cardImage.sprite = card.Sprite;

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PlayerCardUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    public int Index { get; private set; } = -1;

    private void Awake()
    {
        Hide();
    }

    public void Instantiate(Card card, int index)
    {
        cardImage.sprite = card.Sprite;
        cardImage.preserveAspect = true;
        Index = index;

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

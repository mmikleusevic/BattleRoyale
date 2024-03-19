using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardsUnequippedUI : MonoBehaviour
{
    [SerializeField] private RectTransform PlayerCardsUnequippedUIRectTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform container;
    [SerializeField] private Transform template;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image background;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            HideWithAnimation();
        });

        Player.OnPlayerUnequippedCardAdded += Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards += PlayerCardsEquippedUI_OnShowUnequippedCards;

        template.gameObject.SetActive(false);

        HideInstantly();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();

        Player.OnPlayerUnequippedCardAdded -= Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards -= PlayerCardsEquippedUI_OnShowUnequippedCards;
    }

    private void Player_OnPlayerUnequippedCardAdded(Card obj)
    {
        Transform cardUITransform = Instantiate(template, container);

        cardUITransform.gameObject.SetActive(false);

        Image image = cardUITransform.GetComponent<Image>();

        image.sprite = obj.Sprite;        
    }

    private void PlayerCardsEquippedUI_OnShowUnequippedCards()
    {
        ShowWithAnimation();
    }

    private void Show()
    {
        foreach (Transform item in container)
        {
            if (item == template) continue;

            item.gameObject.SetActive(true);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        foreach (Transform item in container)
        {
            if (item == template) continue;

            item.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public void ShowWithAnimation()
    {      
        Show();
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    public void HideWithAnimation()
    {
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    public void HideInstantly()
    {
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }
}

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
    [SerializeField] private ConfirmDialogUI confirmDialogUI;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            HideWithAnimation();
        });

        Player.OnPlayerUnequippedCardAdded += Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards += PlayerCardsEquippedUI_OnShowUnequippedCards;

        confirmDialogUI.gameObject.SetActive(false);
        template.gameObject.SetActive(false);

        HideInstantly();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();

        Player.OnPlayerUnequippedCardAdded -= Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards -= PlayerCardsEquippedUI_OnShowUnequippedCards;
    }

    private void Player_OnPlayerUnequippedCardAdded(Card card)
    {
        Transform lastChild = container.GetChild(container.childCount - 1);
        PlayerCardUI lastPlayerCardUI = lastChild.GetComponent<PlayerCardUI>();

        int newLastPlayerIndex = lastPlayerCardUI.Index + 1;

        Transform cardUITransform = Instantiate(template, container);

        cardUITransform.gameObject.SetActive(true);

        PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

        playerCardUI.Instantiate(card, newLastPlayerIndex);
    }

    private void PlayerCardsEquippedUI_OnShowUnequippedCards()
    {
        titleText.text = "Unequipped cards:";

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

    private void ShowWithAnimation()
    {      
        Show();
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    private void HideWithAnimation()
    {
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void HideInstantly()
    {
        PlayerCardsUnequippedUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }
}

using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCardsUnequippedUI : MonoBehaviour
{
    public static event Action OnPlayerCardsUnequippedUIClosed;

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
            OnPlayerCardsUnequippedUIClosed?.Invoke();

            HideWithAnimation();
        });

        Player.OnPlayerUnequippedCardAdded += Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards += PlayerCardsEquippedUI_OnShowUnequippedCards;
        PlayerCardUI.OnEquippedCardPress += PlayerCardUI_OnEquippedCardPress;
        ConfirmSwapCardDialogUI.OnYesPressed += ConfirmSwapDialogUI_OnYesPressed;

        HideInstantly();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();

        Player.OnPlayerUnequippedCardAdded -= Player_OnPlayerUnequippedCardAdded;
        PlayerCardsEquippedUI.OnShowUnequippedCards -= PlayerCardsEquippedUI_OnShowUnequippedCards;
        PlayerCardUI.OnEquippedCardPress -= PlayerCardUI_OnEquippedCardPress;
        ConfirmSwapCardDialogUI.OnYesPressed -= ConfirmSwapDialogUI_OnYesPressed;
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

    private void PlayerCardUI_OnEquippedCardPress(PlayerCardUI obj)
    {
        ShowWithAnimation();

        foreach (Transform child in container)
        {
            if (child == template) continue;

            PlayerCardUI playerCardUI = child.GetComponent<PlayerCardUI>();
            playerCardUI.EnableTrigger();
        }
    }

    private void ConfirmSwapDialogUI_OnYesPressed(PlayerCardUI arg1, PlayerCardUI arg2)
    {
        if (arg1.isEmpty)
        {
            arg1.GetComponent<Image>().sprite = arg2.GetComponent<Image>().sprite;

            int index = arg2.Index + 1;

            Destroy(arg2.gameObject);

            for (int i = index; i < container.childCount; i++)
            {
                Transform playerCardUITransform = container.GetChild(i);

                PlayerCardUI playerCardUI = playerCardUITransform.GetComponent<PlayerCardUI>();

                playerCardUI.Index = playerCardUI.Index - 1;
            }
        }
        else
        {
            Sprite tempSprite = arg1.GetComponent<Image>().sprite;
            arg1.GetComponent<Image>().sprite = arg2.GetComponent<Image>().sprite;
            arg2.GetComponent<Image>().sprite = tempSprite;
        }

        HideWithAnimation();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        foreach (Transform item in container)
        {
            if (item == template)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
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

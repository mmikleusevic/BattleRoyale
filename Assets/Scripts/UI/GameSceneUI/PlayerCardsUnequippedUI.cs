using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
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

        PlayerCardsEquippedUI.OnShowUnequippedCards += PlayerCardsEquippedUI_OnShowUnequippedCards;
        PlayerCardUI.OnEquippedCardSwap += PlayerCardUI_OnEquippedCardPress;
        ConfirmSwapCardDialogUI.OnYesPressed += ConfirmSwapDialogUI_OnYesPressed;

        HideInstantly();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();

        PlayerCardsEquippedUI.OnShowUnequippedCards -= PlayerCardsEquippedUI_OnShowUnequippedCards;
        PlayerCardUI.OnEquippedCardSwap -= PlayerCardUI_OnEquippedCardPress;
        ConfirmSwapCardDialogUI.OnYesPressed -= ConfirmSwapDialogUI_OnYesPressed;
    }

    private void PlayerCardsEquippedUI_OnShowUnequippedCards(Player player)
    {
        titleText.text = "Unequipped cards:";
        ShowWithAnimation();

        PlayerCardUI.equippedCardState = EquippedCardState.None;

        InstantiateCards(player);
    }

    private void PlayerCardUI_OnEquippedCardPress(PlayerCardUI obj)
    {
        titleText.text = $"Swapping cards";
        ShowWithAnimation();

        PlayerCardUI.equippedCardState = EquippedCardState.Swap;

        InstantiateCards(Player.LocalInstance);
    }

    private void ConfirmSwapDialogUI_OnYesPressed(PlayerCardUI equippedCardsUI, PlayerCardUI unequippedCardsUI)
    {
        if (equippedCardsUI.isEmpty)
        {
            equippedCardsUI.GetComponent<Image>().sprite = unequippedCardsUI.GetComponent<Image>().sprite;
        }
        else
        {
            Sprite tempSprite = equippedCardsUI.GetComponent<Image>().sprite;
            equippedCardsUI.GetComponent<Image>().sprite = unequippedCardsUI.GetComponent<Image>().sprite;
            unequippedCardsUI.GetComponent<Image>().sprite = tempSprite;
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

    private void InstantiateCards(Player player)
    {
        for (int i = 0; i < player.UnequippedCards.Count; i++)
        {
            Transform cardUITransform = Instantiate(template, container);

            cardUITransform.gameObject.SetActive(true);

            PlayerCardUI playerCardUI = cardUITransform.GetComponent<PlayerCardUI>();

            playerCardUI.gameObject.SetActive(true);

            Card card = player.UnequippedCards[i];

            playerCardUI.Instantiate(card, i);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        foreach (Transform child in container)
        {
            if (child == template) continue;
            Destroy(child.gameObject);
        }
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

    public static void ResetStaticData()
    {
        OnPlayerCardsUnequippedUIClosed = null;
    }
}

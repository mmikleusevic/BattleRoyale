using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmSwapCardDialogUI : MonoBehaviour
{
    public static event Action<PlayerCardUI, PlayerCardUI> OnYesPressed;

    [SerializeField] private RectTransform ConfirmDialogUIRectTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private PlayerCardUI equippedPlayerCardUI;
    private PlayerCardUI unequippedPlayerCardUI;

    private Card equippedCard;
    private Card unequippedCard;

    private void Awake()
    {
        yesButton.onClick.AddListener(() =>
        {
            OnYesPressed?.Invoke(equippedPlayerCardUI, unequippedPlayerCardUI);

            equippedPlayerCardUI = null;
            unequippedPlayerCardUI = null;
            equippedCard = null;
            unequippedCard = null;

            HideWithAnimation();
        });

        noButton.onClick.AddListener(() =>
        {
            HideWithAnimation();
            unequippedCard = null;
            unequippedPlayerCardUI = null;
        });

        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed += PlayerCardsEquippedUI_OnPlayerCardsEquippedUIClosed;
        PlayerCardsUnequippedUI.OnPlayerCardsUnequippedUIClosed += PlayerCardsUnequippedUI_OnPlayerCardsUnequippedUIClosed;
        PlayerCardUI.OnEquippedCardPress += PlayerCardUI_OnEquippedCardPress;
        PlayerCardUI.OnUnquippedCardPress += PlayerCardUI_OnUnquippedCardPress;

        HideInstantly();
    }

    private void OnDestroy()
    {
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        PlayerCardsEquippedUI.OnPlayerCardsEquippedUIClosed -= PlayerCardsEquippedUI_OnPlayerCardsEquippedUIClosed;
        PlayerCardsUnequippedUI.OnPlayerCardsUnequippedUIClosed -= PlayerCardsUnequippedUI_OnPlayerCardsUnequippedUIClosed;
        PlayerCardUI.OnEquippedCardPress -= PlayerCardUI_OnEquippedCardPress;
        PlayerCardUI.OnUnquippedCardPress -= PlayerCardUI_OnUnquippedCardPress;
    }

    private void PlayerCardsEquippedUI_OnPlayerCardsEquippedUIClosed()
    {
        equippedCard = null;
        equippedPlayerCardUI = null;
    }

    private void PlayerCardsUnequippedUI_OnPlayerCardsUnequippedUIClosed()
    {
        unequippedCard = null;
        unequippedPlayerCardUI = null;
    }

    private void PlayerCardUI_OnEquippedCardPress(PlayerCardUI obj)
    {
        equippedCard = Player.LocalInstance.EquippedCards[obj.Index];
        equippedPlayerCardUI = obj;

        OnConditionMet();
    }

    private void PlayerCardUI_OnUnquippedCardPress(PlayerCardUI obj)
    {
        unequippedCard = Player.LocalInstance.UnequippedCards[obj.Index];
        unequippedPlayerCardUI = obj;

        OnConditionMet();
    }

    private void OnConditionMet()
    {    
        if (equippedPlayerCardUI != null && unequippedPlayerCardUI != null)
        {
            ShowWithAnimation();
        }
    }

    private void Show()
    {
        titleText.text = 
            $"ARE YOU SURE YOU WANT TO SWAP\n" +
            $"{(equippedCard != null ? equippedCard.Name : "EMPTY SLOT")}\n" +
            $"FOR\n" +
            $"{unequippedCard.Name}?";

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowWithAnimation()
    {
        Show();
        ConfirmDialogUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    public void HideWithAnimation()
    {
        ConfirmDialogUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    public void HideInstantly()
    {
        ConfirmDialogUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }
}

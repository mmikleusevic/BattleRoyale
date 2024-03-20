using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCardUI : MonoBehaviour
{
    public static event Action<PlayerCardUI> OnEquippedCardPress;
    public static event Action<PlayerCardUI> OnUnquippedCardPress;

    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite defaultSprite;

    private EventTrigger eventTrigger;
    public int Index { get; set; } = -1;

    public bool isEmpty = true;

    private void Awake()
    {
        PlayerCardsEquippedUI.OnPreturnCardsInstantiated += PlayerCardsEquippedUI_OnPreturnCardsInstantiated;
        PlayerPreturn.OnPlayerPreturnOver += PlayerPreturn_OnPlayerPreturnOver;
        ConfirmSwapCardDialogUI.OnYesPressed += ConfirmSwapDialogUI_OnYesPressed;

        Hide();
    }

    private void OnDestroy()
    {
        PlayerCardsEquippedUI.OnPreturnCardsInstantiated -= PlayerCardsEquippedUI_OnPreturnCardsInstantiated;
        PlayerPreturn.OnPlayerPreturnOver -= PlayerPreturn_OnPlayerPreturnOver;
        ConfirmSwapCardDialogUI.OnYesPressed -= ConfirmSwapDialogUI_OnYesPressed;
    }

    private void PlayerCardsEquippedUI_OnPreturnCardsInstantiated()
    {
        if (eventTrigger != null && Player.LocalInstance.UnequippedCards.Count > 0)
        {
            EnableTrigger();
        }
    }

    private void PlayerPreturn_OnPlayerPreturnOver(object sender, EventArgs e)
    {
        if (eventTrigger == null) return;

        eventTrigger.enabled = false;
    }

    private void ConfirmSwapDialogUI_OnYesPressed(PlayerCardUI arg1, PlayerCardUI arg2)
    {
        if (eventTrigger == null) return;

        eventTrigger.enabled = false;
    }

    public void Instantiate(Card card, int index)
    {
        Show();

        eventTrigger = GetComponent<EventTrigger>();
        eventTrigger.enabled = false;

        cardImage.sprite = card.Sprite;
        cardImage.preserveAspect = true;
        isEmpty = false;
        Index = index;
    }

    public void Instantiate(int index)
    {
        Show();

        eventTrigger = GetComponent<EventTrigger>();
        eventTrigger.enabled = false;

        cardImage.sprite = defaultSprite;
        cardImage.preserveAspect = true;
        Index = index;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void EnableTrigger()
    {
        eventTrigger.enabled = true;
    }

    public void OnPointerDownEquippedCards()
    {
        if (Player.LocalInstance.UnequippedCards.Count > 0)
        {
            OnEquippedCardPress?.Invoke(this);
        }
    }

    public void OnPointerDownUnequippedCards()
    {
        OnUnquippedCardPress?.Invoke(this);
    }
}

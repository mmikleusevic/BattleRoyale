using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardUI : MonoBehaviour
{
    public static event Action<PlayerCardUI> OnEquippedCardPress;
    public static event Action<PlayerCardUI> OnUnquippedCardPress;

    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite defaultSprite;

    public int Index { get; set; } = -1;

    private Button button;

    public bool isEmpty = true;

    private void Awake()
    {
        PlayerCardsEquippedUI.OnPreturnCardsInstantiated += PlayerCardsEquippedUI_OnPreturnCardsInstantiated;
        PlayerPreturn.OnPlayerPreturnOver += PlayerPreturn_OnPlayerPreturnOver;
        Player.OnCardsSwapped += Player_OnCardsSwapped;

        Hide();
    }

    private void OnDestroy()
    {
        PlayerCardsEquippedUI.OnPreturnCardsInstantiated -= PlayerCardsEquippedUI_OnPreturnCardsInstantiated;
        PlayerPreturn.OnPlayerPreturnOver -= PlayerPreturn_OnPlayerPreturnOver;
        Player.OnCardsSwapped -= Player_OnCardsSwapped;
    }

    private void PlayerCardsEquippedUI_OnPreturnCardsInstantiated()
    {
        if (button != null && Player.LocalInstance.UnequippedCards.Count > 0)
        {
            button.interactable = true;
        }
    }

    private void PlayerPreturn_OnPlayerPreturnOver()
    {
        if (button == null) return;

        button.interactable = false;
    }

    private void Player_OnCardsSwapped()
    {
        if (button == null) return;

        if (Player.LocalInstance.UnequippedCards.Count > 0 && Player.LocalInstance.EquippedCards.Count < Player.LocalInstance.MaxEquippableCards)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    public void Instantiate(Card card, int index)
    {
        Show();

        GetButton(false);

        cardImage.sprite = card.Sprite;
        cardImage.preserveAspect = true;
        isEmpty = false;
        Index = index;
    }

    public void Instantiate(int index)
    {
        Show();

        GetButton(false);

        cardImage.sprite = defaultSprite;
        cardImage.preserveAspect = true;
        Index = index;
    }

    public void GetButton(bool value)
    {
        button = GetComponent<Button>();
        button.interactable = value;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnButtonPressEquippedCards()
    {
        if (Player.LocalInstance.UnequippedCards.Count > 0)
        {
            OnEquippedCardPress?.Invoke(this);
        }
    }

    public void OnButtonPressUnequippedCards()
    {
        OnUnquippedCardPress?.Invoke(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class PlayerCardUI : MonoBehaviour
{
    public static event Action<PlayerCardUI> OnEquippedCardSwap;
    public static event Action<PlayerCardUI> OnUnquippedCardSwap;
    public static event Action OnPrebattleOver;

    public static Player enemy;
    public static EquippedCardState equippedCardState = EquippedCardState.None;

    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite defaultSprite;

    public int Index { get; set; } = -1;

    private Button button;
    private Card card;

    private Color originalColor = new Color(1f, 1f, 1f, 1f);
    public Color greyedOutColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public bool isEmpty = true;


    private void Awake()
    {
        Player.OnCardsSwapped += Player_OnCardsSwapped;

        Hide();
    }

    private void OnDestroy()
    {
        Player.OnCardsSwapped -= Player_OnCardsSwapped;
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
        this.card = card;

        cardImage.sprite = card.Sprite;
        cardImage.color = originalColor;
        cardImage.preserveAspect = true;
        isEmpty = false;
        Index = index;

        GetButton();
    }

    public void Instantiate(int index)
    {
        cardImage.sprite = defaultSprite;
        cardImage.preserveAspect = true;
        Index = index;

        if (equippedCardState == EquippedCardState.Disarm)
        {
            button = GetComponent<Button>();
            button.interactable = false;
        }
        else
        {
            GetButton();
        }
    }

    private void GetButton()
    {
        button = GetComponent<Button>();

        switch (equippedCardState)
        {
            case EquippedCardState.None:
                button.interactable = false;
                break;
            case EquippedCardState.Disarm:
            case EquippedCardState.Swap:
            case EquippedCardState.Equip:
                button.interactable = true;
                break;
        }
    }

    public void DisableButton()
    {
        button.interactable = false;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnButtonPressEquippedCards()
    {
        Player player = Player.LocalInstance;

        if (player.UnequippedCards.Count > 0 && equippedCardState == EquippedCardState.Swap)
        {
            OnEquippedCardSwap?.Invoke(this);
        }
        else if (equippedCardState == EquippedCardState.Disarm)
        {
            List<Card> disarmCards = player.EquippedCards
                                           .Where(a => a.Ability is IDisarm && !a.AbilityUsed)
                                           .ToList();

            Card disarmCard = disarmCards.FirstOrDefault();

            if (disarmCard != null)
            {
                IDisarm disarmAbility = disarmCard.Ability as IDisarm;

                disarmAbility.Use(card);
                disarmCards.Remove(disarmCard);

                cardImage.color = greyedOutColor;
                button.interactable = false;

                string[] messages = CreateOnDisarmMessageUI();

                MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
                FadeMessageUI.Instance.StartFadeMessage(messages[0]);
            }

            if (disarmCards.Count == 0 || disarmCards.Count >= enemy.EquippedCards.Count)
            {
                OnPrebattleOver?.Invoke();
            }
        }
        else if (equippedCardState == EquippedCardState.Equip)
        {

        }
    }

    public void OnButtonPressUnequippedCards()
    {
        if (equippedCardState == EquippedCardState.Swap)
        {
            OnUnquippedCardSwap?.Invoke(this);
        }
    }

    private string[] CreateOnDisarmMessageUI()
    {
        Player player = Player.LocalInstance;

        return new string[]
        {
            $"YOU DISARMED {enemy.PlayerName}'s {card.Name}",
            $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color> DISARMED <color=#{enemy.HexPlayerColor}>{enemy.PlayerName}'s </color>{card.Name}"
        };
    }

    public static void ResetStaticData()
    {
        OnEquippedCardSwap = null;
        OnUnquippedCardSwap = null;
        OnPrebattleOver = null;
        enemy = null;
    }
}

public enum EquippedCardState
{
    None,
    Swap,
    Disarm,
    Equip
}

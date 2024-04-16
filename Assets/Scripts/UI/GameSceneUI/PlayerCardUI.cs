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
    public static event Action<Card, Card, Player, Player> OnCurseEquipped;
    public static event Action OnPrebattleOver;

    public static Player enemy;
    public static Card wonCard;
    public static EquippedCardState equippedCardState = EquippedCardState.None;

    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite defaultSprite;

    public int Index { get; set; } = -1;

    private Button button;
    private Card equippedCard;

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
        equippedCard = card;

        cardImage.sprite = card.Sprite;
        cardImage.preserveAspect = true;

        GetImageColor();
        GetButton();

        isEmpty = false;
        Index = index;
    }

    private void GetImageColor()
    {
        if (equippedCard != null && equippedCard.Ability != null && equippedCard.Ability is IDisarm && equippedCardState == EquippedCardState.Disarm
            || equippedCard != null && equippedCard.Ability != null && equippedCard.Ability is ICurse && (equippedCardState == EquippedCardState.Equip || equippedCardState == EquippedCardState.Swap))
        {
            cardImage.color = greyedOutColor;
        }
        else
        {
            cardImage.color = originalColor;
        }
    }

    public void Instantiate(int index)
    {
        cardImage.sprite = defaultSprite;
        cardImage.preserveAspect = true;
        Index = index;

        GetImageColor();
        GetButton();
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
                if (equippedCard != null && equippedCard.Ability != null && equippedCard.Ability is IDisarm || equippedCard == null)
                {
                    button.interactable = false;
                }
                else
                {
                    button.interactable = true;
                }
                break;
            case EquippedCardState.Swap:
                if (equippedCard != null && equippedCard.Ability != null && equippedCard.Ability is ICurse)
                {
                    button.interactable = false;
                }
                else
                {
                    button.interactable = true;
                }
                break;
            case EquippedCardState.Equip:
                if (equippedCard != null && equippedCard.Ability != null && equippedCard.Ability is ICurse)
                {
                    button.interactable = false;
                }
                else
                {
                    button.interactable = true;
                }
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

                disarmAbility.Use(equippedCard);
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
            OnCurseEquipped?.Invoke(wonCard, equippedCard, Player.LocalInstance, enemy);
            cardImage.sprite = wonCard.Sprite;
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
            $"YOU DISARMED {enemy.PlayerName}'s {equippedCard.Name}",
            $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color> DISARMED <color=#{enemy.HexPlayerColor}>{enemy.PlayerName}'s </color>{equippedCard.Name}"
        };
    }

    public static void ResetStaticData()
    {
        OnEquippedCardSwap = null;
        OnUnquippedCardSwap = null;
        OnCurseEquipped = null;
        OnPrebattleOver = null;
        enemy = null;
        wonCard = null;
    }
}

public enum EquippedCardState
{
    None,
    Swap,
    Disarm,
    Equip
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : MonoBehaviour
{
    public static event Action<Card> OnMove;
    public static event Action<Card> OnAttackCard;
    public static event Action<Card> OnAttackPlayer;

    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackCardButton;
    [SerializeField] private Button attackPlayerButton;

    private Card card;

    private void Awake()
    {
        moveButton.onClick.AddListener(() =>
        {
            OnMove?.Invoke(card);
        });

        attackCardButton.onClick.AddListener(() =>
        {
            RollType.rollType = RollTypeEnum.CardAttack;
            OnAttackCard?.Invoke(card);
        });

        attackPlayerButton.onClick.AddListener(() =>
        {
            OnAttackPlayer?.Invoke(card);
        });

        Card.OnCardPressed += Card_OnCardPressed;
    }

    private void OnDestroy()
    {
        Card.OnCardPressed -= Card_OnCardPressed;
        moveButton.onClick.RemoveAllListeners();
        attackCardButton.onClick.RemoveAllListeners();
        attackPlayerButton.onClick.RemoveAllListeners();
    }

    private void Card_OnCardPressed(object sender, Player player)
    {
        card = sender as Card;

        if (card != null && card.Interactable)
        {
            bool isPlayerOnCard = player.GridPosition == card.GridPosition;
            bool canMoveOrUseAction = player.Movement > 0 || player.ActionPoints > 0;

            if (!isPlayerOnCard && canMoveOrUseAction)
            {
                ShowMoveButton();
            }
            else
            {
                HideMoveButton();
            }

            if (isPlayerOnCard && player.ActionPoints > 0 && !card.IsClosed)
            {
                ShowAttackCardButton();
            }
            else
            {
                HideAttackCardButton();
            }

            if (isPlayerOnCard && player.ActionPoints > 0 && card.AreMultiplePeopleOnTheCard() && !card.IsClosed)
            {
                ShowAttackPlayerButton();
            }
            else
            {
                HideAttackPlayerButton();
            }
        }
        else
        {
            HideAll();
        }
    }

    private void HideAll()
    {
        HideMoveButton();
        HideAttackCardButton();
        HideAttackPlayerButton();
    }

    private void ShowMoveButton()
    {
        moveButton.gameObject.SetActive(true);
    }

    private void HideMoveButton()
    {
        moveButton.gameObject.SetActive(false);
    }

    private void ShowAttackCardButton()
    {
        attackCardButton.gameObject.SetActive(true);
    }

    private void HideAttackCardButton()
    {
        attackCardButton.gameObject.SetActive(false);
    }

    private void ShowAttackPlayerButton()
    {
        attackPlayerButton.gameObject.SetActive(true);
    }

    private void HideAttackPlayerButton()
    {
        attackPlayerButton.gameObject.SetActive(false);
    }
}

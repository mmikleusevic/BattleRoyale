using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilities : MonoBehaviour
{
    public static event Action OnRerollCardBattle;
    public static event Action OnRerollPlayerBattle;

    private bool reroll = false;
    private bool accept = false;

    private void Awake()
    {
        RollUI.OnReroll += RollUI_OnReroll;
        RollUI.OnAccept += RollUI_OnAccept;
    }

    private void OnDestroy()
    {
        RollUI.OnReroll -= RollUI_OnReroll;
        RollUI.OnAccept -= RollUI_OnAccept;
    }

    private void RollUI_OnAccept()
    {
        accept = true;
    }

    private void RollUI_OnReroll()
    {
        reroll = true;
    }

    public IEnumerator UseCardAbilitiesOnCardRoll(List<int> resultList, List<int> diceToReroll)
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            IHandleCardResult handleCardResult = card as IHandleCardResult;

            if (handleCardResult != null)
            {
                yield return StartCoroutine(handleCardResult.HandleResults(resultList, diceToReroll));
            }
        }
    }

    public IEnumerator UseCardAbilitiesOnPlayerRoll(int result, List<int> diceToReroll)
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            IHandlePlayerResult handlePlayerResult = card as IHandlePlayerResult;

            if (handlePlayerResult != null)
            {
                yield return StartCoroutine(handlePlayerResult.HandleResults(result, diceToReroll));
            }
        }
    }

    public int GetCardRollModifier()
    {
        int modifier = 0;

        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            modifier += card.GetCardRollModifier();
        }

        return modifier;
    }

    public IEnumerator GetCardRerolls()
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            ICardReroll cardRerollAbility = card.Ability as ICardReroll;

            if (cardRerollAbility != null && !card.AbilityUsed)
            {
                bool isExecuted = false;

                yield return new WaitUntil(() =>
                {
                    if (!isExecuted)
                    {
                        isExecuted = true;

                        OnRerollCardBattle?.Invoke();
                    }

                    return accept || reroll;
                });

                if (reroll)
                {
                    cardRerollAbility.Use();
                    reroll = false;
                }

                accept = false;
            }
        }
    }

    public IEnumerator GetPlayerRerolls()
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            IPlayerReroll playerRerollAbility = card.Ability as IPlayerReroll;

            if (playerRerollAbility != null && !card.AbilityUsed)
            {
                bool isExecuted = false;

                yield return new WaitUntil(() =>
                {
                    if (!isExecuted)
                    {
                        isExecuted = true;

                        OnRerollPlayerBattle?.Invoke();
                    }

                    return accept || reroll;
                });

                if (reroll)
                {
                    playerRerollAbility.Use();
                    reroll = false;
                }

                accept = false;
            }
        }
    }

    public static void ResetRerolls()
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            if (card.Ability != null)
            {
                Type abilityType = card.Ability.GetType();
                Type[] implementedInterfaces = abilityType.GetInterfaces();

                bool isNotOnlyIAbility = implementedInterfaces.Length > 1;

                if (isNotOnlyIAbility)
                {
                    card.AbilityUsed = false;
                    continue;
                }
            }
        }
    }

    public int GetPlayerRollModifier(int result)
    {
        int modifier = 0;

        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            modifier += card.GetPlayerRollModifier(result);
        }

        return modifier;
    }
}

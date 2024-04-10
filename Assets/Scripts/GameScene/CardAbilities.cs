using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilities : MonoBehaviour
{
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

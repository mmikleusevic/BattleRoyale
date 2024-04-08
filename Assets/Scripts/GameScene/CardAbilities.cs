using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilities : MonoBehaviour
{
    public IEnumerator UseCardAbilitiesOnCardRoll(List<int> resultList, List<int> diceToReroll)
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            yield return StartCoroutine(CardsWithRollManipulationAbilities(card, resultList, diceToReroll));
        }
    }

    public IEnumerator UseCardAbilitiesOnPlayerRoll(int result, List<int> diceToReroll)
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            yield return StartCoroutine(CardsWithRollManipulationAbilities(card, result, diceToReroll));
        }
    }

    private IEnumerator CardsWithRollManipulationAbilities(Card card, List<int> resultList, List<int> diceToReroll)
    {
        switch (card)
        {
            case DeepWounds deepWoundsCard:
                yield return StartCoroutine(deepWoundsCard.Ability(resultList, diceToReroll));
                break;
        }
    }

    private IEnumerator CardsWithRollManipulationAbilities(Card card, int result, List<int> diceToReroll)
    {
        switch (card)
        {
            case DeepWounds deepWoundsCard:
                yield return StartCoroutine(deepWoundsCard.Ability(result, diceToReroll));
                break;
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

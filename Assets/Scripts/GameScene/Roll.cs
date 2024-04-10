using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : MonoBehaviour
{
    [SerializeField] private RollResults rollResults;
    [SerializeField] private CardAbilities cardAbilities;

    public int CardBattleModifier { get; private set; } = 0;
    public int PlayerBattleModifier { get; private set; } = 0;

    private List<int> resultList;
    private List<int> diceToReroll;
    private string[] messages;

    private int resultSum = 0;

    private void Awake()
    {
        resultList = new List<int>();
        diceToReroll = new List<int>();
        messages = new string[2];
    }

    public IEnumerator OnRotate(Die[] dice, BattleType battleType)
    {
        switch (battleType)
        {
            case BattleType.Player:
                yield return StartCoroutine(HandlePlayerBattleRoll(dice, battleType));
                break;
            case BattleType.Card:
                yield return StartCoroutine(HandleCardBattleRoll(dice, battleType));
                break;
            case BattleType.Ability:
                yield return StartCoroutine(HandleAbilityCardRoll(dice));
                break;
        }
    }

    private Vector3 GetSide(int number)
    {
        Vector3 side = Vector3.zero;

        switch (number)
        {
            case 1:
                side = Vector3.right;
                break;
            case 2:
                side = Vector3.up;
                break;
            case 3:
                side = Vector3.back;
                break;
            case 4:
                side = Vector3.forward;
                break;
            case 5:
                side = Vector3.down;
                break;
            case 6:
                side = Vector3.left;
                break;
        }

        return side;
    }

    private IEnumerator HandleCardBattleRoll(Die[] dice, BattleType battleType)
    {
        yield return StartCoroutine(CardDiceRoll(dice));
        yield return StartCoroutine(cardAbilities.UseCardAbilitiesOnCardRoll(resultList, diceToReroll));

        if (diceToReroll.Count > 0)
        {
            yield return StartCoroutine(RerollDice(dice, battleType));
            yield break;
        }

        yield return StartCoroutine(cardAbilities.GetCardRerolls());

        bool rerolled = dice.Any(a => a.Reroll);

        if (rerolled) yield break;

        CardBattleModifier += cardAbilities.GetCardRollModifier();

        rollResults.SetRollResults(resultList, resultSum + CardBattleModifier);

        SendToMessageUIResetVariables(dice, resultSum);
    }

    private IEnumerator HandlePlayerBattleRoll(Die[] dice, BattleType battleType)
    {
        yield return StartCoroutine(PlayerDiceRoll(dice));
        yield return StartCoroutine(cardAbilities.UseCardAbilitiesOnPlayerRoll(resultSum, diceToReroll));

        if (diceToReroll.Count > 0)
        {
            yield return StartCoroutine(RerollDice(dice, battleType));
            yield break;
        }

        yield return StartCoroutine(cardAbilities.GetPlayerRerolls());

        for (int i = 0; i < dice.Length; i++)
        {
            if (dice[i].Reroll)
            {
                resultSum = 0;
                yield break;
            }
        }

        PlayerBattleModifier += cardAbilities.GetPlayerRollModifier(resultSum);

        rollResults.SetRollResults(resultSum + PlayerBattleModifier);

        SendToMessageUIResetVariables(dice, resultSum);
    }

    private IEnumerator HandleAbilityCardRoll(Die[] dice)
    {
        yield return StartCoroutine(AbilityDiceRoll(dice));

        rollResults.SetRollResults(resultSum);

        SendToMessageUIResetVariables(dice, resultSum);
    }

    private IEnumerator PlayerDiceRoll(Die[] dice)
    {
        int min = int.MaxValue;

        for (int i = 0; i < dice.Length; i++)
        {
            if (dice[i].Reroll == true || dice[i].Rolled == false)
            {
                int result = Random.Range(1, 7);

                resultSum += result;

                Vector3 side = GetSide(result);

                yield return StartCoroutine(RotateToFace(side, dice[i]));

                if (result < min)
                {
                    min = result;
                }

                dice[i].Rolled = true;
                dice[i].Reroll = false;
            }
        }

        if (RollType.rollType == RollTypeEnum.Disadvantage)
        {
            resultSum = min;
        }
    }

    private IEnumerator AbilityDiceRoll(Die[] dice)
    {
        for (int i = 0; i < dice.Length; i++)
        {
            if (dice[i].Reroll == true || dice[i].Rolled == false)
            {
                int result = Random.Range(1, 7);

                resultSum += result;

                Vector3 side = GetSide(result);

                yield return StartCoroutine(RotateToFace(side, dice[i]));

                dice[i].Rolled = true;
                dice[i].Reroll = false;
            }
        }
    }

    private IEnumerator CardDiceRoll(Die[] dice)
    {
        for (int i = 0; i < dice.Length; i++)
        {
            if (dice[i].Reroll == true || dice[i].Rolled == false)
            {
                int result = Random.Range(1, 7);

                if (!dice[i].Rolled)
                {
                    resultList.Add(result);
                }
                else if (dice[i].Reroll)
                {
                    resultSum -= resultList[i];
                    resultList[i] = result;
                }

                resultSum += result;

                Vector3 side = GetSide(result);

                yield return StartCoroutine(RotateToFace(side, dice[i]));

                dice[i].Rolled = true;
                dice[i].Reroll = false;
            }
        }
    }

    private IEnumerator RerollDice(Die[] dice, BattleType battleType)
    {
        for (int i = 0; i < diceToReroll.Count; i++)
        {
            if (resultList.Count == 0)
            {
                resultSum = 0;

                dice[i].Reroll = true;
            }
            else
            {
                int indexDieToReroll = diceToReroll[i];

                dice[indexDieToReroll].Reroll = true;

                resultSum -= resultList[indexDieToReroll];
            }
        }

        yield return StartCoroutine(OnRotate(dice, battleType));
    }

    private IEnumerator RotateToFace(Vector3 side, Die die)
    {
        float timer = 0.7f;
        float speed = 10f * Time.deltaTime;

        Quaternion rotation = Quaternion.LookRotation(side);

        while (timer > 0)
        {
            if (Quaternion.Angle(die.transform.rotation, rotation) < 0.01f) yield break;

            timer -= Time.deltaTime;

            die.transform.rotation = Quaternion.Slerp(die.transform.rotation, rotation, speed);

            yield return null;
        }
    }

    private void ResetDice(Die[] dice)
    {
        foreach (Die die in dice)
        {
            die.Reroll = false;
            die.Rolled = false;
        }
    }

    private void SendToMessageUIResetVariables(Die[] dice, int result)
    {
        if (resultList.Count == 3)
        {
            messages[0] = $"YOU ROLLED {result} ({CardBattleModifier}) (";
            messages[1] = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled {result} ({CardBattleModifier}) (";

            for (int i = 0; i < resultList.Count; i++)
            {
                messages[0] += $"{resultList[i]}";
                messages[1] += $"{resultList[i]}";

                if (i != resultList.Count - 1)
                {
                    messages[0] += ",";
                    messages[1] += ",";
                }
                else
                {
                    messages[0] += ")";
                    messages[1] += ")";
                }
            }
        }
        else
        {
            messages[0] = $"YOU ROLLED {result} ({PlayerBattleModifier})";
            messages[1] = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled {result} ({PlayerBattleModifier})";
        }

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);

        ResetDice(dice);

        CardBattleModifier = 0;
        PlayerBattleModifier = 0;
        resultList.Clear();
        diceToReroll.Clear();
        resultSum = 0;
    }
}

public enum BattleType
{
    Player,
    Card,
    Ability
}

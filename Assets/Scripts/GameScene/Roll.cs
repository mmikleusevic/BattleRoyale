using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : MonoBehaviour
{
    [SerializeField] private Camera diceCamera;
    [SerializeField] private RollResults rollResults;

    public int CardBattleModifier { get; private set; } = 0;
    public int PlayerBattleModifier { get; private set; } = 0;

    private List<int> resultList;
    private List<int> diceToReroll;

    private int resultSum = 0;
    private string[] messages;
    private Vector3 cameraPosition;
    private readonly float interactDistance = 0.8f;
    private float rotationTime = 2.5f;
    private float rotationSpeed = 540f;
    private int numberOfSideChanges = 12;

    private void Awake()
    {
        cameraPosition = diceCamera.transform.position;
        resultList = new List<int>();
        diceToReroll = new List<int>();
        messages = new string[2];
        diceCamera = null;
    }

    private int GetResult(Vector3 direction, Vector3 cameraPosition)
    {
        Physics.Raycast(cameraPosition, direction, out RaycastHit raycastHit, interactDistance);

        int.TryParse(raycastHit.collider.name, out int result);

        return result;
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

    public void RotateDice(Die[] dice, BattleType battleType)
    {
        ResetDice(dice);

        CardBattleModifier = 0;
        PlayerBattleModifier = 0;
        resultList.Clear();
        diceToReroll.Clear();
        resultSum = 0;

        if (battleType == BattleType.Card)
        {
            StartCoroutine(Rotate(dice, BattleType.Card));
        }
        else
        {
            StartCoroutine(Rotate(dice, BattleType.Player));
        }
    }

    private IEnumerator HandleCardBattleRoll(Die[] dice, BattleType battleType)
    {
        yield return StartCoroutine(CardDiceRoll(dice));
        yield return StartCoroutine(UseCardAbilitiesOnRoll());

        if (diceToReroll.Count > 0)
        {
            resultSum = 0;

            yield return StartCoroutine(RerollDice(dice, battleType));

            yield break;
        }

        GetCardRollModifier();

        rollResults.SetRollResults(resultList, resultSum + CardBattleModifier);

        SendToMessageUI(resultSum);
    }

    private IEnumerator HandlePlayerBattleRoll(Die[] dice, BattleType battleType)
    {
        yield return StartCoroutine(PlayerDiceRoll(dice));
        yield return StartCoroutine(UseCardAbilitiesOnRoll());

        while (diceToReroll.Count > 0)
        {
            resultSum = 0;

            yield return StartCoroutine(RerollDice(dice, battleType));

            yield break;
        }

        rollResults.SetRollResults(resultSum, RollType.rollType);

        SendToMessageUI(resultSum);
    }

    private IEnumerator Rotate(Die[] dice, BattleType battleType)
    {
        Die[] newDice = ReturnRollingDice(dice);

        Vector3[] randomAxes = new Vector3[newDice.Length];

        for (int i = 0; i < newDice.Length; i++)
        {
            randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
            newDice[i].transform.rotation = Random.rotationUniform;
        }

        float spinTimer = rotationTime;
        float rotationTimer = 0.0f;
        float numberOfSpins = rotationTime / numberOfSideChanges;

        while (spinTimer > 0)
        {
            spinTimer -= Time.deltaTime;
            rotationTimer += Time.deltaTime;

            for (int i = 0; i < newDice.Length; i++)
            {
                Quaternion targetRotation = newDice[i].transform.rotation * Quaternion.Euler(randomAxes[i] * rotationSpeed * Time.deltaTime);
                newDice[i].transform.rotation = Quaternion.RotateTowards(newDice[i].transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (rotationTimer >= numberOfSpins)
            {
                for (int i = 0; i < newDice.Length; i++)
                {
                    randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
                }

                rotationTimer = 0.0f;
            }

            yield return null;
        }

        yield return OnRotateCompleteCall(dice, battleType);
    }

    private Die[] ReturnRollingDice(Die[] dice)
    {
        Die[] newDice;

        if (diceToReroll.Count > 0)
        {
            newDice = new Die[diceToReroll.Count];

            int i = 0;

            foreach (Die die in dice)
            {
                if (die.Reroll == true)
                {
                    newDice[i] = die;
                    i++;
                }
            }
        }
        else
        {
            newDice = dice;
        }

        return newDice;
    }

    private IEnumerator PlayerDiceRoll(Die[] dice)
    {
        int min = int.MaxValue;

        for (int i = 0; i < dice.Length; i++)
        {
            Vector3 dicePosition = dice[i].transform.position;

            Vector3 direction = dicePosition - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            resultSum += result;

            Vector3 side = GetSide(result);

            yield return StartCoroutine(RotateToFace(side, dice[i]));

            if (result < min)
            {
                min = result;
            }

            dice[i].Rolled = true;
        }

        if (RollType.rollType == RollTypeEnum.Disadvantage)
        {
            resultSum = min;
        }
    }

    private IEnumerator CardDiceRoll(Die[] dice)
    {
        for (int i = 0; i < dice.Length; i++)
        {
            Vector3 dicePosition = dice[i].transform.position;

            Vector3 direction = dicePosition - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            if (!dice[i].Rolled)
            {
                resultList.Add(result);
            }
            else if (resultList[i] == 0)
            {
                resultList[i] = result;
            }

            resultSum += result;

            Vector3 side = GetSide(result);

            yield return StartCoroutine(RotateToFace(side, dice[i]));

            dice[i].Rolled = true;
            dice[i].Reroll = false;
        }
    }

    private IEnumerator RerollDice(Die[] dice, BattleType battleType)
    {
        for (int i = 0; i < diceToReroll.Count; i++)
        {
            int indexDieToReroll = diceToReroll[i];

            dice[indexDieToReroll].Reroll = true;

            resultList[indexDieToReroll] = 0;
        }

        yield return StartCoroutine(Rotate(dice, battleType));
    }

    private IEnumerator OnRotateCompleteCall(Die[] dice, BattleType battleType)
    {
        switch (battleType)
        {
            case BattleType.Player:
                yield return StartCoroutine(HandlePlayerBattleRoll(dice, battleType));
                break;
            case BattleType.Card:
                yield return StartCoroutine(HandleCardBattleRoll(dice, battleType));
                break;
        }
    }

    private IEnumerator RotateToFace(Vector3 side, Die die)
    {
        float timer = 0.3f;
        float speed = 20f * Time.deltaTime;

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

    private void SendToMessageUI(int result)
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
    }

    private IEnumerator UseCardAbilitiesOnRoll()
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            yield return StartCoroutine(CardsWithRollManipulationAbilities(card));
        }
    }

    private void GetCardRollModifier()
    {
        foreach (Card card in Player.LocalInstance.EquippedCards)
        {
            CardBattleModifier += card.CardRollModifier;
        }
    }

    private IEnumerator CardsWithRollManipulationAbilities(Card card)
    {
        switch (card)
        {
            case DeepWounds deepWoundsCard:
                yield return StartCoroutine(deepWoundsCard.Ability(resultList, diceToReroll));
                break;
        }
    }
}

public enum BattleType
{
    Player,
    Card
}

using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : MonoBehaviour
{
    [SerializeField] private Camera diceCamera;
    private Vector3 cameraPosition;

    private void Awake()
    {
        cameraPosition = diceCamera.transform.position;
        diceCamera = null;
    }

    [SerializeField] private RollResults rollResults;

    private readonly float interactDistance = 0.55f;

    private float rotationTime = 3f;
    private float turnToSideTime = 1f;
    private int numberOfSideChanges = 6;

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

    public void RotateDice(GameObject[] dice, BattleType battleType)
    {
        if (battleType == BattleType.Card)
        {
            Rotate(dice, BattleType.Card);
        }
        else
        {
            Rotate(dice, BattleType.Player);
        }
    }

    private void CalculateCardBattleRoll(GameObject[] dice)
    {
        List<int> resultList = new List<int>();
        int resultSum = 0;

        foreach (GameObject die in dice)
        {
            Vector3 dicePosition = die.transform.position;

            Vector3 direction = dicePosition - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            if (RollType.rollType == RollTypeEnum.CardAttack)
            {
                resultList.Add(result);
            }

            resultSum += result;

            Vector3 side = GetSide(result);

            RotateToFace(side, die);
        }

        rollResults.SetRollResults(resultList);

        bool isThreeOfAKind = resultList.Distinct().Count() == 1;

        if (isThreeOfAKind)
        {
            SendThreeOfAKindMessageToMessageUI();
        }

        SendToMessageUI(resultSum);
    }

    private void CalculatePlayerBattleRoll(GameObject[] dice)
    {
        int resultSum = 0;
        int min = int.MaxValue;

        for (int i = 0; i < dice.Length; i++)
        {
            Vector3 dicePosition = dice[i].transform.position;

            Vector3 direction = dicePosition - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            resultSum += result;

            Vector3 side = GetSide(result);

            RotateToFace(side, dice[i]);

            if (result < min)
            {
                min = result;
            }
        }

        if (RollType.rollType == RollTypeEnum.Disadvantage)
        {
            resultSum = min;
        }

        rollResults.SetRollResults(resultSum, RollType.rollType);

        SendToMessageUI(resultSum);
    }

    private void Rotate(GameObject[] dice, BattleType battleType)
    {
        Sequence mySequence = DOTween.Sequence();

        for (int i = 0; i < dice.Length; i++)
        {
            dice[i].transform.rotation = Random.rotationUniform;

            mySequence.Join(dice[i].transform.DORotate(new Vector3(Random.value,Random.value, Random.value) * 360f, rotationTime / numberOfSideChanges)
                                               .SetEase(Ease.Linear)
                                               .SetLoops(numberOfSideChanges, LoopType.Incremental));
        }

        mySequence.Play()
                  .OnComplete(() => OnRotateCompleteCall(dice, battleType));
    }

    private void OnRotateCompleteCall(GameObject[] dice, BattleType battleType)
    {
        switch (battleType)
        {
            case BattleType.Player:
                CalculatePlayerBattleRoll(dice);
                break;
            case BattleType.Card:
                CalculateCardBattleRoll(dice);
                break;
        }
    }

    private void RotateToFace(Vector3 side, GameObject die)
    {
        Quaternion rotation = Quaternion.LookRotation(side);

        die.transform.DORotateQuaternion(rotation, turnToSideTime)
            .SetEase(Ease.Linear);
    }

    private void SendToMessageUI(int result)
    {
        string[] messages = new string[] {
            $"YOU ROLLED {result}",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled <color=#{Player.LocalInstance.HexPlayerColor}>{result}</color>"
        };

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
    }

    private void SendThreeOfAKindMessageToMessageUI()
    {
        string message = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled THREE OF A KIND";

        MessageUI.Instance.SendMessageToEveryoneExceptMe(message);
    }
}

public enum BattleType
{
    Player,
    Card
}

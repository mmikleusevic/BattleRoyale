using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : MonoBehaviour
{
    [SerializeField] private Camera diceCamera;
    private Vector3 cameraPosition;
    private List<int> resultList;
    private string[] messages;

    private void Awake()
    {
        cameraPosition = diceCamera.transform.position;
        resultList = new List<int>();
        messages = new string[2];
        diceCamera = null;
    }

    [SerializeField] private RollResults rollResults;

    private readonly float interactDistance = 0.55f;

    private float rotationTime = 3f;
    private float turnToSideTime = 1f;
    private int numberOfSideChanges = 12;
    private float rotationSpeed = 540f;

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
            StartCoroutine(Rotate(dice, BattleType.Card));
        }
        else
        {
            StartCoroutine(Rotate(dice, BattleType.Player));
        }
    }

    private void CalculateCardBattleRoll(GameObject[] dice)
    {        
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

    private IEnumerator Rotate(GameObject[] dice, BattleType battleType)
    {
        Vector3[] randomAxes = new Vector3[dice.Length];

        for (int i = 0; i < randomAxes.Length; i++)
        {
            randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
            dice[i].transform.rotation = Random.rotationUniform;
        }

        float spinTimer = rotationTime;
        float rotationTimer = 0.0f;
        float numberOfSpins = rotationTime / numberOfSideChanges;

        while (spinTimer > 0)
        {
            spinTimer -= Time.deltaTime;
            rotationTimer += Time.deltaTime;

            for (int i = 0; i < dice.Length; i++)
            {
                Quaternion targetRotation = dice[i].transform.rotation * Quaternion.Euler(randomAxes[i] * rotationSpeed * Time.deltaTime);
                dice[i].transform.rotation = Quaternion.RotateTowards(dice[i].transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (rotationTimer >= numberOfSpins)
            {
                for (int i = 0; i < dice.Length; i++)
                {
                    randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
                }

                rotationTimer = 0.0f;
            }

            yield return null;
        }

        OnRotateCompleteCall(dice, battleType);
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
        if (resultList.Count == 3)
        {
            messages[0] = $"YOU ROLLED {result} ("; 
            messages[1] = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled <color=#{Player.LocalInstance.HexPlayerColor}>{result} (</color>";

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
            messages[0] = $"YOU ROLLED {result}";
            messages[1] = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled <color=#{Player.LocalInstance.HexPlayerColor}>{result}</color>";      
        }

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        resultList.Clear();
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : MonoBehaviour, IRoll
{
    public static event EventHandler<OnRollEventArgs> OnRoll;

    public class OnRollEventArgs : EventArgs
    {
        public string message;
        public int rollValue;
    }

    [SerializeField] private RollResults rollResults;

    private readonly float interactDistance = 0.51f;

    private float rotationTime = 3f;
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

    public IEnumerator RotateDice(GameObject[] dice, Vector3[] dicePositions, Vector3 cameraPosition)
    {
        List<int> resultList = new List<int>();
        int resultSum = 0;
        int min = int.MaxValue;

        Vector3[] randomAxes = new Vector3[dice.Length];

        for (int i = 0; i < randomAxes.Length; i++)
        {
            randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
            dice[i].transform.rotation = Random.rotationUniform;
        }

        float spinTimer = rotationTime;
        float rotationTimer = 0.0f;

        while (spinTimer > 0)
        {
            spinTimer -= Time.deltaTime;
            rotationTimer += Time.deltaTime;

            for (int i = 0; i < dice.Length; i++)
            {
                Quaternion targetRotation = dice[i].transform.rotation * Quaternion.Euler(randomAxes[i] * rotationSpeed * Time.deltaTime);
                dice[i].transform.rotation = Quaternion.RotateTowards(dice[i].transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (rotationTimer >= 0.25f)
            {
                for (int i = 0; i < dice.Length; i++)
                {
                    randomAxes[i] = new Vector3(Random.value, Random.value, Random.value).normalized;
                }

                rotationTimer = 0.0f;
            }

            yield return null;
        }

        for (int i = 0; i < dice.Length; i++)
        {
            Vector3 direction = dicePositions[i] - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            if (RollType.rollType == RollTypeEnum.CardAttack)
            {
                resultList.Add(result);
            }

            resultSum += result;

            Vector3 side = GetSide(result);

            StartCoroutine(RotateToFace(side, dice[i]));

            if (result < min)
            {
                min = result;
            }
        }

        if (RollType.rollType == RollTypeEnum.Disadvantage)
        {
            resultSum = min;
        }

        if (RollType.rollType == RollTypeEnum.CardAttack)
        {
            rollResults.SetRollResults(resultList);

            bool isThreeOfAKind = resultList.Distinct().Count() == 1;

            if (isThreeOfAKind)
            {
                SendThreeOfAKindMessageToMessageUI(resultSum);
            }
        }
        else
        {
            rollResults.SetRollResults(resultSum, RollType.rollType);
        }

        SendToMessageUI(resultSum);
    }

    private IEnumerator RotateToFace(Vector3 side, GameObject die)
    {
        float timer = 1f;
        float speed = 10f * Time.deltaTime;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            Quaternion rotation = Quaternion.LookRotation(side);

            die.transform.rotation = Quaternion.Slerp(die.transform.rotation, rotation, speed);
            yield return null;
        }
    }

    private void SendToMessageUI(int result)
    {
        string message = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled <color=#{Player.LocalInstance.HexPlayerColor}>{result}</color>";

        OnRollEventArgs eventArgs = new OnRollEventArgs
        {
            message = message,
            rollValue = result
        };

        OnRoll?.Invoke(this, eventArgs);
    }

    private void SendThreeOfAKindMessageToMessageUI(int result)
    {
        string message = $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName}</color> rolled THREE OF A KIND";

        OnRollEventArgs eventArgs = new OnRollEventArgs
        {
            message = message,
            rollValue = result
        };

        OnRoll?.Invoke(this, eventArgs);
    }
}

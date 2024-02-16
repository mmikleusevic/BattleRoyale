using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Roll : IRoll
{
    public static event EventHandler<OnRollEventArgs> OnRoll;

    public class OnRollEventArgs : EventArgs
    {
        public string message;
        public int rollValue;
    }

    private readonly float interactDistance = 0.51f;

    private float rotationTime = 3f;
    private float rotationSpeed = 540f;

    private IRollResults rollResults;

    public Roll()
    {
        rollResults = UnityEngine.Object.FindFirstObjectByType<RollResults>();
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

    public IEnumerator RotateDice(GameObject[] dice, Vector3[] dicePositions, Vector3 cameraPosition)
    {
        int resultSum = 0;

        for (int i = 0; i < dice.Length; i++)
        {
            dice[i].transform.rotation = Random.rotationUniform;

            Vector3 randomAxis = new Vector3(Random.value, Random.value, Random.value).normalized;
            float spinTimer = rotationTime;
            float rotationTimer = 0.0f;

            while (spinTimer > 0)
            {               
                spinTimer -= Time.deltaTime;
                rotationTimer += Time.deltaTime;

                Quaternion targetRotation = dice[i].transform.rotation * Quaternion.Euler(randomAxis * rotationSpeed * Time.deltaTime);
                dice[i].transform.rotation = Quaternion.RotateTowards(dice[i].transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if(rotationTimer >= 0.25f)
                {
                    randomAxis = new Vector3(Random.value, Random.value, Random.value).normalized;
                    rotationTimer = 0.0f;
                }

                yield return null;
            }

            // ---------------------------------

            Vector3 direction = dicePositions[i] - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            resultSum += result;

            Vector3 side = GetSide(result);

            float timer = 1f;
            float speed = 10f * Time.deltaTime;

            // Rotate sides face towards camera

            while (timer > 0)
            {
                timer -= Time.deltaTime;

                Quaternion rotation = Quaternion.LookRotation(side);

                dice[i].transform.rotation = Quaternion.Slerp(dice[i].transform.rotation, rotation, speed);

                yield return null;
            }
        }

        rollResults.SetRollResults(resultSum);
        SendToMessageUI(resultSum);
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
}

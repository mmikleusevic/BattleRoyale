using System;
using System.Collections;
using Unity.VisualScripting;
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

    private readonly float rotationSpeed = 150f;
    private float maxRotationTime = 3f;
    private float rotationTime;
    private readonly float factor;

    private IRollResults rollResults;

    public Roll()
    {
        factor = rotationSpeed * Time.deltaTime;

        rotationTime = maxRotationTime;

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
            dice[i].transform.rotation = Random.rotation;

            float xAxis = Random.Range(1.5f, 3f) * factor;
            float yAxis = Random.Range(1.5f, 3f) * factor;
            float zAxis = Random.Range(1.5f, 3f) * factor;

            Vector3 xAxisVector = new Vector3(xAxis, 0, 0);
            Vector3 yAxisVector = new Vector3(0, yAxis, 0);
            Vector3 zAxisVector = new Vector3(0, 0, zAxis);

            Vector3[] axis = new Vector3[] { xAxisVector, yAxisVector, zAxisVector };

            // Roll the dice in random direction

            float xRotationPerSecond = xAxis / maxRotationTime;
            float yRotationPerSecond = yAxis / maxRotationTime;
            float zRotationPerSecond = zAxis / maxRotationTime;

            Vector3 xRotationPerSecondVector = new Vector3(xRotationPerSecond, 0, 0);
            Vector3 yRotationPerSecondVector = new Vector3(0, yRotationPerSecond, 0);
            Vector3 zRotationPerSecondVector = new Vector3(0, 0, zRotationPerSecond);

            Vector3[] rotationPerSecond = new Vector3[] { xRotationPerSecondVector, yRotationPerSecondVector, zRotationPerSecondVector };

            float axisRotationTime = rotationTime / axis.Length;

            while (rotationTime > 0)
            {
                for (int j = 0; j < axis.Length; j++)
                {
                    float axisRotationTimeMax = axisRotationTime;

                    while (axisRotationTime > 0)
                    {
                        axisRotationTime -= Time.deltaTime;
                        rotationTime -= Time.deltaTime;

                        axis[j] -= rotationPerSecond[j] * Time.deltaTime;

                        dice[i].transform.Rotate(axis[j]);

                        yield return null;
                    }

                    axisRotationTime = axisRotationTimeMax;
                }
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

            // -------------------------

            rotationTime = maxRotationTime;
        }

        rollResults.SetRollResults(resultSum);
        SendToMessageUI(resultSum);
    }

    private void SendToMessageUI(int result)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerData();
        string colorString = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId).ToHexString();

        string message = $"<color=#{colorString}>{playerData.playerName}</color> rolled <color=#{colorString}>{result}</color>";

        OnRollEventArgs eventArgs = new OnRollEventArgs
        {
            message = message,
            rollValue = result
        };

        OnRoll?.Invoke(this, eventArgs);
    }
}

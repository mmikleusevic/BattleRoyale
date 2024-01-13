using System.Collections;
using UnityEngine;

public class Roll : IRoll
{
    private float interactDistance = 0.51f;

    protected float rotationSpeed = 150f;
    protected float rotationTime = 2f;
    protected float factor;

    public Roll()
    {
        factor = rotationSpeed * Time.deltaTime;
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

            float xAxis = Random.Range(0.8f, 1.2f) * factor;
            float yAxis = Random.Range(0.8f, 1.2f) * factor;
            float zAxis = Random.Range(0.8f, 1.2f) * factor;

            // Roll the dice in random direction

            while (rotationTime > 0)
            {
                rotationTime -= Time.deltaTime;

                dice[i].transform.Rotate(xAxis, yAxis, zAxis);

                yield return null;
            }

            // ---------------------------------

            Vector3 direction = dicePositions[i] - cameraPosition;

            int result = GetResult(direction, cameraPosition);

            resultSum += result;

            Vector3 side = GetSide(result);

            float timer = 1f;
            float speed = 10f * Time.deltaTime;

            // Rotate towards side face

            while (timer > 0)
            {
                timer -= Time.deltaTime;

                Quaternion rotation = Quaternion.LookRotation(side);

                dice[i].transform.rotation = Quaternion.Slerp(dice[i].transform.rotation, rotation, speed);

                yield return null;
            }

            // -------------------------

            rotationTime = 3f;
        }

        GameManager.Instance.SetRollResults(resultSum);

        //TODO remove
        Debug.Log(resultSum);

        yield return resultSum;
    }
}

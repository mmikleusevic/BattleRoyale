using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollUI : MonoBehaviour
{
    [SerializeField] Button rollThreeButton;
    [SerializeField] Button rollSingleButton;
    [SerializeField] List<GameObject> dice;
    [SerializeField] Camera threeDiceCamera;
    [SerializeField] Camera singleDieCamera;

    Vector3 threeDiceCameraPosition;
    Vector3 singleDiceCameraPosition;
    private Vector3[] positions;
    private float interactDistance = 0.51f;
    private float rotationSpeed = 150f;
    private float rotationTime = 2f;
    private float factor;

    private void Awake()
    {
        rollThreeButton.onClick.AddListener(() =>
        {
            StartCoroutine(RotateDice());
            //rollThreeButton.gameObject.SetActive(false);
        });

        rollSingleButton.onClick.AddListener(() =>
        {
            StartCoroutine(RotateDie());
            //rollSingleButton.gameObject.SetActive(false);
        });

        positions = new Vector3[4];
    }

    private void Start()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            Vector3 position = dice[i].transform.position;

            positions[i] = position;
        }

        threeDiceCameraPosition = threeDiceCamera.transform.position;
        singleDiceCameraPosition = singleDieCamera.transform.position;

        factor = rotationSpeed * Time.deltaTime;
    }

    private IEnumerator RotateDice()
    {
        int result = 0;

        for (int i = 0; i < dice.Count - 1; i++)
        {
            dice[i].transform.rotation = Random.rotation;

            while (rotationTime > 0)
            {
                rotationTime -= Time.deltaTime;

                dice[i].transform.Rotate(new Vector3(Random.Range(0f, 1f) * factor, Random.Range(0f, 1f) * factor, Random.Range(0f, 1f)) * factor);

                yield return null;
            }

            Vector3 direction = positions[i] - threeDiceCameraPosition;

            result += GetSideAndRotate(direction, threeDiceCameraPosition, dice[i]);

            rotationTime = 2f;
        }

        Debug.Log(result);
    }

    private IEnumerator RotateDie()
    {
        dice[3].transform.rotation = Random.rotation;

        while (rotationTime > 0)
        {
            rotationTime -= Time.deltaTime;

            dice[3].transform.Rotate(new Vector3(Random.Range(0f, 1f) * factor, Random.Range(0f, 1f) * factor, Random.Range(0f, 1f)) * factor);

            yield return null;
        }

        Vector3 direction = positions[3] - singleDiceCameraPosition;

        int result = GetSideAndRotate(direction, singleDiceCameraPosition, dice[3]);

        Debug.Log(result);

        rotationTime = 3f;
    }


    private int GetSideAndRotate(Vector3 direction, Vector3 cameraPosition, GameObject die)
    {
        Physics.Raycast(cameraPosition, direction, out RaycastHit raycastHit, interactDistance);

        int.TryParse(raycastHit.collider.name, out int result);

        Vector3 side = GetSide(result);

        StartCoroutine(RotateToward(side, die));

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

    private IEnumerator RotateToward(Vector3 side, GameObject die)
    {
        float timer = 1f;

        while (timer > 0)
        {
            timer -= Time.deltaTime;

            Quaternion rotation = Quaternion.LookRotation(side);

            die.transform.rotation = Quaternion.Slerp(die.transform.rotation, rotation, 10f * Time.deltaTime);

            yield return null;
        }
    }
}

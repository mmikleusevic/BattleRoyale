using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreeDiceRollUI : Roll
{
    public static ThreeDiceRollUI Instance { get; private set; }

    [SerializeField] private Button rollThreeButton;
    [SerializeField] private List<GameObject> dice;
    [SerializeField] private Camera threeDiceCamera;

    private Vector3 threeDiceCameraPosition;
    private Vector3[] dicePositions;

    private void Awake()
    {
        Instance = this;

        rollThreeButton.onClick.AddListener(() =>
        {
            rollThreeButton.gameObject.SetActive(false);
            StartCoroutine(RotateDice());
        });

        dicePositions = new Vector3[3];

        Hide();
    }

    private void Start()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            Vector3 position = dice[i].transform.position;

            dicePositions[i] = position;
        }

        threeDiceCameraPosition = threeDiceCamera.transform.position;

        factor = rotationSpeed * Time.deltaTime;
    }

    private IEnumerator RotateDice()
    {
        int result = 0;

        for (int i = 0; i < dice.Count; i++)
        {
            dice[i].transform.rotation = Random.rotation;

            float xAxis = Random.Range(0.5f, 1f) * factor;
            float yAxis = Random.Range(0.5f, 1f) * factor;
            float zAxis = Random.Range(0.5f, 1f) * factor;

            while (rotationTime > 0)
            {
                rotationTime -= Time.deltaTime;

                dice[i].transform.Rotate(xAxis, yAxis, zAxis);

                yield return null;
            }

            Vector3 direction = dicePositions[i] - threeDiceCameraPosition;

            result += GetSideAndRotate(direction, threeDiceCameraPosition, dice[i]);

            rotationTime = 2f;
        }

        //TODO remove
        Debug.Log(result);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

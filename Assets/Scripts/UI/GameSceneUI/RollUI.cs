using UnityEngine;
using UnityEngine.UI;

public class RollUI : MonoBehaviour
{
    [SerializeField] private Button rollButton;
    [SerializeField] private GameObject[] dice;
    [SerializeField] private Camera diceCamera;

    private IRoll roll;

    private Vector3 cameraPosition;
    private Vector3[] dicePositions;

    public virtual void Awake()
    {
        roll = new Roll();

        rollButton.onClick.AddListener(() =>
        {
            rollButton.gameObject.SetActive(false);
            StartCoroutine(roll.RotateDice(dice, dicePositions, cameraPosition));
        });

        cameraPosition = diceCamera.transform.position;

        AssignDicePosition();

        Hide();
    }

    private void AssignDicePosition()
    {
        dicePositions = new Vector3[dice.Length];

        for (int i = 0; i < dice.Length; i++)
        {
            Vector3 position = dice[i].transform.position;

            dicePositions[i] = position;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

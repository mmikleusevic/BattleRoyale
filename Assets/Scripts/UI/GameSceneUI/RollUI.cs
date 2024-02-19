using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RollUI : NetworkBehaviour
{
    [SerializeField] private GameObject backgroundImageGameObject;
    [SerializeField] private Button rollButton;
    [SerializeField] private GameObject[] dice;
    [SerializeField] private Camera diceCamera;
    [SerializeField] private Roll roll;
    private Vector3 cameraPosition;
    private Vector3[] dicePositions;

    public virtual void Awake()
    {
        rollButton.onClick.AddListener(() =>
        {
            rollButton.gameObject.SetActive(false);
            StartCoroutine(roll.RotateDice(dice, dicePositions, cameraPosition));
        });

        cameraPosition = diceCamera.transform.position;

        AssignDicePosition();
    }

    public override void OnDestroy()
    {
        rollButton.onClick.RemoveAllListeners();

        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        Hide();

        base.OnNetworkSpawn();
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
        backgroundImageGameObject.SetActive(true);
        gameObject.SetActive(true);
        rollButton.gameObject.SetActive(true);
    }

    public void Hide()
    {
        backgroundImageGameObject.SetActive(false);
        gameObject.SetActive(false);
        rollButton.gameObject.SetActive(false);
    }
}

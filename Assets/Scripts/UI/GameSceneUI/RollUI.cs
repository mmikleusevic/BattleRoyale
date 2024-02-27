using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RollUI : NetworkBehaviour
{
    [SerializeField] RectTransform rollUIRectTransform;
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

        HideInstant();
    }

    public override void OnDestroy()
    {
        rollButton.onClick.RemoveAllListeners();

        base.OnDestroy();
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

    public void ShowWithAnimation()
    {
        gameObject.SetActive(true);
        rollUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
        backgroundImageGameObject.SetActive(true);
        rollButton.gameObject.SetActive(true);
    }

    public void HideWithAnimation()
    {
        rollUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
        backgroundImageGameObject.SetActive(false);
        rollButton.gameObject.SetActive(false);
    }

    public void HideInstant()
    {
        rollUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
        backgroundImageGameObject.SetActive(false);
        rollButton.gameObject.SetActive(false);
    }
}

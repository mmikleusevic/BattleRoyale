using DG.Tweening;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class RollUI : NetworkBehaviour
{
    [SerializeField] RectTransform rollUIRectTransform;
    [SerializeField] private Button rollButton;
    [SerializeField] private GameObject[] dice;

    [SerializeField] private Roll roll;

    private BattleType battleType = BattleType.Player;
    private GameObject[] rollDice;

    public void Awake()
    {
        rollButton.onClick.AddListener(() =>
        {
            rollButton.gameObject.SetActive(false);
            roll.RotateDice(rollDice, battleType);
        });

        HideInstant();
    }

    public override void OnDestroy()
    {
        rollButton.onClick.RemoveAllListeners();

        base.OnDestroy();
    }

    private void ToggleDice(int value)
    {
        switch (value)
        {
            case 1:
                dice[1].gameObject.SetActive(true);
                rollDice = new GameObject[] { dice[1] };

                battleType = BattleType.Player;
                break;
            case 2:
                dice[0].gameObject.SetActive(true);
                dice[2].gameObject.SetActive(true);
                rollDice = new GameObject[] { dice[0], dice[2] };

                battleType = BattleType.Player;
                break;
            case 3:
                dice[0].gameObject.SetActive(true);
                dice[1].gameObject.SetActive(true);
                dice[2].gameObject.SetActive(true);
                rollDice = new GameObject[] { dice[0], dice[1], dice[2] };

                battleType = BattleType.Card;
                break;
        }
    }

    public void ShowWithAnimation(int value)
    {
        if (rollDice != null && rollDice.Length == value)
        {
            foreach (GameObject die in rollDice)
            {
                die.gameObject.SetActive(true);
            }
        }
        else
        {
            ToggleDice(value);
        }

        gameObject.SetActive(true);
        rollUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
        rollButton.gameObject.SetActive(true);
    }

    public void HideWithAnimation()
    {
        StartCoroutine(Delay());
    }

    public void HideInstant()
    {
        rollUIRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1.5f);
        rollUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        rollButton.gameObject.SetActive(false);

        DisableAllDice();
    }

    private void DisableAllDice()
    {
        foreach (var die in dice)
        {
            die.gameObject.SetActive(false);
        }
    }
}

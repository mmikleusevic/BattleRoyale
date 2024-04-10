using DG.Tweening;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class RollUI : NetworkBehaviour
{
    [SerializeField] RectTransform rollUIRectTransform;
    [SerializeField] private Button rollButton;
    [SerializeField] private Die[] dice;

    [SerializeField] private Roll roll;

    private BattleType battleType = BattleType.Player;
    private Die[] rollDice;

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

    private void ToggleDice(int value, BattleType battleType)
    {
        switch (value)
        {
            case 1:
                dice[1].gameObject.SetActive(true);
                rollDice = new Die[] { dice[1] };

                this.battleType = battleType == BattleType.Player ? BattleType.Player : BattleType.Ability;
                break;
            case 2:
                dice[0].gameObject.SetActive(true);
                dice[2].gameObject.SetActive(true);
                rollDice = new Die[] { dice[0], dice[2] };

                this.battleType = BattleType.Player;
                break;
            case 3:
                dice[0].gameObject.SetActive(true);
                dice[1].gameObject.SetActive(true);
                dice[2].gameObject.SetActive(true);
                rollDice = new Die[] { dice[0], dice[1], dice[2] };

                this.battleType = BattleType.Card;
                break;
        }
    }

    public void ShowWithAnimation(int value, BattleType battleType)
    {
        if (rollDice != null && rollDice.Length == value)
        {
            foreach (Die die in rollDice)
            {
                die.gameObject.SetActive(true);
            }
        }
        else
        {
            ToggleDice(value, battleType);
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

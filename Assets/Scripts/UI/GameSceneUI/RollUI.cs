using DG.Tweening;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class RollUI : NetworkBehaviour
{
    public static event Action OnReroll;
    public static event Action OnAccept;

    [SerializeField] RectTransform rollUIRectTransform;
    [SerializeField] private Button rollButton;
    [SerializeField] private Button rerollButton0;
    [SerializeField] private Button rerollButton1;
    [SerializeField] private Button rerollButton2;
    [SerializeField] private Button acceptRollButton;
    [SerializeField] private Die[] dice;

    [SerializeField] private Roll roll;

    private BattleType battleType = BattleType.Player;
    private Die[] rollDice;

    public void Awake()
    {
        rollButton.onClick.AddListener(() =>
        {
            rollButton.gameObject.SetActive(false);
            Roll();
        });

        rerollButton0.onClick.AddListener(() =>
        {
            rollDice[0].Reroll = true;
            OnReroll?.Invoke();
            HideRerollOrPass();
            Roll();
        });

        rerollButton1.onClick.AddListener(() =>
        {
            rollDice[1].Reroll = true;
            OnReroll?.Invoke();
            HideRerollOrPass();
            Roll();
        });

        rerollButton2.onClick.AddListener(() =>
        {
            rollDice[2].Reroll = true;
            OnReroll?.Invoke();
            HideRerollOrPass();
            Roll();
        });

        acceptRollButton.onClick.AddListener(() =>
        {
            OnAccept?.Invoke();
            HideRerollOrPass();
        });

        CardAbilities.OnTryReroll += CardAbilities_OnTryReroll;

        HideRerollOrPass();
        HideInstant();
    }

    public override void OnDestroy()
    {
        rollButton.onClick.RemoveAllListeners();
        rerollButton0.onClick.RemoveAllListeners();
        rerollButton1.onClick.RemoveAllListeners();
        rerollButton2.onClick.RemoveAllListeners();
        acceptRollButton.onClick.RemoveAllListeners();

        CardAbilities.OnTryReroll -= CardAbilities_OnTryReroll;

        base.OnDestroy();
    }

    private void Roll()
    {
        StartCoroutine(roll.OnRotate(rollDice, battleType));
    }

    private void CardAbilities_OnTryReroll()
    {
        ShowRerollOrPass();
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

    private void ShowRerollOrPass()
    {
        rerollButton0.gameObject.SetActive(true);
        rerollButton1.gameObject.SetActive(true);
        rerollButton2.gameObject.SetActive(true);
        acceptRollButton.gameObject.SetActive(true);
    }

    private void HideRerollOrPass()
    {
        rerollButton0.gameObject.SetActive(false);
        rerollButton1.gameObject.SetActive(false);
        rerollButton2.gameObject.SetActive(false);
        acceptRollButton.gameObject.SetActive(false);
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

using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ResurrectUI : MonoBehaviour
{
    public static event Action OnResurrectPressed;

    [SerializeField] RectTransform resurrectUIRectTransform;
    [SerializeField] private Button resurrectButton;

    private void Awake()
    {
        resurrectButton.onClick.AddListener(() =>
        {
            OnResurrectPressed?.Invoke();
            HideWithAnimation();
        });

        Player.OnPlayerDiedCardBattle += Player_OnPlayerDiedCardBattle;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        PlayerTurn.OnPlayerTurnOver += PlayerTurn_OnPlayerTurnOver;

        HideWithAnimation();
    }



    private void OnDestroy()
    {
        Player.OnPlayerDiedCardBattle -= Player_OnPlayerDiedCardBattle;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        PlayerTurn.OnPlayerTurnOver -= PlayerTurn_OnPlayerTurnOver;
        resurrectButton.onClick.RemoveAllListeners();
    }

    private void Player_OnPlayerDiedCardBattle()
    {
        if (Player.LocalInstance.ActionPoints > 0)
        {
            ShowWithAnimation();
        }
    }

    private void Player_OnPlayerTurnSet()
    {
        if (Player.LocalInstance.Dead)
        {
            ShowWithAnimation();
        }
    }

    private void PlayerTurn_OnPlayerTurnOver()
    {
        HideWithAnimation();
    }

    public void ShowWithAnimation()
    {
        Show();
        resurrectUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    public void HideWithAnimation()
    {
        resurrectUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}

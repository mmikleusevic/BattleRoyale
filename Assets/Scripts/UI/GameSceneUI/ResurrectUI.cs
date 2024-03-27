using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ResurrectUI : MonoBehaviour
{
    [SerializeField] RectTransform resurrectUIRectTransform;
    [SerializeField] private Button resurrectButton;
    [SerializeField] private ParticleSystem particleGlow;

    private void Awake()
    {
        resurrectButton.onClick.AddListener(() =>
        {
            Player.LocalInstance.Ressurect();
            HideWithAnimation();
        });

        Player.OnPlayerDiedCardBattle += Player_OnPlayerDiedCardBattle;
        Player.OnPlayerDiedPlayerBattle += Player_OnPlayerDiedPlayerBattle;
        Player.OnPlayerTurnSet += Player_OnPlayerTurnSet;
        Player.OnPlayerSelectedPlaceToDie += Player_OnPlayerSelectedPlaceToDie;
        PlayerTurn.OnPlayerTurnOver += PlayerTurn_OnPlayerTurnOver;

        ParticleSystemManager.Instance.Stop(particleGlow);

        HideWithAnimation();
    }

    private void OnDestroy()
    {
        Player.OnPlayerDiedCardBattle -= Player_OnPlayerDiedCardBattle;
        Player.OnPlayerDiedPlayerBattle -= Player_OnPlayerDiedPlayerBattle;
        Player.OnPlayerTurnSet -= Player_OnPlayerTurnSet;
        Player.OnPlayerSelectedPlaceToDie -= Player_OnPlayerSelectedPlaceToDie;
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

    private void Player_OnPlayerDiedPlayerBattle()
    {
        if (Player.LocalInstance == PlayerManager.Instance.ActivePlayer && Player.LocalInstance.ActionPoints > 0 && !Player.LocalInstance.PickPlaceToDie)
        {
            ShowWithAnimation();
        }
    }

    private void Player_OnPlayerTurnSet()
    {
        if (Player.LocalInstance.IsDead.Value)
        {
            ShowWithAnimation();
        }
    }

    private void Player_OnPlayerSelectedPlaceToDie(ulong obj)
    {
        if (Player.LocalInstance == PlayerManager.Instance.ActivePlayer && Player.LocalInstance.ActionPoints > 0)
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
        resurrectUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void Show()
    {
        gameObject.SetActive(true);
        ParticleSystemManager.Instance.Play(particleGlow);
    }

    private void Hide()
    {
        ParticleSystemManager.Instance.Stop(particleGlow);
        gameObject.SetActive(false);
    }
}

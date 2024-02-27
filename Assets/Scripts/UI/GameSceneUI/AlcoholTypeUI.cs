using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AlcoholTypeUI : MonoBehaviour
{
    public static event Action OnAlcoholButtonPress;

    [SerializeField] private RectTransform alcoholTypeRectTransform;
    [SerializeField] private Button strongAlcoholButton;
    [SerializeField] private Button mediumAlcoholButton;
    [SerializeField] private Button weakAlcoholButton;

    private void Awake()
    {
        strongAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(1);
            HideWithAnimation();
            OnAlcoholButtonPress?.Invoke();
        });

        mediumAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(2);
            HideWithAnimation();
            OnAlcoholButtonPress?.Invoke();
        });

        weakAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(3);
            HideWithAnimation();
            OnAlcoholButtonPress?.Invoke();
        });

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;

        Hide();
    }

    private void OnDestroy()
    {
        strongAlcoholButton.onClick.RemoveAllListeners();
        mediumAlcoholButton.onClick.RemoveAllListeners();
        weakAlcoholButton.onClick.RemoveAllListeners();

        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {
        Show();
    }

    private void SendAcoholValue(int value)
    {
        Player.LocalInstance.SetSipValue(value);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }


    private void HideWithAnimation()
    {
        alcoholTypeRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
            alcoholTypeRectTransform.DOKill(true);
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

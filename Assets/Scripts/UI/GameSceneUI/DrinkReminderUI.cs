using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkReminderUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI drinkReminderText;
    [SerializeField] private RectTransform drinkReminderRectTransform;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            HideWithAnimation();
        });

        Player.OnPlayerResurrected += Player_OnPlayerResurrected;

        HideInstant();
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveAllListeners();
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
    }

    private void Player_OnPlayerResurrected(string[] obj)
    {
        int numberOfSips = Player.LocalInstance.SipValue;

        if (numberOfSips > 1)
        {
            drinkReminderText.text = $"Drink {numberOfSips} sips";
        }
        else
        {
            drinkReminderText.text = $"Drink {numberOfSips} sip";
        }

        ShowWithAnimation();
    }

    public void ShowWithAnimation()
    {
        Show();
        drinkReminderRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    private void HideWithAnimation()
    {
        drinkReminderRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void HideInstant()
    {
        drinkReminderRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
    }
}

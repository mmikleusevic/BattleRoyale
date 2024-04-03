using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrinkReminderUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI drinkReminderText;
    [SerializeField] private RectTransform drinkReminderRectTransform;
    [SerializeField] private Button closeButton;
    [SerializeField] private UIElementController uiElementController;

    private int numberOfSips;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            HideWithAnimation();

            MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerDrinkReminderMessage());
        });

        Player.OnPlayerResurrected += Player_OnPlayerResurrected;
        Player.OnAction += Player_OnAction;

        HideInstant();
    }

    private void OnDestroy()
    {
        Player.OnPlayerResurrected -= Player_OnPlayerResurrected;
        Player.OnAction -= Player_OnAction;

        closeButton.onClick.RemoveAllListeners();
    }

    private void Player_OnPlayerResurrected()
    {
        uiElementController.AddEvent(() =>
        {
            numberOfSips = Player.LocalInstance.SipValue;

            SetText();

            ShowWithAnimation();
        });
    }

    private void Player_OnAction()
    {
        uiElementController.AddEvent(() =>
        {
            numberOfSips = Player.LocalInstance.ActionSipValue;

            SetText();

            ShowWithAnimation();
        });
    }

    private void SetText()
    {
        string sipText = (numberOfSips > 1) ? "sips" : "sip";
        drinkReminderText.text = $"Drink {numberOfSips} {sipText}";
    }

    public void ShowWithAnimation()
    {
        Show();
        drinkReminderRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    private void HideWithAnimation()
    {
        drinkReminderRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        uiElementController.CloseUIElement();
    }

    private void HideInstant()
    {
        drinkReminderRectTransform.DOScale(Vector2.zero, .0f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private string[] CreateOnPlayerDrinkReminderMessage()
    {
        string sipText = (numberOfSips > 1) ? " sips" : " sip";

        return new string[]
        {
            $"YOU DRANK {numberOfSips + sipText}",
            $"<color=#{Player.LocalInstance.HexPlayerColor}>{Player.LocalInstance.PlayerName} </color>drank {numberOfSips + sipText}"
        };
    }
}

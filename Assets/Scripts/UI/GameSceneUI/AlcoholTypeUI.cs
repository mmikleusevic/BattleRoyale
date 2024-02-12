using DG.Tweening;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class AlcoholTypeUI : MonoBehaviour
{
    [SerializeField] private RectTransform alcoholTypeRectTransform;
    [SerializeField] private Button strongAlcoholButton;
    [SerializeField] private Button mediumAlcoholButton;
    [SerializeField] private Button weakAlcoholButton;

    private void Awake()
    {
        strongAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(1);
            Hide();
        });

        mediumAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(2);
            Hide();
        });

        weakAlcoholButton.onClick.AddListener(() =>
        {
            SendAcoholValue(3);
            Hide();
        });

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;

        Hide();
    }

    private void OnDestroy()
    {
        strongAlcoholButton.onClick.RemoveAllListeners();
        mediumAlcoholButton.onClick.RemoveAllListeners();
        weakAlcoholButton.onClick.RemoveAllListeners();
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
        alcoholTypeRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        alcoholTypeRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false)).Kill(true);
    }
}

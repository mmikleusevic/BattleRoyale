using DG.Tweening;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] RectTransform interactUIRectTransform;

    private void Awake()
    {
        Card.OnCardPressed += Card_OnCardPressed;
        ActionsUI.OnMove += ActionsUI_OnMove;
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer += ActionsUI_OnAttackPlayer;
    }

    private void Start()
    {
        HideWithAnimation();
    }

    private void OnDestroy()
    {
        Card.OnCardPressed -= Card_OnCardPressed;
        ActionsUI.OnMove -= ActionsUI_OnMove;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;
    }

    private void Card_OnCardPressed(object sender, Player player)
    {
        ShowWithAnimation();
    }

    private void ActionsUI_OnMove(Card card)
    {
        HideWithAnimation();
    }

    private void ActionsUI_OnAttackCard(Card card, string[] messages)
    {
        HideWithAnimation();
    }

    private void ActionsUI_OnAttackPlayer(Card card)
    {
        HideWithAnimation();
    }

    public void ShowWithAnimation()
    {
        Show();
        interactUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack);
    }

    public void HideWithAnimation()
    {
        interactUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => gameObject.SetActive(false));
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

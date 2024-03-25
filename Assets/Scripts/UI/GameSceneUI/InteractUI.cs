using DG.Tweening;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private RectTransform interactUIRectTransform;
    [SerializeField] private Transform actionsContainer;

    private void Awake()
    {
        Tile.OnTilePressed += Tile_OnTilePressed;
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
        Tile.OnTilePressed -= Tile_OnTilePressed;
        ActionsUI.OnMove -= ActionsUI_OnMove;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;
    }

    private void Tile_OnTilePressed(Tile tile)
    {
        ShowWithAnimation();
    }

    private void ActionsUI_OnMove(Tile tile)
    {
        HideWithAnimation();
    }

    private void ActionsUI_OnAttackCard(Tile tile, string[] messages)
    {
        HideWithAnimation();
    }

    private void ActionsUI_OnAttackPlayer(Tile tile)
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
        interactUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => Hide());
    }

    private void Show()
    {
        actionsContainer.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        actionsContainer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}

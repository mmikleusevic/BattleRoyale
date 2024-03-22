using DG.Tweening;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] RectTransform interactUIRectTransform;

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

    private void Tile_OnTilePressed(object sender, Player player)
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
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

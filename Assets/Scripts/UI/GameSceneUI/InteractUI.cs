using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private RectTransform interactUIRectTransform;
    [SerializeField] private Transform actionsContainer;

    private bool interactable = true;
    private Tile tile;

    private void Awake()
    {
        Tile.OnTilePressed += Tile_OnTilePressed;
        ActionsUI.OnMove += Hide;
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer += Hide;
        AbilityUI.OnAbilityUsed += AbilityUI_OnAbilityUsed;
    }

    private void Start()
    {
        HideWithAnimation();
    }

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
        ActionsUI.OnMove -= Hide;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer -= Hide;
        AbilityUI.OnAbilityUsed -= AbilityUI_OnAbilityUsed;
    }

    private void Tile_OnTilePressed(Tile tile)
    {
        this.tile = tile;

        ShowWithAnimation();
    }

    private void Hide(Tile tile)
    {
        HideWithAnimation();
    }

    private void ActionsUI_OnAttackCard(NetworkObjectReference arg1, NetworkObjectReference arg2)
    {
        HideWithAnimation();
    }

    private void AbilityUI_OnAbilityUsed()
    {
        HideWithAnimation();
    }

    public void ShowWithAnimation()
    {
        if (interactable)
        {
            interactable = false;

            Show();
            interactUIRectTransform.DOScale(Vector2.one, .4f).SetEase(Ease.InOutBack).OnComplete(() => interactable = true);
        }
    }

    public void HideWithAnimation()
    {
        if (interactable)
        {
            interactable = false;

            interactUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() =>
            {
                Hide();
                interactable = true;
            });
        }
    }

    private void Show()
    {
        actionsContainer.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        tile = null;

        actionsContainer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public Tile GetTile()
    {
        return tile;
    }
}

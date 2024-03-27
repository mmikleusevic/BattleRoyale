using DG.Tweening;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private RectTransform interactUIRectTransform;
    [SerializeField] private Transform actionsContainer;

    private bool interactable = true;

    private void Awake()
    {
        Tile.OnTilePressed += Tile_OnTilePressed;
        ActionsUI.OnMove += Hide;
        ActionsUI.OnAttackCard += Hide;
        ActionsUI.OnAttackPlayer += Hide;
    }

    private void Start()
    {
        HideWithAnimation();
    }

    private void OnDestroy()
    {
        Tile.OnTilePressed -= Tile_OnTilePressed;
        ActionsUI.OnMove -= Hide;
        ActionsUI.OnAttackCard -= Hide;
        ActionsUI.OnAttackPlayer -= Hide;
    }

    private void Tile_OnTilePressed(Tile tile)
    {
        ShowWithAnimation();
    }

    private void Hide(Tile tile)
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

            interactUIRectTransform.DOScale(Vector2.zero, .4f).SetEase(Ease.InOutBack).OnComplete(() => {
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
        actionsContainer.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}

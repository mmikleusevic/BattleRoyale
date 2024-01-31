using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;

    private GridManager gridManager;

    private bool IsDisabled { get; set; } = true;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Show();

        Debug.Log("You placed");

        gridManager.NextClientPlacingServerRpc();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Hide();
    }

    private void Show()
    {
        highlight.SetActive(true);
    }

    private void Hide()
    {
        highlight.SetActive(false);
    }
}

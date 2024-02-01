using Unity.Netcode;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;

    private GridManager gridManager;

    private NetworkVariable<bool> isClosed = new NetworkVariable<bool>(false);

    private bool IsDisabled { get; set; } = true;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Show();

        if (!isClosed.Value && !IsDisabled)
        {
            CloseCardServerRpc();
        }
        else if (IsDisabled)
        {
            gridManager.NextClientPlacingServerRpc();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Hide();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CloseCardServerRpc(ServerRpcParams serverRpcParams = default)
    {
        isClosed.Value = true;
        CloseCardClientRpc();
    }

    [ClientRpc]
    private void CloseCardClientRpc(ClientRpcParams clientRpcParams = default)
    {
        CloseCard();
    }

    private void CloseCard()
    {
        GetComponent<CardAnimator>().CloseCard();
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

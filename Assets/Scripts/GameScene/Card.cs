using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;

    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(bool previous, bool current)
    {
        isOpen.Value = current;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Show();

        OpenCardServerRpc();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Hide();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenCardServerRpc()
    {
        if (!isOpen.Value)
        {
            OpenCardClientRpc();
            isOpen.Value = true;
        }
    }

    [ClientRpc]
    private void OpenCardClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("b");
        OpenCard();
    }

    private void OpenCard()
    {
        GetComponent<CardAnimator>().OpenCard();
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

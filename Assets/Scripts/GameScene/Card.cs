using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static event EventHandler OnCardPressed;

    [SerializeField] private GameObject highlight;
    [SerializeField] private PlayerCardSpot[] playerCardSpots;

    private CardAnimator cardAnimator;

    private NetworkVariable<bool> isClosed = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isOccupiedOnPlacing { get; private set; }

    public bool Interactable { get; private set; } = false;

    private void Awake()
    {
        isOccupiedOnPlacing = new NetworkVariable<bool>(false);
    }

    private void Start()
    {
        cardAnimator = GetComponent<CardAnimator>();
    }

    public PlayerCardSpot FindFirstEmptyPlayerSpot()
    {
        foreach (PlayerCardSpot playerCardSpot in playerCardSpots)
        {
            if (playerCardSpot.IsOccupied == false)
            {
                OccupyCardServerRpc();
                return playerCardSpot;
            }
        }

        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void OccupyCardServerRpc()
    {
        isOccupiedOnPlacing.Value = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ShowHighlight();

        if (Interactable)
        {
            OnCardPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HideHighlight();
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
        cardAnimator.CloseCardAnimation();
    }

    public void ShowHighlight()
    {
        highlight.SetActive(true);
    }

    public void HideHighlight()
    {
        highlight.SetActive(false);
    }

    public void Enable()
    {
        Interactable = true;
    }

    public void Disable()
    {
        Interactable = false;
    }
}

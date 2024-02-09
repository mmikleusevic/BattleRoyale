using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler
{
    public static event EventHandler<Player> OnCardPressed;

    [SerializeField] private GameObject highlight;
    [SerializeField] private PlayerCardSpot[] playerCardSpots;

    private CardAnimator cardAnimator;

    private NetworkVariable<bool> isClosed = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isOccupiedOnPlacing { get; private set; }

    public bool Interactable { get; private set; } = false;

    public Vector2 GridPosition { get; private set; }
    public string Name { get; private set; }
    public Sprite Sprite { get; private set; }

    private void Awake()
    {
        isOccupiedOnPlacing = new NetworkVariable<bool>(false);
    }

    private void Start()
    {
        cardAnimator = GetComponent<CardAnimator>();
    }

    [ClientRpc]
    public void InitializeClientRpc(int index, Vector2 gridPosition)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        GridPosition = gridPosition;
        Sprite = cardSO.cardSprite;
        Name = cardSO.name;
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
        OnCardPressed?.Invoke(this, Player.LocalInstance);
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

    public bool AreMultiplePeopleOnTheCard()
    {
        int count = 0;
        foreach (PlayerCardSpot playerCardSpot in playerCardSpots)
        {
            if (playerCardSpot.IsOccupied == true)
            {
                count++;
            }
        }

        if (count >= 2) return true;

        return false;
    }
}

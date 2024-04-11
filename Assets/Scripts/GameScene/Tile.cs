using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : NetworkBehaviour, IPointerDownHandler
{
    public static event Action<Tile> OnTilePressed;
    public event Action OnTileValueChanged;

    [SerializeField] private List<CardPosition> cardPositions;
    [SerializeField] private GameObject highlight;
    [SerializeField] private Sprite defaultSprite;

    private TileAnimator tileAnimator;

    private string tileName;

    public NetworkVariable<bool> IsOccupiedOnPlacing { get; private set; }
    public Card Card { get; private set; }
    public Sprite Sprite { get; private set; }
    public Vector2 GridPosition { get; private set; }
    public bool IsClosed { get; private set; }
    public bool Interactable { get; private set; }

    private void Awake()
    {
        IsOccupiedOnPlacing = new NetworkVariable<bool>(false);
        Card = GetComponent<Card>();
    }

    private void Start()
    {
        tileAnimator = GetComponent<TileAnimator>();
    }

    [ClientRpc]
    private void SetCardClosedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Sprite = defaultSprite;

        if (Player.LocalInstance == PlayerManager.Instance.ActivePlayer)
        {
            GridManager.Instance.GetGridPositionsWherePlayerCanInteract();
        }
    }

    [ClientRpc]
    public void InitializeClientRpc(Vector3 gridPosition, int index)
    {
        CardSO cardSO = GridManager.Instance.GetCardSOAtPosition(index);

        tileName = "TILE" + index;
        Sprite = cardSO.cardSprite;
        GridPosition = gridPosition;
    }

    public CardPosition GetCardPosition(Player player)
    {
        return cardPositions.Where(a => a.Player == player).FirstOrDefault();
    }

    public void SetEmptyCardPosition(Player player)
    {
        SetEmptyCardPositionServerRpc(player.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetEmptyCardPositionServerRpc(NetworkObjectReference networkObjectReferencePlayer, ServerRpcParams serverRpcParams = default)
    {
        SetEmptyCardPositionClientRpc(networkObjectReferencePlayer);
    }

    [ClientRpc]
    private void SetEmptyCardPositionClientRpc(NetworkObjectReference networkObjectReferencePlayer, ClientRpcParams clientRpcParams = default)
    {
        Player player = Player.GetPlayerFromNetworkReference(networkObjectReferencePlayer);

        if (player == null) return;

        foreach (CardPosition cardPosition in cardPositions)
        {
            if (cardPosition.IsOccupied == false)
            {
                cardPosition.Player = player;
                cardPosition.IsOccupied = true;

                if (player == Player.LocalInstance)
                {
                    StartCoroutine(Player.LocalInstance.MovePlayerPosition(this));
                }

                return;
            }
        }
    }

    public bool AreMultipleAlivePlayersOnTheCard()
    {
        int count = 0;
        foreach (CardPosition cardPosition in cardPositions)
        {
            if (cardPosition.IsOccupied == true && !cardPosition.Player.IsDead.Value && !cardPosition.Player.Disabled)
            {
                count++;
            }
        }

        if (count >= 2) return true;

        return false;
    }

    public List<Player> GetAlivePlayersOnCard()
    {
        List<Player> players = new List<Player>();

        foreach (CardPosition cardPosition in cardPositions)
        {
            if (cardPosition.Player != null && !cardPosition.Player.IsDead.Value && !cardPosition.Player.Disabled)
            {
                players.Add(cardPosition.Player);
            }
        }

        return players;
    }

    public string GetCardOrTileName()
    {
        if (Card != null)
        {
            return Card.Name;
        }
        else
        {
            return tileName;
        }
    }

    public Sprite GetCardOrTileSprite()
    {
        if (Card != null)
        {
            return Card.Sprite;
        }
        else
        {
            return Sprite;
        }
    }

    public void OnMoveResetPlayerPosition(NetworkObjectReference networkObjectReference)
    {
        OnMoveResetPlayerPositionServerRpc(networkObjectReference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnMoveResetPlayerPositionServerRpc(NetworkObjectReference networkObjectReference)
    {
        OnMoveResetPlayerPositionClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void OnMoveResetPlayerPositionClientRpc(NetworkObjectReference networkObjectReferencePlayer)
    {
        Player player = Player.GetPlayerFromNetworkReference(networkObjectReferencePlayer);

        if (player == null) return;

        CardPosition cardPosition = cardPositions.Where(a => a.Player == player).FirstOrDefault();

        if (cardPosition == null) return;

        cardPosition.IsOccupied = false;
        cardPosition.Player = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetIsOccupiedOnPlacingServerRpc()
    {
        IsOccupiedOnPlacing.Value = true;
    }

    public void DisableCard()
    {
        SetIsClosedTileServerRpc();

        CloseCardServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetIsClosedTileServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetIsClosedTileClientRpc();
    }

    [ClientRpc]
    private void SetIsClosedTileClientRpc(ClientRpcParams clientRpcParams = default)
    {
        IsClosed = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveCardServerRpc(ServerRpcParams serverRpcParams = default)
    {
        RemoveCardClientRpc();
    }

    [ClientRpc]
    private void RemoveCardClientRpc(ClientRpcParams clientRpcParams = default)
    {
        RemoveCard();
    }

    private void RemoveCard()
    {
        Card = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnTilePressed?.Invoke(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CloseCardServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetCardClosedClientRpc();

        CloseCard();
    }

    private void CloseCard()
    {
        tileAnimator.CloseTileAnimation();
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

        OnTileValueChanged?.Invoke();
    }

    public void Disable()
    {
        Interactable = false;

        OnTileValueChanged?.Invoke();
    }

    public static Tile GetTileFromNetworkReference(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return null;

        return networkObject.GetComponent<Tile>();
    }
}
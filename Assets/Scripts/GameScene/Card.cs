using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler
{
    public static event EventHandler<Player> OnCardPressed;
    public static event Action OnCardClosed;

    [SerializeField] private GameObject highlight;
    [SerializeField] private PlayerCardPosition[] playerCardPositions;
    [SerializeField] private Sprite defaultSprite;

    private CardAnimator cardAnimator;

    public NetworkVariable<bool> IsOccupiedOnPlacing { get; private set; }
    public bool IsClosed { get; private set; } = false;
    public bool Interactable { get; private set; } = false;
    public Vector2 GridPosition { get; private set; }
    public string Name { get; private set; }
    public Sprite Sprite { get; private set; }
    public int Value { get; private set; }

    private void Awake()
    {
        IsOccupiedOnPlacing = new NetworkVariable<bool>(false);
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
        Value = cardSO.cost;
    }

    public void DisableCard()
    {
        CloseCardServerRpc();
    }


    [ClientRpc]
    private void SetCardClosedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        IsClosed = true;
        Sprite = defaultSprite;

        if (Player.LocalInstance == PlayerManager.Instance.ActivePlayer)
        {
            OnCardClosed?.Invoke();
        }
    }

    public PlayerCardPosition GetPlayerCardSpot(Player player)
    {
        return playerCardPositions.Where(a => a.Player == player).FirstOrDefault();
    }

    public void SetEmptyPlayerCardSpot(Player player)
    {
        SetEmptyPlayerSpotServerRpc(player.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetEmptyPlayerSpotServerRpc(NetworkObjectReference networkObjectReferencePlayer, ServerRpcParams serverRpcParams = default)
    {
        SetEmptyPlayerSpotClientRpc(networkObjectReferencePlayer);
    }

    [ClientRpc]
    private void SetEmptyPlayerSpotClientRpc(NetworkObjectReference networkObjectReferencePlayer, ClientRpcParams clientRpcParams = default)
    {
        networkObjectReferencePlayer.TryGet(out NetworkObject networkObjectPlayer);

        if (networkObjectPlayer == null) return;

        Player player = networkObjectPlayer.GetComponent<Player>();

        foreach (PlayerCardPosition playerCardPosition in playerCardPositions)
        {
            if (playerCardPosition.IsOccupied == false)
            {
                playerCardPosition.Player = player;
                playerCardPosition.IsOccupied = true;

                if (player.ClientId.Value == Player.LocalInstance.ClientId.Value)
                {
                    Player.LocalInstance.MovePlayerPosition(this);
                }

                return;
            }
        }
    }

    public bool AreMultiplePeopleOnTheCard()
    {
        int count = 0;
        foreach (PlayerCardPosition playerCardPosition in playerCardPositions)
        {
            if (playerCardPosition.IsOccupied == true && !playerCardPosition.Player.Dead)
            {
                count++;
            }
        }

        if (count >= 2) return true;

        return false;
    }

    public List<Player> GetPlayersOnCard()
    {
        List<Player> players = new List<Player>();

        foreach (PlayerCardPosition playerCardPosition in playerCardPositions)
        {
            if (playerCardPosition.Player != null && !playerCardPosition.Player.Dead)
            {
                players.Add(playerCardPosition.Player);
            }
        }

        return players;
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
        networkObjectReferencePlayer.TryGet(out NetworkObject networkObjectPlayer);

        if (networkObjectPlayer == null) return;

        Player player = networkObjectPlayer.GetComponent<Player>();

        PlayerCardPosition playerCarPosition = playerCardPositions.Where(a => a.Player == player).FirstOrDefault();

        if (playerCarPosition == null) return;

        playerCarPosition.IsOccupied = false;
        playerCarPosition.Player = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OccupyCardOnPlaceOnGridServerRpc()
    {
        IsOccupiedOnPlacing.Value = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnCardPressed?.Invoke(this, Player.LocalInstance);
    }


    [ServerRpc(RequireOwnership = false)]
    private void CloseCardServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetCardClosedClientRpc();
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

using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler
{
    public static event EventHandler<Player> OnCardPressed;
    public static event EventHandler OnPlayerCardSpotSet;

    [SerializeField] private GameObject highlight;
    [SerializeField] private PlayerCardPosition[] playerCardSpots;

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

    public PlayerCardPosition GetPlayerCardSpot(Player player)
    {
        return playerCardSpots.Where(a => a.Player == player).FirstOrDefault();
    }

    public void SetEmptyPlayerCardSpot(Player player)
    {
        SetEmptyPlayerSpotServerRpc(player.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetEmptyPlayerSpotServerRpc(NetworkObjectReference networkObjectReferencePlayer)
    {
        SetEmptyPlayerSpotClientRpc(networkObjectReferencePlayer);
    }

    [ClientRpc]
    private void SetEmptyPlayerSpotClientRpc(NetworkObjectReference networkObjectReferencePlayer)
    {
        networkObjectReferencePlayer.TryGet(out NetworkObject networkObjectPlayer);

        if (networkObjectPlayer == null) return;

        Player player = networkObjectPlayer.GetComponent<Player>();
      
        foreach (PlayerCardPosition playerCardSpot in playerCardSpots)
        {
            if (playerCardSpot.IsOccupied == false)
            {
                playerCardSpot.Player = player;
                playerCardSpot.IsOccupied = true;
                OnPlayerCardSpotSet?.Invoke(this, EventArgs.Empty);
                return;
            }
        }        
    }

    public bool AreMultiplePeopleOnTheCard()
    {
        int count = 0;
        foreach (PlayerCardPosition playerCardSpot in playerCardSpots)
        {
            if (playerCardSpot.IsOccupied == true)
            {
                count++;
            }
        }

        if (count >= 2) return true;

        return false;
    }

    public void OnMoveResetPosition(NetworkObjectReference networkObjectReference)
    {
        OnMoveResetPositionServerRpc(networkObjectReference);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnMoveResetPositionServerRpc(NetworkObjectReference networkObjectReference)
    {
        OnMoveResetPositionClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void OnMoveResetPositionClientRpc(NetworkObjectReference networkObjectReferencePlayer)
    {
        networkObjectReferencePlayer.TryGet(out NetworkObject networkObjectPlayer);

        if (networkObjectPlayer == null) return;

        Player player = networkObjectPlayer.GetComponent<Player>();

        PlayerCardPosition playerCardSpot = playerCardSpots.Where(a => a.Player == player).FirstOrDefault();

        if(playerCardSpot == null) return;

        playerCardSpot.IsOccupied = false;
        playerCardSpot.Player = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OccupyCardOnPlaceOnGridServerRpc()
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
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : NetworkBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private PlayerCardSpot[] playerCardSpots;

    private GridManager gridManager;
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
        gridManager = FindFirstObjectByType<GridManager>();
        cardAnimator = GetComponent<CardAnimator>();
    }

    private PlayerCardSpot FindFirstEmptyPlayerSpot()
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

    private void PlacePlayerOnGrid()
    {
        PlayerCardSpot playerCardSpot = FindFirstEmptyPlayerSpot();

        Player.LocalInstance.SetPlayersPosition(this, playerCardSpot);

        gridManager.DisableCards();

        GameManager.Instance.NextClientPlacingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OccupyCardServerRpc()
    {
        isOccupiedOnPlacing.Value = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Interactable)
        {
            PlacePlayerOnGrid();

            //if (!isClosed.Value)
            //{
            //    CloseCardServerRpc();
            //}
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
        cardAnimator.CloseCard();
    }

    public void ShowHighlight()
    {
        highlight.SetActive(true);
    }

    public void Enable()
    {
        Interactable = true;
    }

    public void Disable()
    {
        Interactable = false;
    }

    public void HideHighlight()
    {
        highlight.SetActive(false);
    }
}

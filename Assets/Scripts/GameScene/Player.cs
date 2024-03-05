using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    public static event Action OnPlayerTurnSet;
    public static event EventHandler<string> OnPlayerMoved;
    public static event Action OnPlayerActionUsed;
    public static event Action OnPlayerDiedCardBattle;
    public static event Action OnPlayerSelectedPlaceToDie;
    public static event Action<string[]> OnPlayerResurrected;

    [SerializeField] private SetVisual playerVisual;
    [SerializeField] private GameObject particleCircle;

    ParticleSystem playerParticleSystem;
    PlayerAnimator playerAnimator;

    private int maxEquipableCards = 3;
    private int defaultMovement = 0;
    private int defaultActionPoints = 2;
    private float moveSpeed = 20f;

    public Tile CurrentTile { get; private set; }
    public List<Card> EquippedCards { get; private set; }
    public List<Card> UnequippedCards { get; private set; }
    public NetworkVariable<ulong> ClientId { get; private set; }
    public NetworkVariable<bool> IsDead { get; set; }
    public Vector2 GridPosition { get; private set; }
    public int Movement { get; private set; }
    public int ActionPoints { get; private set; }
    public int SipValue { get; private set; }
    public int SipCounter { get; private set; }
    public int Points { get; private set; }
    public string HexPlayerColor { get; private set; }
    public string PlayerName { get; private set; }
    
    private void Awake()
    {
        ClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        IsDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        playerParticleSystem = particleCircle.GetComponent<ParticleSystem>();
        EquippedCards = new List<Card>();
        UnequippedCards = new List<Card>();

        Movement = defaultMovement;
        ActionPoints = defaultActionPoints;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            ClientId.Value = NetworkObject.OwnerClientId;
            PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;
            CardBattleResults.OnCardWon += CardBattleResults_OnCardWon;
            ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
            ActionsUI.OnAttackPlayer += ActionsUI_OnAttackPlayer;
            CardBattleResults.OnCardLost += CardBattleResults_OnCardLost;
            ResurrectUI.OnResurrectPressed += ResurrectUI_OnResurrectPressed;
        }

        InitializePlayerClientRpc();

        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardWon;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;
        ActionsUI.OnAttackPlayer -= ActionsUI_OnAttackPlayer;
        CardBattleResults.OnCardLost -= CardBattleResults_OnCardLost;
        ResurrectUI.OnResurrectPressed -= ResurrectUI_OnResurrectPressed;

        base.OnDestroy();
    }


    [ClientRpc]
    private void InitializePlayerClientRpc()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        Color color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        HexPlayerColor = color.ToHexString();
        PlayerName = playerData.playerName.ToString();

        playerVisual.SetColor(color);

        playerAnimator = playerVisual.gameObject.GetComponent<PlayerAnimator>();
    }

    public void MovePlayerPosition(Tile tile)
    {
        CardPosition cardPosition = tile.GetCardPosition(this);

        if (cardPosition == null) return;

        Vector3 targetPosition = tile.transform.position + cardPosition.Position;

        StartCoroutine(PlayWalkingAnimation(targetPosition));

        string message = string.Empty;

        if (!IsDead.Value)
        {
            if (Movement > 0)
            {
                SubtractMovement();
            }
            else
            {
                SubtractActionPoints();
            }

            message = CreateOnPlayerMovedMessage(tile);
        }
        else
        {
            message = CreateOnPlayerDiedMoveMessage(tile);

            DeathAnimationServerRpc();

            OnPlayerSelectedPlaceToDie?.Invoke();
        }

        GridPosition = tile.GridPosition;

        if (CurrentTile != null)
        {
            OnPlayerMoved?.Invoke(this, message);
        }

        CurrentTile = tile;
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
    {
        ResetActionsAndMovement();
    }

    private void CardBattleResults_OnCardWon(CardBattleResults.OnCardWonEventArgs obj)
    {
        SaveWonCardServerRpc(obj.card.NetworkObject, NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveWonCardServerRpc(NetworkObjectReference networkObjectReferenceCard, NetworkObjectReference networkObjectReferencePlayer, ServerRpcParams serverRpcParams = default)
    {
        SaveWonCardClientRpc(networkObjectReferenceCard, networkObjectReferencePlayer);
    }

    [ClientRpc]
    private void SaveWonCardClientRpc(NetworkObjectReference networkObjectReferenceCard, NetworkObjectReference networkObjectReferencePlayer, ClientRpcParams clientRpcParams = default)
    {
        networkObjectReferenceCard.TryGet(out NetworkObject networkObjectCard);

        if (networkObjectCard == null) return;

        Tile tile = networkObjectCard.GetComponent<Tile>();

        Player player = GetPlayerFromNetworkReference(networkObjectReferencePlayer);

        if (EquippedCards.Count < maxEquipableCards)
        {
            player.EquippedCards.Add(tile.Card);
        }
        else
        {
            player.UnequippedCards.Add(tile.Card);
        }

        tile.RemoveCardServerRpc();
    }

    private void ActionsUI_OnAttackCard(Tile obj, string[] messages)
    {
        SubtractActionPoints();
    }

    private void ActionsUI_OnAttackPlayer(Tile obj)
    {
        SubtractActionPoints();
    }

    private void CardBattleResults_OnCardLost(string[] obj)
    {
        IsDead.Value = true;

        DeathAnimationServerRpc();

        OnPlayerDiedCardBattle?.Invoke();
    }

    private void ResurrectUI_OnResurrectPressed()
    {
        IsDead.Value = false;

        AliveAnimationServerRpc();

        SubtractActionPoints();

        AddSips();

        OnPlayerResurrected?.Invoke(CreateOnPlayerResurrectedMessage());
    }

    public void SetPlayersPosition(Tile tile)
    {
        if (CurrentTile != null)
        {
            CurrentTile.OnMoveResetPlayerPosition(NetworkObject);
        }

        tile.SetEmptyCardPosition(this);
    }

    private string CreateOnPlayerMovedMessage(Tile tile)
    {
        return $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"moved to {tile.Card.Name}";
    }

    private string CreateOnPlayerDiedMoveMessage(Tile tile)
    {
        return $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"chose to die on {tile.Card.Name}";
    }

    private string[] CreateOnPlayerResurrectedMessage()
    {
        return new string[]
        {
            "YOU RESURRECTED",
            $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"has resurrected"
        };
    }

    public void SetSipValue(int value)
    {
        SipValue = value;
    }

    public void SetPlayerPoints(int cardValue)
    {
        Points += Points + cardValue;
    }

    public void AddSips()
    {
        SipCounter += SipValue;
    }

    private IEnumerator PlayWalkingAnimation(Vector3 targetPosition)
    {
        MoveAnimationServerRpc();

        while (targetPosition != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            yield return null;
        }

        StopMovingAnimationServerRpc();
    }

    public static Player GetPlayerFromNetworkReference(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return null;

        return networkObject.GetComponent<Player>();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveAnimationServerRpc(ServerRpcParams serverRpcParams = default)
    {
        MoveAnimationClientRpc();
    }

    [ClientRpc]
    private void MoveAnimationClientRpc(ClientRpcParams clientRpcParams = default)
    {
        MoveAnimation();
    }

    private void MoveAnimation()
    {
        playerAnimator.MoveAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopMovingAnimationServerRpc(ServerRpcParams serverRpcParams = default)
    {
        StopMovingAnimationClientRpc();
    }

    [ClientRpc]
    private void StopMovingAnimationClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StopMovingAnimation();
    }

    private void StopMovingAnimation()
    {
        playerAnimator.StopMovingAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeathAnimationServerRpc(ServerRpcParams serverRpcParams = default)
    {
        DeathAnimationClientRpc();
    }

    [ClientRpc]
    private void DeathAnimationClientRpc(ClientRpcParams clientRpcParams = default)
    {
        DeathAnimation();
    }

    private void DeathAnimation()
    {
        playerAnimator.DieAnimation();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AliveAnimationServerRpc(ServerRpcParams serverRpcParams = default)
    {
        AliveAnimationClientRpc();
    }

    [ClientRpc]
    private void AliveAnimationClientRpc(ClientRpcParams clientRpcParams = default)
    {
        AliveAnimation();
    }

    private void AliveAnimation()
    {
        playerAnimator.AliveAnimation();
    }

    public void ShowParticleCircle()
    {
        particleCircle.SetActive(true);
        playerParticleSystem.Play();
    }

    public void HideParticleCircle()
    {
        particleCircle.SetActive(false);
        playerParticleSystem.Stop();
    }

    public void SubtractMovement()
    {
        Movement--;
    }

    public void SubtractActionPoints()
    {
        ActionPoints--;

        OnPlayerActionUsed?.Invoke();
    }

    private void ResetActionsAndMovement()
    {
        ActionPoints = defaultActionPoints;
        Movement = defaultMovement;

        OnPlayerTurnSet?.Invoke();
    }
}

using Mono.Cecil;
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

    private int defaultMovement = 0;
    private int defaultActionPoints = 2;
    private float moveSpeed = 20f;

    public Card CurrentCard { get; private set; }
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
    public bool Dead { get; private set; }

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

    public void MovePlayerPosition(Card card)
    {
        PlayerCardPosition playerCardSpot = card.GetPlayerCardSpot(this);

        if (playerCardSpot == null) return;

        Vector3 targetPosition = card.transform.position + playerCardSpot.Position;

        StartCoroutine(PlayWalkingAnimation(targetPosition));

        string message = string.Empty;

        if (!Dead)
        {
            if (Movement > 0)
            {
                SubtractMovement();
            }
            else
            {
                SubtractActionPoints();
            }

            message = CreateOnPlayerMovedMessage(card);
        }
        else
        {
            message = CreateOnPlayerDiedMoveMessage(card);

            DeathAnimation();

            OnPlayerSelectedPlaceToDie?.Invoke();
        }

        GridPosition = card.GridPosition;

        if (CurrentCard != null)
        {
            OnPlayerMoved?.Invoke(this, message);
        }

        CurrentCard = card;
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
    {
        ResetActionsAndMovement();
    }

    private void CardBattleResults_OnCardWon(CardBattleResults.OnCardWonEventArgs obj)
    {
        if (EquippedCards.Count < 3)
        {
            EquippedCards.Add(obj.card);
        }
        else
        {
            UnequippedCards.Add(obj.card);
        }
    }

    private void ActionsUI_OnAttackCard(Card obj)
    {
        SubtractActionPoints();
    }

    private void ActionsUI_OnAttackPlayer(Card obj)
    {
        SubtractActionPoints();
    }

    private void CardBattleResults_OnCardLost(string[] obj)
    {
        SendPlayerDeadStatusToAllClientsClientRpc(true);

        DeathAnimation();

        OnPlayerDiedCardBattle?.Invoke();
    }

    private void ResurrectUI_OnResurrectPressed()
    {
        SendPlayerDeadStatusToAllClientsClientRpc(false);

        AliveAnimation();

        SubtractActionPoints();

        AddSips();

        OnPlayerResurrected?.Invoke(CreateOnPlayerResurrectedMessage());
    }

    [ClientRpc]
    private void SendPlayerDeadStatusToAllClientsClientRpc(bool isDead, ClientRpcParams clientRpcParams = default)
    {
        Dead = isDead;
    }

    public void SetPlayersPosition(Card card)
    {
        if (CurrentCard != null)
        {
            CurrentCard.OnMoveResetPlayerPosition(NetworkObject);
        }

        card.SetEmptyPlayerCardSpot(this);
    }

    private string CreateOnPlayerMovedMessage(Card card)
    {
        return $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"moved to {card.Name}";
    }

    private string CreateOnPlayerDiedMoveMessage(Card card)
    {
        return $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"chose to die on {card.Name}";
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
        MoveAnimation();

        while (targetPosition != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            yield return null;
        }

        StopMovingAnimation();
    }

    public static Player GetPlayerFromNetworkReference(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return null;

        return networkObject.GetComponent<Player>();
    }

    private void MoveAnimation()
    {
        playerAnimator.MoveAnimation();
    }

    private void StopMovingAnimation()
    {
        playerAnimator.StopMovingAnimation();
    }

    private void DeathAnimation()
    {
        playerAnimator.DieAnimation();
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

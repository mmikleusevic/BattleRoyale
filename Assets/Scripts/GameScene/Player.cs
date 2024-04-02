using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    public static event Action OnCardsSwapped;
    public static event Action OnPlayerTurnSet;
    public static event Action OnPlayerMoved;
    public static event Action OnPlayerDiedCardBattle;
    public static event Action OnPlayerDiedPlayerBattle;
    public static event Action<ulong> OnPlayerSelectedPlaceToDie;
    public static event Action OnPlayerResurrected;
    public static event Action<Card> OnPlayerUnequippedCardAdded;
    public static event Action OnNoMoreMovementOrActionPoints;

    [SerializeField] private SetVisual playerVisual;
    [SerializeField] private GameObject particleCircle;

    ParticleSystem playerParticleSystem;
    PlayerAnimator playerAnimator;


    public StateEnum currentState = StateEnum.WaitingForPlayers;
    private int defaultMovement = 0;
    private int gamesNeededForDefeat = 3;
    private int defaultActionPoints = 2;
    private float moveSpeed = 20f;

    public bool Disabled { get; private set; } = false;
    public int MaxEquippableCards { get; private set; } = 3;
    public Tile CurrentTile { get; private set; }
    public List<Card> EquippedCards { get; private set; }
    public List<Card> UnequippedCards { get; private set; }
    public NetworkVariable<ulong> ClientId { get; private set; }
    public NetworkVariable<bool> IsDead { get; private set; }
    public NetworkVariable<int> Points { get; private set; }
    public Vector2 GridPosition { get; private set; }
    public int Movement { get; private set; }
    public int ActionPoints { get; private set; }
    public int SipValue { get; private set; }
    public int SipCounter { get; private set; }
    public string HexPlayerColor { get; private set; }
    public string PlayerName { get; private set; }
    public int RollsNeededToLose { get; private set; }
    public bool PickPlaceToDie { get; private set; }

    private void Awake()
    {
        ClientId = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        IsDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        Points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        playerParticleSystem = particleCircle.GetComponent<ParticleSystem>();
        EquippedCards = new List<Card>();
        UnequippedCards = new List<Card>();

        RollsNeededToLose = gamesNeededForDefeat;
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
            PlayerBattleResults.OnBattleLost += PlayerBattleResults_OnBattleLost;
            Points.OnValueChanged += PointsChanged;
        }

        InitializePlayerClientRpc();

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        PlayerBattleResults.OnBattleLost -= PlayerBattleResults_OnBattleLost;
        Points.OnValueChanged -= PointsChanged;

        base.OnNetworkDespawn();
    }


    [ClientRpc]
    private void InitializePlayerClientRpc()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        Color color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);

        HexPlayerColor = color.ToHexString();
        PlayerName = playerData.playerName.ToString().ToUpper();

        playerVisual.SetColor(color);

        playerAnimator = playerVisual.gameObject.GetComponent<PlayerAnimator>();

        MessageUI.Instance.SetMessage(PlayerConnectedMessage());
    }

    public void UpdateCurrentState(StateEnum state)
    {
        UpdateCurrentStateServerRpc(state, NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateCurrentStateServerRpc(StateEnum state, NetworkObjectReference playerNetworkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        UpdateCurrentStateClientRpc(state, playerNetworkObjectReference);
    }

    [ClientRpc]
    private void UpdateCurrentStateClientRpc(StateEnum state, NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player player = GetPlayerFromNetworkReference(playerNetworkObjectReference);

        player.currentState = state;
    }

    private string PlayerConnectedMessage()
    {
        return $"<color=#{HexPlayerColor}>{PlayerName} </color> successfully connected";
    }

    public void MovePlayerPosition(Tile tile)
    {
        CardPosition cardPosition = tile.GetCardPosition(this);

        if (cardPosition == null) return;

        Vector3 targetPosition = tile.transform.position + cardPosition.Position;

        StartCoroutine(PlayWalkingAnimation(targetPosition));

        string[] messages = null;

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

            messages = CreateOnPlayerMovedMessage(tile);
        }
        else
        {
            messages = CreateOnPlayerDiedMoveMessage(tile);

            DeathAnimationServerRpc();

            PickPlaceToDie = false;

            OnPlayerSelectedPlaceToDie?.Invoke(ClientId.Value);
        }

        GridPosition = tile.GridPosition;

        if (CurrentTile != null)
        {
            GridManager.Instance.GetGridPositionsWherePlayerCanInteract();

            OnPlayerMoved?.Invoke();

            MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        }

        CurrentTile = tile;
    }

    private void PlayerTurn_OnPlayerTurn()
    {
        ResetActionsAndMovement();
    }

    public void SaveWonCard(Card card)
    {
        SaveWonCardServerRpc(card.NetworkObject, NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveWonCardServerRpc(NetworkObjectReference networkObjectReferenceCard, NetworkObjectReference networkObjectReferencePlayer, ServerRpcParams serverRpcParams = default)
    {
        Card card = Card.GetCardFromNetworkReference(networkObjectReferenceCard);

        AddOrSubtractPoints(card.Points);

        SaveWonCardClientRpc(networkObjectReferenceCard, networkObjectReferencePlayer);

        GridManager.Instance.CheckNumberOfLeftCards();
    }

    [ClientRpc]
    private void SaveWonCardClientRpc(NetworkObjectReference networkObjectReferenceCard, NetworkObjectReference networkObjectReferencePlayer, ClientRpcParams clientRpcParams = default)
    {
        Tile tile = Tile.GetTileFromNetworkReference(networkObjectReferenceCard);

        Card card = tile.Card;

        Player player = GetPlayerFromNetworkReference(networkObjectReferencePlayer);

        SaveCardToWinner(player, card);

        tile.RemoveCardServerRpc();
    }

    private void SaveCardToWinner(Player player, Card card)
    {
        if (player.EquippedCards.Count < player.MaxEquippableCards)
        {
            player.EquippedCards.Add(card);

            card.Equip(player);

            MessageUI.Instance.SetMessage(CreateOnPlayerEquippedCardMessage(player, card));
        }
        else
        {
            OnPlayerUnequippedCardAdded?.Invoke(card);

            player.UnequippedCards.Add(card);
        }
    }

    public void SwapCardPreturn(int equippedCardIndex, int unequippedCardIndex)
    {
        SwapCardPreturnServerRpc(NetworkObject, equippedCardIndex, unequippedCardIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwapCardPreturnServerRpc(NetworkObjectReference playerNetworkObjectReference, int equippedCardIndex, int unequippedCardIndex, ServerRpcParams serverRpcParams = default)
    {
        SwapCardPreturnClientRpc(playerNetworkObjectReference, equippedCardIndex, unequippedCardIndex);
    }

    [ClientRpc]
    private void SwapCardPreturnClientRpc(NetworkObjectReference playerNetworkObjectReference, int equippedCardIndex, int unequippedCardIndex, ClientRpcParams clientRpcParams = default)
    {
        Player player = GetPlayerFromNetworkReference(playerNetworkObjectReference);

        Card equippedCard = null;

        if (equippedCardIndex < player.EquippedCards.Count)
        {
            equippedCard = player.EquippedCards[equippedCardIndex];
        }

        Card unequippedCard = player.UnequippedCards[unequippedCardIndex];

        if (equippedCard == null)
        {
            player.UnequippedCards.Remove(unequippedCard);
            player.EquippedCards.Insert(equippedCardIndex, unequippedCard);

            unequippedCard.Equip(player);
        }
        else
        {
            player.EquippedCards.Remove(equippedCard);
            player.UnequippedCards.Remove(unequippedCard);

            player.EquippedCards.Insert(equippedCardIndex, unequippedCard);
            player.UnequippedCards.Insert(unequippedCardIndex, equippedCard);

            equippedCard.Unequip(player);
            unequippedCard.Equip(player);
        }

        if (player == LocalInstance)
        {
            OnCardsSwapped?.Invoke();
            MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerSwappedCardMessage(equippedCard, unequippedCard));
        }
    }

    private void RemoveCardFromLoser(Player loser, Card card)
    {
        loser.EquippedCards.Remove(card);

        card.Unequip(loser);

        MessageUI.Instance.SetMessage(CreateOnPlayerRemovedCardMessage(loser, card));
    }

    public void PlayerDiedCardBattle()
    {
        IsDead.Value = true;

        DeathAnimationServerRpc();

        PCInfoUI.Instance.SetIsDeadText();

        GridManager.Instance.DisableCards();

        OnPlayerDiedCardBattle?.Invoke();
    }

    private void PlayerBattleResults_OnBattleLost()
    {
        IsDead.Value = true;

        PickPlaceToDie = true;

        string[] messages = CreateOnPlayerNeedsToPickAPlaceToDieMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.KeepMessage(messages[0]);
        PCInfoUI.Instance.SetIsDeadText();

        GridManager.Instance.ToggleCardToGetGridPositionsWherePlayerCanGoDie();

        OnPlayerDiedPlayerBattle?.Invoke();
    }

    private void PointsChanged(int previousValue, int newValue)
    {
        PCInfoUI.Instance.SetPointsText();
    }

    public void OnBattleWon(Card card, Player loser)
    {
        OnBattleWonServerRpc(card.NetworkObject, NetworkObject, loser.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnBattleWonServerRpc(NetworkObjectReference cardNetworkObjectReference, NetworkObjectReference playerNetworkObjectReference, NetworkObjectReference loserNetworkObjectReference, ServerRpcParams serverRpcParams = default)
    {
        Player winner = GetPlayerFromNetworkReference(playerNetworkObjectReference);

        Player loser = GetPlayerFromNetworkReference(loserNetworkObjectReference);

        Card card = Card.GetCardFromNetworkReference(cardNetworkObjectReference);

        winner.AddOrSubtractPoints(card.Points);
        loser.AddOrSubtractPoints(-card.Points);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerTakenCardMessage(card, winner, loser));

        OnBattleWonClientRpc(cardNetworkObjectReference, playerNetworkObjectReference, loserNetworkObjectReference);
    }

    [ClientRpc]
    private void OnBattleWonClientRpc(NetworkObjectReference cardNetworkObjectReference, NetworkObjectReference winnerNetworkObjectReference, NetworkObjectReference loserNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player winner = GetPlayerFromNetworkReference(winnerNetworkObjectReference);

        Player loser = GetPlayerFromNetworkReference(loserNetworkObjectReference);

        Card card = Card.GetCardFromNetworkReference(cardNetworkObjectReference);

        SaveCardToWinner(winner, card);
        RemoveCardFromLoser(loser, card);
    }

    public void Ressurect()
    {
        IsDead.Value = false;

        AliveAnimationServerRpc();

        SubtractActionPoints();

        AddSipsToPlayerServerRpc(NetworkObject, SipValue);

        GridManager.Instance.GetGridPositionsWherePlayerCanInteract();
        PCInfoUI.Instance.SetIsDeadText();

        OnPlayerResurrected?.Invoke();

        string[] messages = CreateOnPlayerResurrectedMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
    }

    public void SetPlayersPosition(Tile tile)
    {
        if (CurrentTile != null)
        {
            CurrentTile.OnMoveResetPlayerPosition(NetworkObject);
        }

        tile.SetEmptyCardPosition(this);
    }

    private string[] CreateOnPlayerNeedsToPickAPlaceToDieMessage()
    {
        return new string[]
        {
            $"PICK A PLACE TO DIE",
            $"<color=#{HexPlayerColor}>{PlayerName} </color>needs to pick a place to die"
        };
    }

    private string[] CreateOnPlayerMovedMessage(Tile tile)
    {
        return new string[]
        {
            $"YOU MOVED TO {tile.GetCardOrTileName()}",
            $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"moved to {tile.GetCardOrTileName()}"
        };
    }

    private string[] CreateOnPlayerDiedMoveMessage(Tile tile)
    {
        return new string[]
        {
            $"YOU DIED ON {tile.GetCardOrTileName()}",
            $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"chose to die on {tile.GetCardOrTileName()}"
        };
    }

    private string CreateOnPlayerEquippedCardMessage(Player player, Card card)
    {
        if (player == LocalInstance)
        {
            return $"YOU EQUIPPED {card.Name}";
        }
        else
        {
            return $"<color=#{player.HexPlayerColor}>{player.PlayerName} </color>equipped {card.Name}";
        }
    }

    private string CreateOnPlayerRemovedCardMessage(Player player, Card card)
    {
        if (player == LocalInstance)
        {
            return $"YOUR {card.Name} GOT REMOVED";
        }
        else
        {
            return $"<color=#{player.HexPlayerColor}>{player.PlayerName}'s </color>{card.Name} got removed";
        }
    }

    private string[] CreateOnPlayerResurrectedMessage()
    {
        return new string[]
        {
            "YOU RESURRECTED",
            $"<color=#{HexPlayerColor}>{PlayerName} </color>" + $"has resurrected"
        };
    }

    private string CreateOnPlayerTakenCardMessage(Card card, Player winner, Player loser)
    {
        return $"<color=#{winner.HexPlayerColor}>{winner.PlayerName} </color>took {card.Name} from <color=#{loser.HexPlayerColor}>{loser.PlayerName}</color>";
    }

    private string[] CreateOnPlayerSwappedCardMessage(Card equippedCard, Card unequippedCard)
    {
        if (equippedCard == null)
        {
            return new string[]
            {
                $"YOU SWAPPED EMPTY SLOT FOR {unequippedCard.Name}",
                $"<color=#{HexPlayerColor}>{PlayerName} </color> swapped EMPTY SLOT for {unequippedCard.Name}"
            };
        }
        else
        {
            return new string[]
            {
                $"YOU SWAPPED {equippedCard.Name} FOR {unequippedCard.Name}",
                $"<color=#{HexPlayerColor}>{PlayerName} </color>swapped {equippedCard.Name} for {unequippedCard.Name}"
            };
        }
    }

    public void SetSipValue(int value)
    {
        SipValue = value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddSipsToPlayerServerRpc(NetworkObjectReference playerNetworkObjectReference, int value, ServerRpcParams serverRpcParams = default)
    {
        AddSipsToPlayerClientRpc(playerNetworkObjectReference, value);
    }

    [ClientRpc]
    private void AddSipsToPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference, int value, ClientRpcParams clientRpcParams = default)
    {
        Player player = GetPlayerFromNetworkReference(playerNetworkObjectReference);

        AddSips(player, value);
    }

    private void AddSips(Player player, int value)
    {
        player.SipCounter += value;

        if (player == LocalInstance)
        {
            PCInfoUI.Instance.SetSipCounter();
        }
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

    public void DeathAnimation()
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

        CheckIfMovementAndActionsAreZero();
    }

    public void SubtractActionPoints()
    {
        ActionPoints--;

        CheckIfMovementAndActionsAreZero();

        PCInfoUI.Instance.SetActionsText();
    }

    public void AddOrSubtractPoints(int value)
    {
        Points.Value += value;
    }

    private void CheckIfMovementAndActionsAreZero()
    {
        if (Movement == 0 && ActionPoints == 0)
        {
            OnNoMoreMovementOrActionPoints?.Invoke();
        }
    }

    private void ResetActionsAndMovement()
    {
        ActionPoints = defaultActionPoints;
        Movement = defaultMovement;

        GridManager.Instance.GetGridPositionsWherePlayerCanInteract();

        OnPlayerTurnSet?.Invoke();
    }

    public void DisablePlayer()
    {
        DisablePlayerClientRpc(NetworkObject);

        ChangePlayerOwnershipAndDie(this);
    }

    public void ChangePlayerOwnershipAndDie(Player player)
    {
        player.NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);

        player.IsDead.Value = true;

        player.DeathAnimation();
    }

    [ClientRpc]
    private void DisablePlayerClientRpc(NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player player = GetPlayerFromNetworkReference(playerNetworkObjectReference);

        player.Disabled = true;

        PlayerManager.Instance.RemoveFromActivePlayers(player);
    }

    public void SetRollsNeededToLose(int value)
    {
        RollsNeededToLose += value;
    }
}

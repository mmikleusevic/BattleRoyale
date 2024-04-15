using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerBattleResults : NetworkBehaviour, IResult
{
    public static event Action OnPlayerBattleRollDie;
    public static event Action OnPlayerBattleRollDieOver;
    public static event Action OnPlayerBattleRollDisadvantage;
    public static event Action<string> OnPlayerBattleShowUI;
    public static event Action OnPlayerBattleRollOver;
    public static event Action<Player> OnBattleWin;
    public static event Action OnBattleLost;
    public static event Action OnAfterBattleResolved;
    public static event Action<Player> OnPrebattle;
    public static event Action OnPlayerBattleSet;

    private CardAbilities cardAbilities;
    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<ulong, List<int>> battleRolls;
    private Dictionary<ulong, bool> prebattleResolve;
    private Dictionary<ulong, bool> afterBattleResolved;
    private Player player;
    private Player enemy;

    private ClientRpcParams clientRpcParamsBattle;
    private ClientRpcParams clientRpcParamsOther;
    private ClientRpcParams clientRpcParamsPlayer;
    private ClientRpcParams clientRpcParamsEnemy;

    private RollTypeEnum rollTypePlayer;
    private RollTypeEnum rollTypeEnemy;

    private int playerWinsNeeded = 0;
    private int enemyWinsNeeded = 0;
    private int playerWins = 0;
    private int enemyWins = 0;

    private void Awake()
    {
        cardAbilities = FindFirstObjectByType<CardAbilities>();

        PlayerInfoUI.OnAttackPlayer += AttackPlayerServerRpc;
        PlayerCardsEquippedUI.OnWonEquippedCard += ResolvePlayerBattleServerRpc;
        PlayerCardUI.OnPrebattleOver += PrebattleOver;
        PlayerCardsEquippedUI.OnPreturnOver += PrebattleOver;
        Player.OnPlayerSelectedPlaceToDie += ResolvePlayerBattleServerRpc;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();
        battleRolls = new Dictionary<ulong, List<int>>();
        prebattleResolve = new Dictionary<ulong, bool>();
        afterBattleResolved = new Dictionary<ulong, bool>();

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        PlayerInfoUI.OnAttackPlayer -= AttackPlayerServerRpc;
        PlayerCardsEquippedUI.OnWonEquippedCard -= ResolvePlayerBattleServerRpc;
        Player.OnPlayerSelectedPlaceToDie -= ResolvePlayerBattleServerRpc;
        PlayerCardUI.OnPrebattleOver -= PrebattleOver;
        PlayerCardsEquippedUI.OnPreturnOver -= PrebattleOver;

        base.OnNetworkDespawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackPlayerServerRpc(NetworkObjectReference playerNetworkObjectReference, NetworkObjectReference enemyNetworkObjectReference)
    {
        player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);
        enemy = Player.GetPlayerFromNetworkReference(enemyNetworkObjectReference);

        clientRpcParamsPlayer = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { player.ClientId.Value },
            }
        };

        clientRpcParamsEnemy = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { enemy.ClientId.Value },
            }
        };

        int disarmCardsCountPlayer = player.EquippedCards.Count(a => a.Ability is IDisarm);
        bool playerCanDisarm = enemy.EquippedCards.Count > 0 && disarmCardsCountPlayer > 0;

        int disarmCardsCountEnemy = enemy.EquippedCards.Count(a => a.Ability is IDisarm);
        bool enemyCanDisarm = player.EquippedCards.Count > 0 && disarmCardsCountEnemy > 0;

        if (playerCanDisarm)
        {
            prebattleResolve.Add(player.ClientId.Value, false);
            OpenDisarmWindowClientRpc(enemy.NetworkObject, clientRpcParamsPlayer);
        }
        if (enemyCanDisarm)
        {
            prebattleResolve.Add(enemy.ClientId.Value, false);
            OpenDisarmWindowClientRpc(player.NetworkObject, clientRpcParamsEnemy);
        }

        if (prebattleResolve.Count == 0)
        {
            SetPlayerBattle();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResolvePlayerBattleServerRpc(ulong clientId)
    {
        afterBattleResolved[clientId] = true;

        foreach (var item in afterBattleResolved)
        {
            if (item.Value == false) return;
        }

        clientRolled.Clear();
        battleRolls.Clear();
        prebattleResolve.Clear();
        afterBattleResolved.Clear();

        player = null;
        enemy = null;

        CallOnAfterBattleResolvedClientRpc(clientRpcParamsPlayer);
    }

    private void PrebattleOver()
    {
        PrebattleOverServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PrebattleOverServerRpc(ServerRpcParams serverRpcParams = default)
    {
        prebattleResolve[serverRpcParams.Receive.SenderClientId] = true;

        foreach (KeyValuePair<ulong, bool> item in prebattleResolve)
        {
            if (item.Value == false) return;
        }

        SetPlayerBattle();
    }

    [ClientRpc]
    private void OpenDisarmWindowClientRpc(NetworkObjectReference enemyNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player enemy = Player.GetPlayerFromNetworkReference(enemyNetworkObjectReference);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(SendDisarmMessage(Player.LocalInstance));

        OnPrebattle?.Invoke(enemy);
    }

    private void SetPlayerBattle()
    {
        playerWins = 0;
        enemyWins = 0;

        clientRolled[player.ClientId.Value] = false;
        clientRolled[enemy.ClientId.Value] = false;

        SetClientRpcParamsForBattle();

        if (player.EquippedCards.Count > 0 && enemy.EquippedCards.Count > 0 || player.EquippedCards.Count == 0 && enemy.EquippedCards.Count == 0)
        {
            rollTypePlayer = RollTypeEnum.PlayerAttack;
            rollTypeEnemy = RollTypeEnum.PlayerAttack;

            SetRollTypeForClientClientRpc(rollTypePlayer, clientRpcParamsPlayer);
            SetRollTypeForClientClientRpc(rollTypeEnemy, clientRpcParamsEnemy);
        }
        else if (player.EquippedCards.Count == 0 && enemy.EquippedCards.Count > 0)
        {
            rollTypePlayer = RollTypeEnum.Disadvantage;
            rollTypeEnemy = RollTypeEnum.PlayerAttack;

            SetRollTypeForClientClientRpc(rollTypePlayer, clientRpcParamsPlayer);
            SetRollTypeForClientClientRpc(rollTypeEnemy, clientRpcParamsEnemy);
        }
        else
        {
            rollTypePlayer = RollTypeEnum.PlayerAttack;
            rollTypeEnemy = RollTypeEnum.Disadvantage;

            SetRollTypeForClientClientRpc(rollTypePlayer, clientRpcParamsPlayer);
            SetRollTypeForClientClientRpc(rollTypeEnemy, clientRpcParamsEnemy);
        }

        playerWinsNeeded = enemy.RollsNeededToLose + cardAbilities.GetPlayerGameModifier(enemy);
        enemyWinsNeeded = player.RollsNeededToLose + cardAbilities.GetPlayerGameModifier(player);

        string message = SendCurrentBattleResultMessage(player, playerWins, string.Empty, playerWinsNeeded);
        SetBattleResultInfoClientRpc(message);

        PlayerBattleSetClientRpc(clientRpcParamsBattle);
    }

    [ClientRpc]
    private void PlayerBattleSetClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleSet?.Invoke();
    }

    private void SetClientRpcParamsForBattle()
    {
        ulong[] clientIdsBattle = new ulong[clientRolled.Count];
        ulong[] clientIdsOther = new ulong[NetworkManager.ConnectedClientsIds.Count - clientRolled.Count];
        int i = 0;
        int j = 0;

        foreach (var item in NetworkManager.ConnectedClientsIds)
        {
            if (clientRolled.Keys.Contains(item))
            {
                clientIdsBattle[i] = item;
                i++;
            }
            else
            {
                clientIdsOther[j] = item;
                j++;
            }
        }

        clientRpcParamsBattle = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIdsBattle
            }
        };

        clientRpcParamsOther = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIdsOther
            }
        };
    }

    [ClientRpc]
    private void SetBattleResultInfoClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleShowUI?.Invoke(message);
    }

    [ClientRpc]
    private void SetRollTypeForClientClientRpc(RollTypeEnum rollType, ClientRpcParams clientRpcParams = default)
    {
        RollType.rollType = rollType;

        if (rollType == RollTypeEnum.PlayerAttack)
        {
            OnPlayerBattleRollDie?.Invoke();
        }
        else if (rollType == RollTypeEnum.Disadvantage)
        {
            OnPlayerBattleRollDisadvantage?.Invoke();
        }
    }

    public void SetResult(int result, RollTypeEnum rollTypeEnum)
    {
        if (rollTypeEnum != RollTypeEnum.PlayerAttack && rollTypeEnum != RollTypeEnum.Disadvantage) return;

        SetResultServerRpc(result);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        SetBattleRollResult(result, serverRpcParams);

        bool allRolled = CheckIfBattleRollsOver();

        if (!allRolled) return;

        ResetClientRolledValues();

        bool canContinue = SetBattleResultOrReroll();

        if (!canContinue) return;

        DetermineBattleWinnerOrReroll();
    }

    private void SetBattleRollResult(int result, ServerRpcParams serverRpcParams)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (!battleRolls.ContainsKey(clientId))
        {
            battleRolls.Add(clientId, new List<int> { result });
        }
        else
        {
            battleRolls[clientId].Add(result);
        }

        clientRolled[clientId] = true;
    }

    private bool CheckIfBattleRollsOver()
    {
        foreach (KeyValuePair<ulong, bool> item in clientRolled)
        {
            if (item.Value == false) return false;
        }

        return true;
    }

    private void ResetClientRolledValues()
    {
        foreach (ulong clientId in clientRolled.Keys.ToList())
        {
            clientRolled[clientId] = false;
        }
    }

    private bool SetBattleResultOrReroll()
    {
        KeyValuePair<ulong, List<int>> playerEntries = battleRolls.FirstOrDefault(a => a.Key == player.ClientId.Value);
        KeyValuePair<ulong, List<int>> enemyEntries = battleRolls.FirstOrDefault(a => a.Key == enemy.ClientId.Value);

        int playerResult = playerEntries.Value.LastOrDefault();
        int enemyResult = enemyEntries.Value.LastOrDefault();

        if (playerResult < enemyResult)
        {
            enemyWins++;
        }
        else if (playerResult > enemyResult)
        {
            playerWins++;
        }
        else
        {
            CallOnBattleReRollServerRpc();
            return false;
        }

        return true;
    }

    private void DetermineBattleWinnerOrReroll()
    {
        if (playerWins == playerWinsNeeded || enemyWins == enemyWinsNeeded)
        {
            SetRollTypeForClientBattleOverClientRpc(clientRpcParamsBattle);

            string message = string.Empty;

            if (playerWins == playerWinsNeeded)
            {
                message = SendBattleWinnerMessageToMessageUI(player, enemy, playerWins, enemyWins);

                if (enemy.EquippedCards.Count > 0)
                {
                    CallOnBattleWonResolveClientRpc(enemy.NetworkObject, clientRpcParamsPlayer);
                    afterBattleResolved[player.ClientId.Value] = false;
                    afterBattleResolved[enemy.ClientId.Value] = false;
                }
                else
                {
                    afterBattleResolved[enemy.ClientId.Value] = false;
                }

                CallOnBattleLostResolveClientRpc(clientRpcParamsEnemy);
            }
            else if (enemyWins == enemyWinsNeeded)
            {
                message = SendBattleWinnerMessageToMessageUI(enemy, player, enemyWins, playerWins);

                if (player.EquippedCards.Count > 0)
                {
                    CallOnBattleWonResolveClientRpc(player.NetworkObject, clientRpcParamsEnemy);
                    afterBattleResolved[player.ClientId.Value] = false;
                    afterBattleResolved[enemy.ClientId.Value] = false;
                }
                else
                {
                    afterBattleResolved[player.ClientId.Value] = false;
                }

                CallOnBattleLostResolveClientRpc(clientRpcParamsPlayer);
            }

            CallOnPlayerBattleRollOver(message);
        }
        else
        {
            CallOnBattleReRollServerRpc();
        }
    }

    [ClientRpc]
    private void SetRollTypeForClientBattleOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleRollDieOver?.Invoke();
    }


    [ServerRpc(RequireOwnership = false)]
    private void CallOnBattleReRollServerRpc(ServerRpcParams serverRpcParams = default)
    {
        string message = SendCurrentBattleResultMessage(player, playerWins, string.Empty, playerWinsNeeded);
        CallOnBattleReRoll(message);
        CallOnBattleShowUIClientRpc(message, clientRpcParamsOther);
    }

    private void CallOnBattleReRoll(string message)
    {
        CallOnBattleShowUIClientRpc(message, clientRpcParamsBattle);

        SetRollTypeForClientClientRpc(rollTypePlayer, clientRpcParamsPlayer);
        SetRollTypeForClientClientRpc(rollTypeEnemy, clientRpcParamsEnemy);
    }

    [ClientRpc]
    private void CallOnBattleShowUIClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleShowUI?.Invoke(message);
    }

    private string SendDisarmMessage(Player player)
    {
        return $"<color=#{player.HexPlayerColor}>{player.PlayerName}'s</color> CURRENTLY USING DISARM";
    }

    private string SendCurrentBattleResultMessage(Player player, int winCount, string message, int battlesNeeded)
    {
        if (message == string.Empty)
        {
            message = "Current battle result:\n";
        }

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        message += $"<color=#{playerColor}>{playerName}</color>: {winCount}/{battlesNeeded}";

        if (player == enemy) return message;

        message += "\n";

        return SendCurrentBattleResultMessage(enemy, enemyWins, message, enemyWinsNeeded);
    }

    private void CallOnPlayerBattleRollOver(string message, ClientRpcParams clientRpcParams = default)
    {
        CardAbilities.ResetRerolls();

        CallOnPlayerBattleRollOverClientRpc(message);
    }

    [ClientRpc]
    private void CallOnBattleWonResolveClientRpc(NetworkObjectReference loserNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player loser = Player.GetPlayerFromNetworkReference(loserNetworkObjectReference);

        OnBattleWin?.Invoke(loser);
    }

    [ClientRpc]
    private void CallOnBattleLostResolveClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnBattleLost?.Invoke();
    }

    [ClientRpc]
    private void CallOnPlayerBattleRollOverClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleRollOver?.Invoke();
        MessageUI.Instance.SetMessage(message);
    }

    [ClientRpc]
    private void CallOnAfterBattleResolvedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GridManager.Instance.GetGridPositionsWherePlayerCanInteract();

        OnAfterBattleResolved?.Invoke();
    }

    private string SendBattleWinnerMessageToMessageUI(Player winner, Player enemy, int wins, int loses)
    {
        return $"<color=#{winner.HexPlayerColor}>{winner.PlayerName}</color> wins the battle with {wins} wins against " +
               $"<color=#{enemy.HexPlayerColor}>{enemy.PlayerName}'s</color> {loses} wins";
    }

    public static void ResetStaticData()
    {
        OnPlayerBattleRollDie = null;
        OnPlayerBattleRollDieOver = null;
        OnPlayerBattleRollDisadvantage = null;
        OnPlayerBattleShowUI = null;
        OnPlayerBattleRollOver = null;
        OnBattleWin = null;
        OnBattleLost = null;
        OnAfterBattleResolved = null;
        OnPrebattle = null;
        OnPlayerBattleSet = null;
    }
}

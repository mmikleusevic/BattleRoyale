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

    public class OnBattleRollOverEventArgs : EventArgs
    {
        public string message;
    }

    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<ulong, List<int>> battleRolls;
    private Dictionary<ulong, bool> afterBattleResolved;
    private Player player1;
    private Player player2;

    private ClientRpcParams clientRpcParamsBattle;
    private ClientRpcParams clientRpcParamsOther;
    private ClientRpcParams clientRpcParamsPlayer1;
    private ClientRpcParams clientRpcParamsPlayer2;

    private RollTypeEnum rollTypePlayer1;
    private RollTypeEnum rollTypePlayer2;

    private int player1BattlesNeeded = 0;
    private int player2BattlesNeeded = 0;
    private int player1Wins = 0;
    private int player2Wins = 0;

    private void Awake()
    {
        PlayerInfoUI.OnAttackPlayer += PlayerInfoUI_OnAttackPlayerServerRpc;
        PlayerCardsEquippedUI.OnWonEquippedCard += ResolvePlayerBattleServerRpc;
        Player.OnPlayerSelectedPlaceToDie += ResolvePlayerBattleServerRpc;
    }

    private void Start()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();
        battleRolls = new Dictionary<ulong, List<int>>();
        afterBattleResolved = new Dictionary<ulong, bool>();
    }

    public override void OnDestroy()
    {
        PlayerInfoUI.OnAttackPlayer -= PlayerInfoUI_OnAttackPlayerServerRpc;
        PlayerCardsEquippedUI.OnWonEquippedCard -= ResolvePlayerBattleServerRpc;
        Player.OnPlayerSelectedPlaceToDie -= ResolvePlayerBattleServerRpc;

        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerInfoUI_OnAttackPlayerServerRpc(NetworkObjectReference arg1, NetworkObjectReference arg2)
    {
        Player player1 = Player.GetPlayerFromNetworkReference(arg1);
        Player player2 = Player.GetPlayerFromNetworkReference(arg2);

        SetPlayerBattle(player1, player2);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResolvePlayerBattleServerRpc(ulong clientId)
    {
        afterBattleResolved[clientId] = true;

        foreach (var item in afterBattleResolved)
        {
            if (item.Value == false) return;
        }

        CallOnAfterBattleResolvedClientRpc(clientRpcParamsPlayer1);
    }

    private void SetPlayerBattle(Player player, Player enemyPlayer)
    {
        player1 = player;
        player2 = enemyPlayer;

        clientRpcParamsPlayer1 = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { player.ClientId.Value },
            }
        };

        clientRpcParamsPlayer2 = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { enemyPlayer.ClientId.Value },
            }
        };

        player1BattlesNeeded = enemyPlayer.RollsNeededToLose;
        player2BattlesNeeded = player.RollsNeededToLose;

        clientRolled.Clear();
        battleRolls.Clear();
        afterBattleResolved.Clear();
        player1Wins = 0;
        player2Wins = 0;

        clientRolled[player.ClientId.Value] = false;
        clientRolled[enemyPlayer.ClientId.Value] = false;

        string message = SendCurrentBattleResultMessage(player1, player1Wins, string.Empty, player1BattlesNeeded);
        SetClientRpcParamsForBattle();
        SetBattleResultInfoClientRpc(message);

        if (player.EquippedCards.Count > 0 && enemyPlayer.EquippedCards.Count > 0 || player.EquippedCards.Count == 0 && enemyPlayer.EquippedCards.Count == 0)
        {
            rollTypePlayer1 = RollTypeEnum.PlayerAttack;
            rollTypePlayer2 = RollTypeEnum.PlayerAttack;

            SetRollTypeForClientClientRpc(rollTypePlayer1, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(rollTypePlayer2, clientRpcParamsPlayer2);
        }
        else if (player.EquippedCards.Count == 0 && enemyPlayer.EquippedCards.Count > 0)
        {
            rollTypePlayer1 = RollTypeEnum.Disadvantage;
            rollTypePlayer2 = RollTypeEnum.PlayerAttack;

            SetRollTypeForClientClientRpc(rollTypePlayer1, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(rollTypePlayer2, clientRpcParamsPlayer2);
        }
        else
        {
            rollTypePlayer1 = RollTypeEnum.PlayerAttack;
            rollTypePlayer2 = RollTypeEnum.Disadvantage;

            SetRollTypeForClientClientRpc(rollTypePlayer1, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(rollTypePlayer2, clientRpcParamsPlayer2);
        }
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
        KeyValuePair<ulong, List<int>> player1Entries = battleRolls.FirstOrDefault(a => a.Key == player1.ClientId.Value);
        KeyValuePair<ulong, List<int>> player2Entries = battleRolls.FirstOrDefault(a => a.Key == player2.ClientId.Value);

        int player1Result = player1Entries.Value.LastOrDefault();
        int player2Result = player2Entries.Value.LastOrDefault();

        if (player1Result < player2Result)
        {
            player2Wins++;
        }
        else if (player1Result > player2Result)
        {
            player1Wins++;
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
        if (player1Wins == player1BattlesNeeded || player2Wins == player2BattlesNeeded)
        {
            SetRollTypeForClientBattleOverClientRpc(clientRpcParamsBattle);

            ulong winnerId = 0;

            if (player1Wins == player1BattlesNeeded)
            {
                winnerId = player1.ClientId.Value;

                if (player2.EquippedCards.Count > 0)
                {
                    CallOnBattleWonResolveClientRpc(player2.NetworkObject, clientRpcParamsPlayer1);
                    afterBattleResolved[player1.ClientId.Value] = false;
                    afterBattleResolved[player2.ClientId.Value] = false;
                }
                else
                {
                    afterBattleResolved[player2.ClientId.Value] = false;
                }

                CallOnBattleLostResolveClientRpc(clientRpcParamsPlayer2);
            }
            else if (player2Wins == player2BattlesNeeded)
            {
                winnerId = player2.ClientId.Value;

                if (player1.EquippedCards.Count > 0)
                {
                    CallOnBattleWonResolveClientRpc(player1.NetworkObject, clientRpcParamsPlayer2);
                    afterBattleResolved[player1.ClientId.Value] = false;
                    afterBattleResolved[player2.ClientId.Value] = false;
                }
                else
                {
                    afterBattleResolved[player1.ClientId.Value] = false;
                }

                CallOnBattleLostResolveClientRpc(clientRpcParamsPlayer1);
            }

            CallOnPlayerBattleRollOver(winnerId);
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
        string message = SendCurrentBattleResultMessage(player1, player1Wins, string.Empty, player1BattlesNeeded);
        CallOnBattleReRoll(message);
        CallOnBattleShowUIClientRpc(message, clientRpcParamsOther);
    }

    private void CallOnBattleReRoll(string message)
    {
        CallOnBattleShowUIClientRpc(message, clientRpcParamsBattle);

        SetRollTypeForClientClientRpc(rollTypePlayer1, clientRpcParamsPlayer1);
        SetRollTypeForClientClientRpc(rollTypePlayer2, clientRpcParamsPlayer2);
    }

    [ClientRpc]
    private void CallOnBattleShowUIClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleShowUI?.Invoke(message);
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

        if (player == player2) return message;

        message += "\n";

        return SendCurrentBattleResultMessage(player2, player2Wins, message, player2BattlesNeeded);
    }

    private void CallOnPlayerBattleRollOver(ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        string message = SendBattleWinnerMessageToMessageUI(winnerId);

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

    private string SendBattleWinnerMessageToMessageUI(ulong winnerId)
    {
        Player player = PlayerManager.Instance.ActivePlayers.Where(a => a.ClientId.Value == winnerId).FirstOrDefault();

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return $"<color=#{playerColor}>{playerName}</color> wins the battle";
    }
}

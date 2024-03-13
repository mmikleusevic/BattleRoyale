using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerBattleResults : NetworkBehaviour
{
    public static event Action OnPlayerBattleRollDie;
    public static event Action OnPlayerBattleRollDieOver;
    public static event Action OnPlayerBattleRollDisadvantage;
    public static event Action OnPlayerBattleRollDisadvantageRollOver;
    public static event Action<string> OnPlayerBattleShowUI;
    public static event Action<OnBattleRollOverEventArgs> OnPlayerBattleRollOver;

    public class OnBattleRollOverEventArgs : EventArgs
    {
        public string message;
        public ulong winnerId;
    }

    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<ulong, List<int>> battleRolls;
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
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayerServerRpc;
    }

    private void Start()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();
        battleRolls = new Dictionary<ulong, List<int>>();
    }

    public override void OnDestroy()
    {
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayerServerRpc;

        base.OnDestroy();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackPlayerInfoUI_OnAttackPlayerServerRpc(NetworkObjectReference arg1, NetworkObjectReference arg2, string arg3)
    {
        Player player1 = Player.GetPlayerFromNetworkReference(arg1);
        Player player2 = Player.GetPlayerFromNetworkReference(arg2);

        SetPlayerBattle(player1, player2);
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

        player1BattlesNeeded = enemyPlayer.EnemyRollWinsToLose;
        player2BattlesNeeded = player.EnemyRollWinsToLose;

        clientRolled.Clear();
        battleRolls.Clear();
        player1Wins = 0;
        player2Wins = 0;

        clientRolled[player.ClientId.Value] = false;
        clientRolled[enemyPlayer.ClientId.Value] = false;

        string message = SendCurrentBattleResultMessage(player1, player1Wins, string.Empty);
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

    [ServerRpc(RequireOwnership = false)]
    public void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
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
        KeyValuePair<ulong, List<int>> client1Entries = battleRolls.FirstOrDefault();
        KeyValuePair<ulong, List<int>> client2Entries = battleRolls.LastOrDefault();

        int client1Result = client1Entries.Value.LastOrDefault();
        int client2Result = client2Entries.Value.LastOrDefault();

        if (client1Result < client2Result)
        {
            player2Wins++;
        }
        else if (client1Result > client2Result)
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
            ulong winnerId = 0;

            if (player1Wins == player1BattlesNeeded)
            {
                winnerId = battleRolls.FirstOrDefault().Key;
            }
            else if (player2Wins == player2BattlesNeeded)
            {
                winnerId = battleRolls.LastOrDefault().Key;
            }

            SetRollTypeForClientBattleOverClientRpc(rollTypePlayer1, clientRpcParamsPlayer1);
            SetRollTypeForClientBattleOverClientRpc(rollTypePlayer2, clientRpcParamsPlayer2);

            CallOnPlayerBattleRollOver(winnerId);
        }
        else
        {
            CallOnBattleReRollServerRpc();
        }
    }

    [ClientRpc]
    private void SetRollTypeForClientBattleOverClientRpc(RollTypeEnum rollType, ClientRpcParams clientRpcParams = default)
    {
        if (rollType == RollTypeEnum.PlayerAttack)
        {
            OnPlayerBattleRollDieOver?.Invoke();
        }
        else if (rollType == RollTypeEnum.Disadvantage)
        {
            OnPlayerBattleRollDisadvantageRollOver?.Invoke();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void CallOnBattleReRollServerRpc(ServerRpcParams serverRpcParams = default)
    {
        string message = SendCurrentBattleResultMessage(player1, player1Wins, string.Empty);
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

    private string SendCurrentBattleResultMessage(Player player, int winCount, string message)
    {
        if (message == string.Empty)
        {
            message = "Current battle result:\n";
        }

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        message += $"<color=#{playerColor}>{playerName}</color>: {winCount}";

        if (player == player2) return message;

        message += "\n";

        return SendCurrentBattleResultMessage(player2, player2Wins, message);
    }

    private void CallOnPlayerBattleRollOver(ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        string message = SendBattleWinnerMessageToMessageUI(winnerId);

        CallOnPlayerBattleRollOverClientRpc(message, winnerId);
    }

    [ClientRpc]
    private void CallOnPlayerBattleRollOverClientRpc(string message, ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        OnBattleRollOverEventArgs eventArgs = new OnBattleRollOverEventArgs
        {
            message = message,
            winnerId = winnerId
        };

        OnPlayerBattleRollOver?.Invoke(eventArgs);
    }

    private string SendBattleWinnerMessageToMessageUI(ulong clientId)
    {
        Player player = PlayerManager.Instance.Players.Where(a => a.ClientId.Value == clientId).FirstOrDefault();

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return $"<color=#{playerColor}>{playerName}</color> wins the battle";
    }
}

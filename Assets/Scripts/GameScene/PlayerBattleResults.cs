using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerBattleResults : NetworkBehaviour
{
    public static event EventHandler OnPlayerBattleRoll;
    public static event EventHandler OnPlayerBattleRollDisadvantage;
    public static event Action<string> OnPlayerBattleReRoll;
    public static event EventHandler OnPlayerBattlePrepared;
    public static event EventHandler<OnBattleRollOverEventArgs> OnPlayerBattleRollOver;

    public class OnBattleRollOverEventArgs : EventArgs
    {
        public string message;
        public ulong winnerId;
    }

    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<ulong, List<int>> battleRolls;
    private Dictionary<ulong, int> battleResults;

    private void Awake()
    {
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayerServerRpc;
    }

    private void Start()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();
        battleRolls = new Dictionary<ulong, List<int>>();
        battleResults = new Dictionary<ulong, int>();
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
        ClientRpcParams clientRpcParamsPlayer1 = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { player.ClientId.Value },
            }
        };

        ClientRpcParams clientRpcParamsPlayer2 = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new[] { enemyPlayer.ClientId.Value },
            }
        };

        if (player.EquippedCards.Count > 0 && enemyPlayer.EquippedCards.Count > 0 || player.EquippedCards.Count == 0 && enemyPlayer.EquippedCards.Count == 0)
        {
            SetRollTypeForClientClientRpc(RollTypeEnum.PlayerAttack, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(RollTypeEnum.PlayerAttack, clientRpcParamsPlayer2);
        }
        else if (player.EquippedCards.Count == 0 && enemyPlayer.EquippedCards.Count > 0)
        {
            SetRollTypeForClientClientRpc(RollTypeEnum.Disadvantage, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(RollTypeEnum.PlayerAttack, clientRpcParamsPlayer2);
        }
        else
        {
            SetRollTypeForClientClientRpc(RollTypeEnum.PlayerAttack, clientRpcParamsPlayer1);
            SetRollTypeForClientClientRpc(RollTypeEnum.Disadvantage, clientRpcParamsPlayer2);
        }

        clientRolled[player.ClientId.Value] = false;
        clientRolled[enemyPlayer.ClientId.Value] = false;
    }

    [ClientRpc]
    private void SetRollTypeForClientClientRpc(RollTypeEnum rollType, ClientRpcParams clientRpcParams = default)
    {
        RollType.rollType = rollType;

        if (rollType == RollTypeEnum.PlayerAttack)
        {
            OnPlayerBattleRoll?.Invoke(this, EventArgs.Empty);
        }
        else if (rollType == RollTypeEnum.Disadvantage)
        {
            OnPlayerBattleRollDisadvantage?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        SetBattleRollResult(result, serverRpcParams);

        bool allRolled = CheckIfBattleRollsOver();

        if (!allRolled) return;

        ResetClientRolledValues();

        ClientRpcParams clientRpcParams = SetClientRpcParamsForBattle();

        SetBattleResultOrReroll(clientRpcParams);

        DetermineBattleWinnerOrReroll(clientRpcParams);
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

    private ClientRpcParams SetClientRpcParamsForBattle()
    {
        ulong[] clientIdsForReRoll = new ulong[battleResults.Count];
        int i = 0;

        foreach (var item in battleResults)
        {
            clientIdsForReRoll[i] = item.Key;
            i++;
        }

        return new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIdsForReRoll
            }
        };
    }

    private void SetBattleResultOrReroll(ClientRpcParams clientRpcParams)
    {
        int client1Result = battleRolls.FirstOrDefault().Value.LastOrDefault();
        int client2Result = battleRolls.LastOrDefault().Value.LastOrDefault();

        if (client1Result < client2Result)
        {
            battleResults[battleResults.LastOrDefault().Key] = battleResults.LastOrDefault().Value + 1;
        }
        else if (client1Result > client2Result)
        {
            battleResults[battleResults.FirstOrDefault().Key] = battleResults.FirstOrDefault().Value + 1;
        }
        else
        {
            CallOnBattleReRollClientRpc(clientRpcParams);
        }
    }

    private void DetermineBattleWinnerOrReroll(ClientRpcParams clientRpcParams)
    {
        //check how to implement shield
        if (battleResults.Any(a => a.Value == 3))
        {
            clientRolled.Clear();

            ulong winnerId = battleResults.FirstOrDefault(a => a.Value == 3).Key;
            SetBattleResultClientRpc(winnerId, clientRpcParams);
        }
        else
        {
            CallOnBattleReRollClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void CallOnBattleReRollClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnPlayerBattleReRoll?.Invoke(SendBattleBattleRerollMessageToMessageUI());
    }

    [ClientRpc]
    private void SetBattleResultClientRpc(ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        CallOnBattleRollOver(winnerId, clientRpcParams);
    }

    private string SendBattleBattleRerollMessageToMessageUI()
    {
        string message = "Current battle Result:'\n'";

        foreach (KeyValuePair<ulong, int> item in battleResults)
        {
            Player player = PlayerManager.Instance.Players.Where(a => a.ClientId.Value == item.Key).FirstOrDefault();

            string playerName = player.PlayerName;
            string playerColor = player.HexPlayerColor;

            message += $"<color=#{playerColor}>{playerName}</color>: {item.Value}";
        }

        return message;
    }

    private void CallOnBattleRollOver(ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        string message = SendBattleWinnerMessageToMessageUI(winnerId);

        CallOnBattleRollOverClientRpc(winnerId, message, clientRpcParams);
    }

    private string SendBattleWinnerMessageToMessageUI(ulong clientId)
    {
        Player player = PlayerManager.Instance.Players.Where(a => a.ClientId.Value == clientId).FirstOrDefault();

        string playerName = player.PlayerName;
        string playerColor = player.HexPlayerColor;

        return $"The winner of the battle is: <color=#{playerColor}>{playerName}</color>";
    }

    [ClientRpc]
    private void CallOnBattleRollOverClientRpc(ulong winnerId, string message, ClientRpcParams clientRpcParams = default)
    {
        OnBattleRollOverEventArgs eventArgs = new OnBattleRollOverEventArgs
        {
            message = message,
            winnerId = winnerId
        };

        OnPlayerBattleRollOver?.Invoke(this, eventArgs);
    }
}

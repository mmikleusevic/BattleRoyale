using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    public static event EventHandler OnPlayerBattleRoll;
    public static event EventHandler OnPlayerBattleRollDisadvantage;
    public static event EventHandler OnReRoll;
    public static event EventHandler OnBattlePrepared;
    public static event EventHandler<OnInitiativeRollOverEventArgs> OnInitiativeRollOver;
    public static event EventHandler<OnBattleRollOverEventArgs> OnBattleRollOver;
    public static event EventHandler<string> OnCardRollOver;

    public class OnInitiativeRollOverEventArgs : EventArgs
    {
        public string message;
        public List<ulong> playerOrder;
    }

    public class OnBattleRollOverEventArgs : EventArgs
    {
        public string message;
        public ulong winnerId;
    }

    private Dictionary<ulong, List<int>> battleRolls;
    private Dictionary<ulong, int> battleResults;
    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<int, List<ulong>> rollResults;
    private Dictionary<ulong, List<int>> clientInitiative;
    private List<List<ulong>> clientsToReRollList;
    private List<ulong> finalOrder;

    private void Awake()
    {
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayerServerRpc;
    }

    private void Start()
    {
        finalOrder = new List<ulong>();

        if (!IsServer) return;

        battleRolls = new Dictionary<ulong, List<int>>();
        battleResults = new Dictionary<ulong, int>();
        clientRolled = new Dictionary<ulong, bool>();
        rollResults = new Dictionary<int, List<ulong>>();
        clientInitiative = new Dictionary<ulong, List<int>>();
        clientsToReRollList = new List<List<ulong>>();

        SetClientIdToDictionary();
    }

    public override void OnDestroy()
    {
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayerServerRpc;

        base.OnDestroy();
    }

    public void SetRollResults(int result, RollTypeEnum rollType)
    {
        switch (rollType)
        {
            case RollTypeEnum.Initiative:
                SetInitiativeResultServerRpc(result);
                break;
            case RollTypeEnum.PlayerAttack:
            case RollTypeEnum.Disadvantage:
                //TODO finish
                SetBattleResultServerRpc(result);
                break;
            case RollTypeEnum.CardAttack:
                // TODO
                break;
        }
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
    private void SetInitiativeResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        SetInitiativeResult(result, clientId);

        List<ulong> clientIdsForReRoll = CheckIfReRollNeeded();

        if (clientIdsForReRoll == null) return;

        FinishClientInitiativeOrReRoll(clientIdsForReRoll);
    }

    private void SetClientIdToDictionary()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!clientRolled.ContainsKey(clientId))
            {
                clientRolled.Add(clientId, false);
            }
        }
    }

    private void SetInitiativeResult(int result, ulong clientId)
    {
        if (rollResults.ContainsKey(result))
        {
            rollResults[result].Add(clientId);
        }
        else
        {
            rollResults[result] = new List<ulong>() { clientId };
        }

        clientRolled[clientId] = true;
    }

    private List<ulong> CheckIfReRollNeeded()
    {
        bool everyoneRolled = CheckIfEveryoneRolled();

        List<ulong> clientIdsForReRoll = new List<ulong>();

        if (!everyoneRolled) return null;

        return CheckRolls(clientIdsForReRoll);
    }

    private bool CheckIfEveryoneRolled()
    {
        foreach (KeyValuePair<ulong, bool> clientRolledPair in clientRolled)
        {
            if (!clientRolledPair.Value)
            {
                return false;
            }
        }

        return true;
    }

    private List<ulong> CheckRolls(List<ulong> clientIdsForReRoll)
    {
        foreach (KeyValuePair<int, List<ulong>> playerInitiative in rollResults.OrderByDescending(a => a.Key))
        {
            if (playerInitiative.Value.Count > 1)
            {
                var compareList = new List<ulong>();

                foreach (ulong clientId in playerInitiative.Value)
                {
                    AddToClientInitative(clientId, playerInitiative);

                    compareList.Add(clientId);
                    clientIdsForReRoll.Add(clientId);
                    clientRolled[clientId] = false;
                }

                clientsToReRollList.Add(compareList);
            }
            else
            {
                ulong clientId = playerInitiative.Value[0];

                AddToClientInitative(clientId, playerInitiative);
            }
        }

        rollResults.Clear();

        OrderClientInitiative();

        return clientIdsForReRoll;
    }

    private void AddToClientInitative(ulong clientId, KeyValuePair<int, List<ulong>> playerInitiative)
    {
        if (clientInitiative.ContainsKey(clientId))
        {
            clientInitiative[clientId].Add(playerInitiative.Key);
        }
        else
        {
            clientInitiative[clientId] = new List<int> { playerInitiative.Key };
        }

        if (finalOrder.Count != clientInitiative.Keys.Count)
        {
            finalOrder.Add(clientId);
        }
    }

    private void OrderClientInitiative()
    {
        if (clientsToReRollList == null) return;

        foreach (List<ulong> clientsToReRoll in clientsToReRollList.ToList())
        {
            Dictionary<int, List<ulong>> clientsToReOrder = new Dictionary<int, List<ulong>>();

            int firstIndex = finalOrder.IndexOf(clientsToReRoll[0]);
            int lastValueOfFirstIndex = clientInitiative[clientsToReRoll[0]].Last();

            clientInitiative.OrderByDescending(a => a.Value).ThenBy(a => a.Value.Last());

            SetDictionaryValuesOfClientsToReOrder(clientsToReOrder, clientsToReRoll);

            KeyValuePair<int, List<ulong>>[] clientsResults = clientsToReOrder.OrderByDescending(a => a.Key).Where(a => a.Value.Count == 1).ToArray();

            SwapPlacesIfValueLargerAndRemoveFromList(clientsResults, clientsToReRoll, lastValueOfFirstIndex, firstIndex);

            if (!clientsToReRoll.Any()) clientsToReRollList.Remove(clientsToReRoll);
        }

        if (!clientsToReRollList.Any()) clientsToReRollList = null;
    }

    private void SetDictionaryValuesOfClientsToReOrder(Dictionary<int, List<ulong>> clientsToReOrder, List<ulong> clientsToReRoll)
    {
        foreach (ulong clientId in clientsToReRoll)
        {
            int result = clientInitiative[clientId].Last();

            if (clientsToReOrder.ContainsKey(result))
            {
                clientsToReOrder[result].Add(clientId);
            }
            else
            {
                clientsToReOrder[result] = new List<ulong>() { clientId };
            }
        }
    }

    private void SwapPlacesIfValueLargerAndRemoveFromList(KeyValuePair<int, List<ulong>>[] clientsResults, List<ulong> clientsToReRoll, int lastValueOfFirstIndex, int firstIndex)
    {
        foreach (KeyValuePair<int, List<ulong>> clientResult in clientsResults)
        {
            ulong clientIdToReOrder = clientResult.Value.FirstOrDefault();
            int indexToSwap = finalOrder.IndexOf(clientIdToReOrder);
            int lastValueOfIndexToSwap = clientInitiative[clientIdToReOrder].Last();

            if (lastValueOfIndexToSwap > lastValueOfFirstIndex)
            {
                ulong temp = finalOrder[firstIndex];
                finalOrder[firstIndex] = clientIdToReOrder;
                finalOrder[indexToSwap] = temp;

                firstIndex = indexToSwap;
                lastValueOfFirstIndex = lastValueOfIndexToSwap;
            }

            clientsToReRoll.Remove(clientIdToReOrder);
        }
    }

    private void FinishClientInitiativeOrReRoll(List<ulong> clientIdsForReRoll)
    {
        if (clientIdsForReRoll.Any())
        {
            ulong[] clientIdsForReRollArray = clientIdsForReRoll.ToArray();

            CallOnOnReRollServerRpc(clientIdsForReRollArray);
        }
        else if (clientIdsForReRoll.Count == 0)
        {
            clientRolled.Clear();
            StartCoroutine(SetFinalOrderOnClients());
        }
    }

    private IEnumerator SetFinalOrderOnClients()
    {
        foreach (ulong clientId in finalOrder)
        {
            SetFinalOrderOnClientsClientRpc(clientId);
        }

        yield return new WaitForEndOfFrame();

        CallOnInitiativeRollOver();
    }

    [ClientRpc]
    private void SetFinalOrderOnClientsClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        if (IsServer) return;

        finalOrder.Add(clientId);
    }

    private string SendFinalOrderToMessageUI()
    {
        string message = "Final order is: " + '\n';

        for (int i = 0; i < finalOrder.Count; i++)
        {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(finalOrder[i]);
            string colorString = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId).ToHexString();

            message += $"<color=#{colorString}> {i + 1}. {playerData.playerName}</color>";

            if (i < finalOrder.Count - 1) message += '\n';
        }

        return message;
    }

    [ServerRpc]
    private void CallOnOnReRollServerRpc(ulong[] clientIdsForReRoll, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIdsForReRoll
            }
        };

        CallOnReRollClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void CallOnReRollClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnReRoll?.Invoke(this, EventArgs.Empty);
    }

    private void CallOnInitiativeRollOver()
    {
        string message = SendFinalOrderToMessageUI();

        CallOnInitiativeRollOverClientRpc(message);
    }

    [ClientRpc]
    private void CallOnInitiativeRollOverClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        OnInitiativeRollOverEventArgs eventArgs = new OnInitiativeRollOverEventArgs
        {
            message = message,
            playerOrder = finalOrder
        };

        OnInitiativeRollOver?.Invoke(this, eventArgs);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
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
        foreach (ulong item in clientRolled.Keys)
        {
            clientRolled[item] = false;
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
            CallOnReRollClientRpc(clientRpcParams);
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
            CallOnReRollClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    private void SetBattleResultClientRpc(ulong winnerId, ClientRpcParams clientRpcParams = default)
    {
        CallOnBattleRollOver(winnerId, clientRpcParams);
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

        OnBattleRollOver?.Invoke(this, eventArgs);
    }
}
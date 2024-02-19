using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    public static event EventHandler OnReRoll;
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

    private string SendToMessageUI()
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
        string message = SendToMessageUI();

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
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (!battleRolls.ContainsKey(clientId))
        {
            battleRolls.Add(clientId, new List<int> { result });
        }
        else
        {
            battleRolls[clientId].Add(result);
        }

        //Need to write into it to know who is battling
        clientRolled[clientId] = true;

        foreach (var item in clientRolled)
        {
            if (item.Value == false) return;
        }

        clientRolled.Clear();

        int client1Result = battleRolls.FirstOrDefault().Value.LastOrDefault();
        int client2Result = battleRolls.LastOrDefault().Value.LastOrDefault();

        //refactor to not call it more than once
        ulong[] clientIdsForReRoll = new ulong[battleResults.Count];

        int i = 0;
        foreach (var item in battleResults)
        {
            clientIdsForReRoll[i] = item.Key;
            i++;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIdsForReRoll
            }
        };

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

        //check how to implement shield
        if (battleResults.Any(a => a.Value == 3))
        {
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
        string message = SendToMessageUI();

        CallOnBattleRollOverClientRpc(winnerId, message, clientRpcParams);
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
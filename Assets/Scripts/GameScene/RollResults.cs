using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    public static event EventHandler OnReRoll;
    public static event EventHandler OnInitiativeRollOver;

    private Dictionary<ulong, bool> clientRolled;
    private Dictionary<int, List<ulong>> rollResults;
    private Dictionary<ulong, List<int>> clientInitiative;
    private List<List<ulong>> clientsToReRollList;
    private List<ulong> finalOrder;

    private bool rollForInitiative;

    private void Awake()
    {
        rollForInitiative = true;
    }

    private void Start()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();
        rollResults = new Dictionary<int, List<ulong>>();
        clientInitiative = new Dictionary<ulong, List<int>>();
        clientsToReRollList = new List<List<ulong>>();
        finalOrder = new List<ulong>();

        SetClientIdToDictionary();
    }

    public void SetRollResults(int result)
    {
        if (rollForInitiative)
        {
            SetInitiativeResultServerRpc(result);
        }
        else
        {
            SetBattleResultServerRpc(result);
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
            Dictionary<int, List<ulong>> clientsReOrder = new Dictionary<int, List<ulong>>();

            int indexToSwap = finalOrder.IndexOf(clientsToReRoll[0]);
            int resultOfIndexToSwap = clientInitiative[clientsToReRoll[0]].Last();

            clientInitiative.OrderByDescending(a => a.Value).ThenBy(a => a.Value.Last());

            foreach (ulong clientId in clientsToReRoll)
            {
                int result = clientInitiative[clientId].Last();

                if (clientsReOrder.ContainsKey(result))
                {
                    clientsReOrder[result].Add(clientId);
                }
                else
                {
                    clientsReOrder[result] = new List<ulong>() { clientId };
                }
            }

            KeyValuePair<int, List<ulong>>[] clientsResults = clientsReOrder.OrderByDescending(a => a.Key).Where(a => a.Value.Count == 1).ToArray();

            foreach (KeyValuePair<int, List<ulong>> clientResult in clientsResults)
            {
                ulong clientIdToReOrder = clientResult.Value.FirstOrDefault();
                int newIndexToSwap = finalOrder.IndexOf(clientIdToReOrder);
                int resultOfNewIndexToSwap = clientInitiative[clientIdToReOrder].Last();

                if (resultOfNewIndexToSwap > resultOfIndexToSwap)
                {
                    ulong temp = finalOrder[indexToSwap];
                    finalOrder[indexToSwap] = clientIdToReOrder;
                    finalOrder[newIndexToSwap] = temp;

                    indexToSwap = newIndexToSwap;
                    resultOfIndexToSwap = resultOfNewIndexToSwap;
                }

                clientsToReRoll.Remove(clientIdToReOrder);
            }

            if (!clientsToReRoll.Any()) clientsToReRollList.Remove(clientsToReRoll);
        }

        foreach (var item in finalOrder)
        {
            Debug.Log("clientId:" + item);
        }

        if (!clientsToReRollList.Any()) clientsToReRollList = null;
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
            rollForInitiative = false;

            CallOnInitiativeRollOverServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
    private void CallOnInitiativeRollOverServerRpc(ServerRpcParams serverRpcParams = default)
    {
        CallOnInitiativeRollOverClientRpc();
    }

    [ClientRpc]
    private void CallOnInitiativeRollOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnInitiativeRollOver?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        SetBattleResultClientRpc(result);
    }

    [ClientRpc]
    private void SetBattleResultClientRpc(int result)
    {
        // Handle setting battle results on the client
    }
}
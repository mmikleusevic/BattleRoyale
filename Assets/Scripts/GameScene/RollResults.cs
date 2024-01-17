using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    public static event EventHandler OnInitiativeRollOver;

    private NetworkList<ClientRolled> clientRolledNetworkList;
    private Dictionary<int, List<ulong>> playerInitiativeOrder;
    private Dictionary<int, List<ulong>> finalInitiativeList;

    private bool rollForInitiative;

    private void Awake()
    {
        rollForInitiative = true;

        clientRolledNetworkList = new NetworkList<ClientRolled>();
        playerInitiativeOrder = new Dictionary<int, List<ulong>>();
        finalInitiativeList = new Dictionary<int, List<ulong>>();
    }

    private void Start()
    {
        SetClientIdToDictionary();
    }

    private void SetClientIdToDictionary()
    {
        if (IsServer)

        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            clientRolledNetworkList.Add(new ClientRolled
            {
                clientId = clientId,
                rolled = false,
            });
        }
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

        Debug.Log("clientId: " + clientId);

        SetInitiativeResultClientRpc(result, clientId);

        int clientRolledDataIndex = GetRolledDataIndexFromClientId(clientId);

        SetClientRolledTo(clientRolledDataIndex, true);

        List<ulong> clientIdsForReRoll = CheckIfReRollNeeded();

        FinishInitiativeRollOrReRoll(clientIdsForReRoll);
    }

    [ClientRpc]
    private void SetInitiativeResultClientRpc(int result, ulong clientId)
    {
        if (playerInitiativeOrder.ContainsKey(result))
        {
            playerInitiativeOrder[result].Add(clientId);
        }
        else
        {
            playerInitiativeOrder[result] = new List<ulong>() { clientId };
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        SetBattleResultClientRpc(result);
    }

    [ClientRpc]
    private void SetBattleResultClientRpc(int result)
    {

    }

    private int GetRolledDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < clientRolledNetworkList.Count; i++)
        {
            if (clientRolledNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    private void SetClientRolledTo(int clientRolledDataIndex, bool rolled)
    {
        ClientRolled clientRolled = clientRolledNetworkList[clientRolledDataIndex];

        clientRolled.rolled = rolled;

        clientRolledNetworkList[clientRolledDataIndex] = clientRolled;
    }

    private List<ulong> CheckIfReRollNeeded()
    {
        bool allClientsRolled = true;

        foreach (ClientRolled clientRolled in clientRolledNetworkList)
        {
            if (clientRolled.rolled != true)
            {
                allClientsRolled = false;
                break;
            }
        }

        List<ulong> clientIdsForReRoll = new List<ulong>();

        if (allClientsRolled)
        {
            playerInitiativeOrder = (Dictionary<int, List<ulong>>)playerInitiativeOrder.OrderByDescending(a => a.Key);

            foreach (KeyValuePair<int, List<ulong>> playerInitiative in playerInitiativeOrder)
            {
                if (finalInitiativeList[playerInitiative.Key] == null)
                {
                    finalInitiativeList[playerInitiative.Key] = playerInitiative.Value;
                }

                if (playerInitiative.Value.Count > 1)
                {
                    foreach (ulong clientId in playerInitiative.Value)
                    {
                        clientIdsForReRoll.Add(clientId);

                        int clientRolledDataIndex = GetRolledDataIndexFromClientId(clientId);

                        SetClientRolledTo(clientRolledDataIndex, false);

                        playerInitiativeOrder[playerInitiative.Key].Remove(clientId);
                    }
                }
                else
                {
                    playerInitiativeOrder[playerInitiative.Key].Remove(playerInitiative.Value[0]);
                }
            }

            //TODO make final order in finalInitiativeList
        }

        return clientIdsForReRoll;
    }

    private void FinishInitiativeRollOrReRoll(List<ulong> clientIdsForReRoll)
    {
        if (clientIdsForReRoll.Any())
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clientIdsForReRoll
                }
            };

            // TODO add Show() Dice for ReRoll
        }
        else
        {
            rollForInitiative = false;

            finalInitiativeList.OrderByDescending(a => a.Value);

            OnInitiativeRollOver?.Invoke(this, EventArgs.Empty);
        }
    }
}

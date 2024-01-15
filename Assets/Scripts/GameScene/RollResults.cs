using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RollResults : NetworkBehaviour, IRollResults
{
    private Dictionary<ulong, int> playerInitiativeOrder;

    private bool rollForInitiative;

    private void Awake()
    {
        rollForInitiative = true;

        playerInitiativeOrder = new Dictionary<ulong, int>();
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
    }

    [ClientRpc]
    private void SetInitiativeResultClientRpc(int result, ulong clientId)
    {
        if (playerInitiativeOrder.ContainsKey(clientId))
        {
            playerInitiativeOrder[clientId] = result;
        }
        else
        {
            playerInitiativeOrder.Add(clientId, result);
        }

        Debug.Log("playerInitiativeOrder count: " + playerInitiativeOrder.Count);

        foreach (var a in playerInitiativeOrder)
        {
            Debug.Log(a);
        }
    }

    //TODO need to implement

    [ServerRpc(RequireOwnership = false)]
    private void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        SetBattleResultClientRpc(result);
    }

    [ClientRpc]
    private void SetBattleResultClientRpc(int result)
    {

    }
}

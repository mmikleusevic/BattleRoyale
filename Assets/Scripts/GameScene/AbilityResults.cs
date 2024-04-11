using System;
using System.Collections.Generic;
using Unity.Netcode;

public class AbilityResults : NetworkBehaviour, IResult
{
    public static event Action OnStartRoll;
    public static event Action OnEndRoll;
    public static event Action OnDisableEndTurnButton;
    public static event Action OnEnableEndTurnButton;

    private Dictionary<ulong, bool> clientRolled;
    private ClientRpcParams callerRpcParams;
    private ClientRpcParams rollingClientsRpcParams;

    private int resultGoal;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        clientRolled = new Dictionary<ulong, bool>();

        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRollOnClientsServerRpc(ulong[] clients, int goal, ServerRpcParams serverRpcParams = default)
    {
        clientRolled.Clear();

        ulong[] callerId = new ulong[] { serverRpcParams.Receive.SenderClientId };

        callerRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = callerId
            }
        };

        if (clients.Length == 0)
        {
            EnableEndTurnButtonClientRpc(callerRpcParams);
            return;
        }

        resultGoal = goal;

        rollingClientsRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clients
            }
        };

        foreach (ulong clientId in clients)
        {
            clientRolled.Add(clientId, false);
        }

        DisableEndTurnButtonClientRpc(callerRpcParams);
        SetRollOnClientsClientRpc(rollingClientsRpcParams);
    }

    [ClientRpc]
    private void DisableEndTurnButtonClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnDisableEndTurnButton?.Invoke();
        GridManager.Instance.DisableCards();
    }

    [ClientRpc]
    private void SetRollOnClientsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        string[] messages = CreateOnRollStartedMessage();

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnRollStartedMessage());
        FadeMessageUI.Instance.KeepMessage(messages[0]);

        RollType.rollType = RollTypeEnum.Ability;

        OnStartRoll?.Invoke();
    }

    public void SetResult(int result, RollTypeEnum rollTypeEnum)
    {
        if (rollTypeEnum != RollTypeEnum.Ability) return;

        SetResultServerRpc(result);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        clientRolled[clientId] = true;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };

        if (result <= resultGoal)
        {
            RollFailedLogicClientRpc(clientRpcParams);
        }
        else
        {
            RollSucceededLogicClientRpc(clientRpcParams);
        }

        bool allRolled = CheckIfAllRolled();

        if (allRolled)
        {
            EnableEndTurnButtonClientRpc(callerRpcParams);
            EndRollClientRpc(rollingClientsRpcParams);
        }
    }

    [ClientRpc]
    private void RollFailedLogicClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Player.LocalInstance.PlayerDiedAbilityCheck();

        string[] messages = CreateOnRollFailedMessage();

        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
    }

    [ClientRpc]
    private void RollSucceededLogicClientRpc(ClientRpcParams clientRpcParams = default)
    {
        string[] messages = CreateOnRollSucceededMessage();

        FadeMessageUI.Instance.StartFadeMessage(messages[0]);
        MessageUI.Instance.SendMessageToEveryoneExceptMe(messages);
    }

    private bool CheckIfAllRolled()
    {
        foreach (KeyValuePair<ulong, bool> client in clientRolled)
        {
            if (client.Value == false) return false;
        }

        return true;
    }

    [ClientRpc]
    private void EnableEndTurnButtonClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnEnableEndTurnButton?.Invoke();
        GridManager.Instance.GetGridPositionsWherePlayerCanInteract();
    }

    [ClientRpc]
    private void EndRollClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnEndRoll?.Invoke();
    }

    private string[] CreateOnRollStartedMessage()
    {
        Player player = Player.LocalInstance;

        return new string[]
        {
            $"ABILITY ROLL",
            $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color> needs to do an ability roll"
        };
    }

    private string[] CreateOnRollFailedMessage()
    {
        Player player = Player.LocalInstance;

        return new string[]
        {
            $"YOU FAILED THE ABILITY ROLL",
            $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color> failed the ability roll"
        };
    }

    private string[] CreateOnRollSucceededMessage()
    {
        Player player = Player.LocalInstance;

        return new string[]
        {
            $"YOU SUCCEEDED THE ABILITY ROLL",
            $"<color=#{player.HexPlayerColor}>{player.PlayerName}</color> succeeded the ability roll"
        };
    }
}

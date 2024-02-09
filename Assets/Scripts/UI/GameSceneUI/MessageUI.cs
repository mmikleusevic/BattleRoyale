using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;

    ClientRpcParams otherClientsRpcParams;

    public void Awake()
    {
        SetMessage("GAME STARTED");

        WaitingForPlayers.OnWaitingForPlayers += OnCallbackSetMessage;
        Initiative.OnInitiativeStart += OnCallbackSetMessage;
        Roll.OnRoll += Roll_OnRollResult;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid += OnCallbackSetMessages;
        PlaceOnGrid.OnPlayerPlaced += PlaceOnGrid_OnPlayerPlaced;
        PlayerTurn.OnPlayerTurn += OnCallbackSetMessages;
        PlayerTurn.OnPlayerMoved += PlayerTurn_OnPlayerMoved;       
    }

    public override void OnNetworkDespawn()
    {
        WaitingForPlayers.OnWaitingForPlayers -= OnCallbackSetMessage;
        Initiative.OnInitiativeStart -= OnCallbackSetMessage;
        Roll.OnRoll -= Roll_OnRollResult;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid -= OnCallbackSetMessages;
        PlaceOnGrid.OnPlayerPlaced -= PlaceOnGrid_OnPlayerPlaced;
        PlayerTurn.OnPlayerTurn -= OnCallbackSetMessages;
        PlayerTurn.OnPlayerMoved -= PlayerTurn_OnPlayerMoved;

        base.OnNetworkDespawn();
    }

    private void PlaceOnGrid_OnPlayerPlaced(object sender, string e)
    {
        SendMessageToEveryoneServerRpc(e);
    }

    private void OnCallbackSetMessage(object sender, string e)
    {
        SetMessage(e);
    }

    private void OnCallbackSetMessages(object sender, string[] e)
    {
        SetMessage(e[0]);

        SendMessageToEveryoneExceptMeServerRpc(e[1]);
    }

    private void Roll_OnRollResult(object sender, Roll.OnRollEventArgs e)
    {
        SendMessageToEveryoneServerRpc(e.message);
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        SetMessage(e.message);
    }

    private void PlayerTurn_OnPlayerMoved(object sender, string e)
    {
        SendMessageToEveryoneServerRpc(e);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SendMessageToEveryoneServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        SendMessageToEveryoneClientRpc(message);
    }

    [ClientRpc]
    private void SendMessageToEveryoneClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        SetMessage(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageToEveryoneExceptMeServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        ulong[] otherCLients = new ulong[NetworkManager.ConnectedClients.Count - 1];

        int i = 0;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (clientId == serverRpcParams.Receive.SenderClientId) continue;

            otherCLients[i] = clientId;

            i++;
        }

        ClientRpcParams otherClientsRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = otherCLients
            }
        };

        SendMessageToEveryoneExceptMeClientRpc(message, otherClientsRpcParams);
    }

    [ClientRpc]
    private void SendMessageToEveryoneExceptMeClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        SetMessage(message);
    }

    private void SetMessage(string message)
    {
        messageText.text += message + '\n';

        StartCoroutine(ScrollToEnd());
    }

    private IEnumerator ScrollToEnd()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0;
    }    
}

using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;

    public void Awake()
    {
        SetMessage("Game started");

        Initiative.OnInitiativeStart += Initiative_OnInitiativeStart;
        WaitingForPlayers.OnWaitingForPlayers += WaitingForPlayers_OnWaitingForPlayers;
        Roll.OnRoll += Roll_OnRollResult;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid += PlaceOnGrid_OnPlaceOnGrid;
    }

    public override void OnNetworkDespawn()
    {
        Initiative.OnInitiativeStart -= Initiative_OnInitiativeStart;
        WaitingForPlayers.OnWaitingForPlayers -= WaitingForPlayers_OnWaitingForPlayers;
        Roll.OnRoll -= Roll_OnRollResult;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid -= PlaceOnGrid_OnPlaceOnGrid;

        base.OnNetworkDespawn();
    }

    private void Initiative_OnInitiativeStart(object sender, string e)
    {       
        SetMessage(e);
    }

    private void WaitingForPlayers_OnWaitingForPlayers(object sender, string e)
    {
        SetMessage(e);
    }

    private void Roll_OnRollResult(object sender, Roll.OnRollEventArgs e)
    {
        SetMessageForEveryoneServerRpc(e.message);
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        StartCoroutine(DelaySendingMessage(3f, e.message));
    }

    private void PlaceOnGrid_OnPlaceOnGrid(object sender, string e)
    {
        SetMessage(e);
    }

    private IEnumerator DelaySendingMessage(float timeToDelay, string message)
    {
        yield return new WaitForSeconds(timeToDelay);

        SetMessage(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMessageForEveryoneServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        SetMessageForEveryoneClientRpc(message);
    }

    [ClientRpc]
    private void SetMessageForEveryoneClientRpc(string message, ClientRpcParams clientRpcParams = default)
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

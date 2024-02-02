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
        SetMessage("GAME STARTED");

        Initiative.OnInitiativeStart += OnCallbackSetMessage;
        WaitingForPlayers.OnWaitingForPlayers += OnCallbackSetMessage;
        Roll.OnRoll += Roll_OnRollResult;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid += OnCallbackSetMessage;
    }

    public override void OnNetworkDespawn()
    {
        Initiative.OnInitiativeStart -= OnCallbackSetMessage;
        WaitingForPlayers.OnWaitingForPlayers -= OnCallbackSetMessage;
        Roll.OnRoll -= Roll_OnRollResult;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid -= OnCallbackSetMessage;

        base.OnNetworkDespawn();
    }

    private void OnCallbackSetMessage(object sender, string e)
    {
        SetMessage(e);
    }

    private void Roll_OnRollResult(object sender, Roll.OnRollEventArgs e)
    {
        SetMessageForEveryoneServerRpc(e.message);
    }

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        SetMessage(e.message);
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

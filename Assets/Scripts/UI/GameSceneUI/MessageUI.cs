using System;
using System.Collections;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;

    public void Awake()
    {

        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        Roll.OnRoll += Roll_OnRollResult;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Instance.OnGameStarted -= GameManager_OnGameStarted;
        Roll.OnRoll -= Roll_OnRollResult;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;

        base.OnNetworkDespawn();
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        SetMessage("Game started");
    }

    private void Roll_OnRollResult(object sender, string e)
    {
        SetMessageForEveryoneServerRpc(e);
    }

    private void RollResults_OnInitiativeRollOver(object sender, string e)
    {
        StartCoroutine(DelaySendingMessage(3f, e));
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

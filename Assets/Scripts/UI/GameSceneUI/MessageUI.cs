using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MessageUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void Awake()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        Roll.OnRoll += Roll_OnRollResult;
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Instance.OnGameStarted -= GameManager_OnGameStarted;
        Roll.OnRoll -= Roll_OnRollResult;

        base.OnNetworkDespawn();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        SetMessage("Game started");
    }

    private void Roll_OnRollResult(object sender, int e)
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerData();
        string colorString = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId).ToHexString();

        Debug.Log(colorString);

        FixedString128Bytes message = new FixedString128Bytes($"<color=#{colorString}>{playerData.playerName}</color> rolled <color=#{colorString}>{e}</color>");

        SetMessageForEveryoneServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMessageForEveryoneServerRpc(FixedString128Bytes message, ServerRpcParams serverRpcParams = default)
    {
        SetMessageForEveryoneClientRpc(message);
    }

    [ClientRpc]
    private void SetMessageForEveryoneClientRpc(FixedString128Bytes message, ClientRpcParams clientRpcParams = default)
    {
        SetMessage(message.ToString());
    }

    private void SetMessage(string message)
    {
        messageText.text += message + '\n';
    }
}

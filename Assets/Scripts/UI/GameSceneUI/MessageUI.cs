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

        WaitingForPlayers.OnWaitingForPlayers += OnCallbackSetMessage;
        Initiative.OnInitiativeStart += OnCallbackSetMessage;
        Roll.OnRoll += Roll_OnRollResult;
        InitiativeResults.OnInitiativeRollOver += InitiativeResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid += OnCallbackSetMessages;
        PlaceOnGrid.OnPlayerPlaced += OnCallbackSetMessages;
        PlayerTurn.OnPlayerTurn += OnCallbackSetMessages;
        Player.OnPlayerMoved += OnCallbackSetMessage;
        PlayerBattleResults.OnPlayerBattleRollOver += PlayerBattleResults_OnPlayerBattleRollOver;
        CardBattleResults.OnCardRollOver += OnCallbackSetMessage;
        AttackPlayerInfoUI.OnAttackPlayer += AttackPlayerInfoUI_OnAttackPlayer;
        EndTurnUI.OnEndTurn += OnCallbackSetMessages;
        PlayerBattleResults.OnPlayerBattleReRoll += OnCallbackSetMessages;
        CardBattleResults.OnCardWon += CardBattleResults_OnCardWon;
        CardBattleResults.OnCardLost += OnCallbackSetMessages;
        Player.OnPlayerResurrected += OnCallbackSetMessages;
        ActionsUI.OnAttackCard += ActionsUI_OnAttackCard;
    }

    public override void OnNetworkDespawn()
    {
        WaitingForPlayers.OnWaitingForPlayers -= OnCallbackSetMessage;
        Initiative.OnInitiativeStart -= OnCallbackSetMessage;
        Roll.OnRoll -= Roll_OnRollResult;
        InitiativeResults.OnInitiativeRollOver -= InitiativeResults_OnInitiativeRollOver;
        PlaceOnGrid.OnPlaceOnGrid -= OnCallbackSetMessages;
        PlaceOnGrid.OnPlayerPlaced -= OnCallbackSetMessages;
        PlayerTurn.OnPlayerTurn -= OnCallbackSetMessages;
        Player.OnPlayerMoved -= OnCallbackSetMessage;
        PlayerBattleResults.OnPlayerBattleRollOver -= PlayerBattleResults_OnPlayerBattleRollOver;
        CardBattleResults.OnCardRollOver -= OnCallbackSetMessage;
        AttackPlayerInfoUI.OnAttackPlayer -= AttackPlayerInfoUI_OnAttackPlayer;
        EndTurnUI.OnEndTurn -= OnCallbackSetMessages;
        PlayerBattleResults.OnPlayerBattleReRoll -= OnCallbackSetMessages;
        CardBattleResults.OnCardWon -= CardBattleResults_OnCardWon;
        CardBattleResults.OnCardLost -= OnCallbackSetMessages;
        Player.OnPlayerResurrected -= OnCallbackSetMessages;
        ActionsUI.OnAttackCard -= ActionsUI_OnAttackCard;

        base.OnNetworkDespawn();
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

    private void OnCallbackSetMessages(string[] e)
    {
        SetMessage(e[0]);

        SendMessageToEveryoneExceptMeServerRpc(e[1]);
    }

    private void OnCallbackSetMessages(string e)
    {
        SetMessage(e);

        SendMessageToEveryoneExceptMeServerRpc(e);
    }

    private void Roll_OnRollResult(object sender, Roll.OnRollEventArgs e)
    {
        OnCallbackSetMessages(e.messages);
    }

    private void InitiativeResults_OnInitiativeRollOver(object sender, InitiativeResults.OnInitiativeRollOverEventArgs e)
    {
        SetMessage(e.message);
    }

    private void PlayerBattleResults_OnPlayerBattleRollOver(object sender, PlayerBattleResults.OnBattleRollOverEventArgs e)
    {
        OnCallbackSetMessages(e.message);
    }

    private void AttackPlayerInfoUI_OnAttackPlayer(NetworkObjectReference arg1, NetworkObjectReference arg2, string arg3)
    {
        OnCallbackSetMessages(arg3);
    }

    private void CardBattleResults_OnCardWon(CardBattleResults.OnCardWonEventArgs obj)
    {
        OnCallbackSetMessages(obj.messages);
    }

    private void ActionsUI_OnAttackCard(Tile arg1, string[] arg2)
    {
        OnCallbackSetMessages(arg2);
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

using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MessageUI : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static MessageUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;
    public bool IsTouched { get; private set; }

    public void Awake()
    {
        Instance = this;

        SetMessage("GAME STARTED");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        IsTouched = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        IsTouched = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsTouched = false;
    }

    public void SendMessageToEveryoneExceptMe(string[] messages)
    {
        SetMessage(messages[0]);

        SendMessageToEveryoneExceptMeServerRpc(messages[1]);
    }

    public void SendMessageToEveryoneExceptMe(string message)
    {
        SetMessage(message);

        SendMessageToEveryoneExceptMeServerRpc(message);
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

    public void SetMessage(string message)
    {
        messageText.text += message + '\n';

        StartCoroutine(ScrollToEnd());
    }

    private IEnumerator ScrollToEnd()
    {
        if (!IsTouched)
        {
            yield return new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}

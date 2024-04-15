using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Hide();

        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;
        LobbyServiceHandler.OnCreateLobbyStarted += LobbyServiceHandler_OnCreateLobbyStarted;
        LobbyServiceHandler.OnCreateLobbyFailed += LobbyServiceHandler_OnCreateLobbyFailed;
        LobbyServiceHandler.OnJoinStarted += LobbyServiceHandler_OnJoinStarted;
        LobbyServiceHandler.OnJoinFailed += LobbyServiceHandler_OnJoinFailed;
        LobbyServiceHandler.OnQuickJoinFailed += LobbyServiceHandler_OnQuickJoinFailed;
        LobbyServiceHandler.OnRemovingJoinedLobbies += LobbyServiceHandler_OnRemovingJoinedLobbies;
        LobbyServiceHandler.OnRemovingJoinedLobbiesOver += LobbyServiceHandler_OnRemovingJoinedLobbiesOver;
        LobbyServiceHandler.OnRemovingJoinedLobbiesFailed += LobbyServiceHandler_OnRemovingJoinedLobbiesFailed;
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
        LobbyServiceHandler.OnCreateLobbyStarted -= LobbyServiceHandler_OnCreateLobbyStarted;
        LobbyServiceHandler.OnCreateLobbyFailed -= LobbyServiceHandler_OnCreateLobbyFailed;
        LobbyServiceHandler.OnJoinStarted -= LobbyServiceHandler_OnJoinStarted;
        LobbyServiceHandler.OnJoinFailed -= LobbyServiceHandler_OnJoinFailed;
        LobbyServiceHandler.OnQuickJoinFailed -= LobbyServiceHandler_OnQuickJoinFailed;
        LobbyServiceHandler.OnRemovingJoinedLobbies -= LobbyServiceHandler_OnRemovingJoinedLobbies;
        LobbyServiceHandler.OnRemovingJoinedLobbiesOver -= LobbyServiceHandler_OnRemovingJoinedLobbiesOver;
        LobbyServiceHandler.OnRemovingJoinedLobbiesFailed -= LobbyServiceHandler_OnRemovingJoinedLobbiesFailed;

        closeButton.onClick.RemoveAllListeners();
    }

    private void LobbyServiceHandler_OnQuickJoinFailed()
    {
        ShowMessage("Could not find a lobby to quick join!");
    }

    private void LobbyServiceHandler_OnJoinFailed()
    {
        ShowMessage("Failed to join lobby!");
    }

    private void LobbyServiceHandler_OnJoinStarted()
    {
        ShowMessage("Joining lobby...");
    }

    private void LobbyServiceHandler_OnCreateLobbyStarted()
    {
        ShowMessage("Creating lobby...");
    }

    private void LobbyServiceHandler_OnCreateLobbyFailed()
    {
        ShowMessage("Failed to create lobby!");
    }

    private void GameMultiplayer_OnFailedToJoinGame()
    {
        if (string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
        {
            ShowMessage("Failed to connect...");
        }
        else
        {
            ShowMessage(NetworkManager.Singleton.DisconnectReason);
        }
    }

    private void LobbyServiceHandler_OnRemovingJoinedLobbies()
    {
        ShowMessage("Removing you from all joined lobbies...");
    }

    private void LobbyServiceHandler_OnRemovingJoinedLobbiesOver()
    {
        Hide();
    }

    private void LobbyServiceHandler_OnRemovingJoinedLobbiesFailed()
    {
        ShowMessage("Removing from lobbies failed...");
    }

    private void ShowMessage(string message)
    {
        Show();
        messageText.text = message;
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

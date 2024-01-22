using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSceneUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI readyButtonText;

    private int playerCount = 0;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            CharacterSceneReady.Instance.RemoveKeyFromPlayerReady();
            GameLobby.Instance.LeaveLobbyGoToMainMenu();
        });

        readyButton.onClick.AddListener(() =>
        {
            UpdateReadyButton();
        });

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        CharacterSceneReady.Instance.OnReadyChanged += CharacterSceneReady_OnReadyChanged;
    }

    private void CharacterSceneReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        ChangeReadyButtonText();
    }

    private void Start()
    {
        UpdateLobbyData();
    }

    private void OnDestroy()
    {
        CharacterSceneReady.Instance.OnReadyChanged -= CharacterSceneReady_OnReadyChanged;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        playerCount++;
        ToggleReadyButton();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        playerCount--;
        ToggleReadyButton();
    }

    private void UpdateLobbyData()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        playerCount = lobby.Players.Count;
        ToggleReadyButton();

        CharacterSceneReady.Instance.GetPlayerReadyValuesForClient();

    }

    private void UpdateReadyButton()
    {
        CharacterSceneReady.Instance.TogglePlayerReady();

        ChangeReadyButtonText();
    }

    private void ToggleReadyButton()
    {
        //Can start game if 2 or more players connected and ready
        if (playerCount >= 2)
        {
            readyButton.interactable = true;
        }
        else
        {
            readyButton.interactable = false;
        }
    }

    private void ChangeReadyButtonText()
    {
        bool isClientReady = CharacterSceneReady.Instance.IsPlayerReady(NetworkManager.Singleton.LocalClientId);

        if (isClientReady)
        {
            readyButtonText.text = "NOT READY";
        }
        else
        {
            readyButtonText.text = "READY";
        }
    }
}

using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyUI : MonoBehaviour
{
    public static event EventHandler OnJoined;

    [SerializeField] private int playerIndex;
    [SerializeField] private Button readyToggleButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshProUGUI readyToggleButtonText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private Transform playerJoinedTemplateContainer;
    [SerializeField] private Transform playerJoinedTemplate;
    [SerializeField] private Image playerColor;

    private bool isReady = false;

    private void Awake()
    {
        readyToggleButton.onClick.AddListener(() =>
        {
            CharacterReady.Instance.SetPlayerReady();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            BackToMainMenu();
        });

        kickButton.onClick.AddListener(() =>
        {
            KickPlayer();
        });
    }

    private void Start()
    {
        Hide();

        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterReady.Instance.OnReadyChanged += CharacterReady_OnReadyChanged;
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterReady.Instance.OnReadyChanged -= CharacterReady_OnReadyChanged;
    }

    private void OnEnable()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();

        if(lobby != null)
        {
            lobbyNameText.text = "Lobby Name: " + lobby.Name;
            lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

            readyToggleButton.gameObject.SetActive(NetworkManager.Singleton.IsClient);

            kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer && playerIndex != 0);

            UpdatePlayer();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CharacterReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void KickPlayer()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        GameLobby.Instance.KickPlayer(playerData.playerId.ToString());
        GameMultiplayer.Instance.KickPlayer(playerData.clientId);
    }

    private void ToggleReady()
    {
        if (isReady)
        {
            readyToggleButtonText.text = "READY!";
        }
        else
        {
            readyToggleButtonText.text = "NOT READY!";
        }

        isReady = !isReady;
    }

    private void UpdatePlayer()
    {
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            //TODO ???
            //Show();

            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);

            isReady = CharacterReady.Instance.IsPlayerReady(playerData.clientId);
            ToggleReady();

            playerNameText.text = playerData.playerName.ToString();
            playerColor.color = GameMultiplayer.Instance.GetPlayerColor(playerData.colorId);
        }
        else
        {
            //TODO ???
            Hide();
        }
    }

    private void BackToMainMenu()
    {
        if(GameMultiplayer.Instance.IsHost)
        {
            GameLobby.Instance.DeleteLobby();
        }
        else
        {
            GameLobby.Instance.LeaveLobby();
        }

        NetworkManager.Singleton.Shutdown();
        LevelManager.Instance.LoadScene(Scene.MainMenuScene);
    }
}

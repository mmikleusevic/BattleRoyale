using TMPro;
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

    private bool isReady = false;
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.LeaveLobbyGoToMainMenu();
        });

        readyButton.onClick.AddListener(() =>
        {
            UpdateReadyButton();
        });
    }
    private void Start()
    {
        UpdateLobbyData();
    }

    private void UpdateLobbyData()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();
        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;
    }

    private void UpdateReadyButton()
    {
        CharacterSceneReady.Instance.TogglePlayerReady();

        isReady = !isReady;

        if (isReady)
        {
            readyButtonText.text = "NOT READY";
        }
        else
        {
            readyButtonText.text = "READY";
        }
    }
}

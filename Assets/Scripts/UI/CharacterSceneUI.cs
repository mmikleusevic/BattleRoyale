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

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            BackToMainMenu();
        });

        readyButton.onClick.AddListener(() =>
        {
            CharacterSceneReady.Instance.SetPlayerReady();
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

    private void BackToMainMenu()
    {
        if (GameMultiplayer.Instance.IsHost)
        {
            GameLobby.Instance.DeleteLobby();
            GameMultiplayer.Instance.StopHost();
        }
        else
        {
            GameLobby.Instance.LeaveLobby();
            GameMultiplayer.Instance.StopClient();
        }

        LevelManager.Instance.LoadScene(Scene.MainMenuScene);
    }
}

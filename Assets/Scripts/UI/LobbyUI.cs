using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        Instance = this;

        mainMenuButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.QuickJoin();
        });

        joinCodeButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = GameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) =>
        {
            GameMultiplayer.Instance.SetPlayerName(newText);
        });

        UpdateLobbyList(new List<Lobby>());
    }

    private void OnEnable()
    {
        GameLobby.Instance.OnLobbyListChanged += GameLobby_OnLobbyListChanged;
    }

    private void OnDestroy()
    {
        GameLobby.Instance.OnLobbyListChanged -= GameLobby_OnLobbyListChanged;
    }

    private void GameLobby_OnLobbyListChanged(object sender, GameLobby.OnLobbyListChangdEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;

            Destroy(child.gameObject);
        }

        ToggleButtonsBasedOnListCount(lobbyList.Count);

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void ToggleButtonsBasedOnListCount(int count)
    {
        if (count > 0)
        {
            joinCodeInputField.interactable = true;
            quickJoinButton.interactable = true;
            joinCodeButton.interactable = true;
        }
        else
        {
            joinCodeInputField.interactable = false;
            quickJoinButton.interactable = false;
            joinCodeButton.interactable = false;
        }
    }
}

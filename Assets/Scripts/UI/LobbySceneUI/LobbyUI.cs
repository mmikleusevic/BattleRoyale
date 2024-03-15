using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    public event EventHandler<OnLobbyFindEventArgs> OnLobbyFind;

    public class OnLobbyFindEventArgs : EventArgs
    {
        public string lobbyName;
    }

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField findLobbyInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        Instance = this;

        playerNameInputField.characterLimit = 10;

        mainMenuButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            LobbyCreateUI.Instance.Show();
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.QuickJoin();
        });

        joinCodeButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });

        playerNameInputField.onValueChanged.AddListener((string playerName) =>
        {
            GameMultiplayer.Instance.SetPlayerName(playerName);
        });

        joinCodeInputField.onValueChanged.AddListener((string lobbyCode) =>
        {
            OnJoinCodeValueChanged(lobbyCode);
        });

        findLobbyInputField.onValueChanged.AddListener((string lobbyName) =>
        {
            OnFindLobbyValueChanged(lobbyName);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = GameMultiplayer.Instance.GetPlayerName();
        joinCodeButton.interactable = false;

        UpdateLobbyList(new List<Lobby>());
    }

    private void OnEnable()
    {
        LobbyServiceHandler.OnLobbyListChanged += LobbyServiceHandler_OnLobbyListChanged;
    }

    private void OnDestroy()
    {
        LobbyServiceHandler.OnLobbyListChanged -= LobbyServiceHandler_OnLobbyListChanged;

        mainMenuButton.onClick.RemoveAllListeners();
        createLobbyButton.onClick.RemoveAllListeners();
        quickJoinButton.onClick.RemoveAllListeners();
        joinCodeButton.onClick.RemoveAllListeners();
        playerNameInputField.onValueChanged.RemoveAllListeners();
        joinCodeInputField.onValueChanged.RemoveAllListeners();
        findLobbyInputField.onValueChanged.RemoveAllListeners();
    }

    private void LobbyServiceHandler_OnLobbyListChanged(object sender, LobbyServiceHandler.OnLobbyListChangdEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void OnJoinCodeValueChanged(string lobbyCode)
    {
        joinCodeInputField.text = lobbyCode.ToUpper();

        if (string.IsNullOrEmpty(joinCodeInputField.text))
        {
            joinCodeButton.interactable = false;
        }
        else
        {
            joinCodeButton.interactable = true;
        }
    }

    private void OnFindLobbyValueChanged(string lobbyName)
    {
        OnLobbyFind?.Invoke(this, new OnLobbyFindEventArgs
        {
            lobbyName = lobbyName
        });
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;

            Destroy(child.gameObject);
        }

        ToggleJoinButtons(lobbyList.Count);

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyListSingleUI>().SetLobby(lobby);
        }
    }

    private void ToggleJoinButtons(int count)
    {
        if (count > 0)
        {
            quickJoinButton.interactable = true;
        }
        else
        {
            quickJoinButton.interactable = false;
        }
    }
}

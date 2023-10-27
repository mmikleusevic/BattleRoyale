using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }

    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        Instance = this;

        createPublicButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyNameInputField.text, false);
        });

        createPrivateButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.CreateLobby(lobbyNameInputField.text, true);
        });

        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        Hide();

        GameLobby.Instance.OnCreateLobbyStarted += GameLobby_OnCreateLobbyStarted;
    }

    private void OnDestroy()
    {
        GameLobby.Instance.OnCreateLobbyStarted -= GameLobby_OnCreateLobbyStarted;
    }

    private void GameLobby_OnCreateLobbyStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        lobbyNameInputField.Select();
        lobbyNameInputField.ActivateInputField();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

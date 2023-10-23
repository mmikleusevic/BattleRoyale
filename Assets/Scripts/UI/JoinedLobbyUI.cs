using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinedLobbyUI : MonoBehaviour
{
    public static JoinedLobbyUI Instance { get; private set; }

    [SerializeField] private Button readyToggleButton;
    [SerializeField] private Button backToLobbiesButton;
    [SerializeField] private TextMeshProUGUI readyToggleButtonText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerColor;
    [SerializeField] private Transform playerJoinedTemplateContainer;
    [SerializeField] private Transform playerJoinedTemplate;

    public bool isReady = false;

    private void Awake()
    {
        Instance = this;

        readyToggleButton.onClick.AddListener(() =>
        {
            ToggleReady();
        });

        backToLobbiesButton.onClick.AddListener(() =>
        {
            LeaveLobby();
            Hide();
        });

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ToggleReady()
    {
        if(isReady)
        {
            readyToggleButtonText.text = "READY!";
        }
        else
        {
            readyToggleButtonText.text = "NOT READY!";
        }

        isReady = !isReady;
    }

    private void LeaveLobby()
    {
        GameLobby.Instance.LeaveLobby();
    }
}

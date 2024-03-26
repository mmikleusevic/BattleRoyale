using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMultiplayerUI : MonoBehaviour
{
    public static PauseMultiplayerUI Instance { get; private set; }

    [SerializeField] private Button pauseButton;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI pauseText;

    private void Awake()
    {
        Instance = this;

        pauseButton.onClick.AddListener(() =>
        {
            GameManager.Instance.TogglePauseGame();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnMultiplayerGamePaused += GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnpaused += GameManager_OnMultiplayerGameUnpaused;
        Won.OnWon += OnGameOver;
        Lost.OnLost += OnGameOver;

        pauseText.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }

    private void OnGameOver(string obj)
    {
        GameManager.Instance.OnMultiplayerGamePaused -= GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnpaused -= GameManager_OnMultiplayerGameUnpaused;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMultiplayerGamePaused -= GameManager_OnMultiplayerGamePaused;
        GameManager.Instance.OnMultiplayerGameUnpaused -= GameManager_OnMultiplayerGameUnpaused;
        Won.OnWon -= OnGameOver;
        Lost.OnLost -= OnGameOver;

        pauseButton.onClick.RemoveAllListeners();
    }

    private void GameManager_OnMultiplayerGameUnpaused()
    {
        Hide();
    }

    private void GameManager_OnMultiplayerGamePaused()
    {
        Show();
    }

    private void Show()
    {
        pauseText.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
    }

    private void Hide()
    {
        pauseText.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }
}

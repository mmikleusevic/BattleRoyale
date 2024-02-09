using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    public static GamePauseUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    bool isPaused = false;

    private void Awake()
    {
        Instance = this;

        resumeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.TogglePauseGame();
        });

        mainMenuButton.onClick.AddListener(async () =>
        {
            await GameLobby.Instance.LeaveLobbyOrDelete();
            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
            Time.timeScale = 1f;
        });
    }

    private void Start()
    {
        GameManager.Instance.OnToggleLocalGamePause += GameManager_OnToggleLocalGamePause;

        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnToggleLocalGamePause -= GameManager_OnToggleLocalGamePause;

        resumeButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
    }

    private void GameManager_OnToggleLocalGamePause(object sender, EventArgs e)
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Show();
        }
        else
        {
            Hide();
        }
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

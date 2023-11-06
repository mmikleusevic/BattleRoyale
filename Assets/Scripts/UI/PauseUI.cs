using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    public static PauseUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        Instance = this;

        resumeButton.onClick.AddListener(() =>
        {
            GameManager.Instance.TogglePauseGame();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLocalGamePaused += GameManager_OnLocalGamePaused;
        GameManager.Instance.OnLocalGameUnpaused += GameManager_OnLocalGameUnpaused;

        Hide();
    }

    private void GameManager_OnLocalGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnLocalGamePaused(object sender, EventArgs e)
    {
        Show();
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

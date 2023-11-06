using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseButtonUI : MonoBehaviour
{
    public static PauseButtonUI Instance { get; private set; }

    [SerializeField] private Button pauseButton;

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
    }

    private void GameManager_OnMultiplayerGameUnpaused(object sender, EventArgs e)
    {
        Show();
    }

    private void GameManager_OnMultiplayerGamePaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        pauseButton.gameObject.SetActive(true);
    }

    private void Hide()
    {
        pauseButton.gameObject.SetActive(false);
    }
}

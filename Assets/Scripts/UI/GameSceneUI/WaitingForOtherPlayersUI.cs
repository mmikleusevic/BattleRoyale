using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnGamePlaying += GameManager_OnGamePlaying;
    }

    private void GameManager_OnGamePlaying(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged -= GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnGamePlaying -= GameManager_OnGamePlaying;
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
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

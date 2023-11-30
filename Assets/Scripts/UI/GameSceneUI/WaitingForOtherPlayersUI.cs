using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnStateChanged += GameManger_OnStateChanged;

        Hide();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged -= GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnStateChanged -= GameManger_OnStateChanged;
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {

        //TODO fix states
        Show();
    }

    private void GameManger_OnStateChanged(object sender, System.EventArgs e)
    {
        //TODO fix states
        Hide();
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

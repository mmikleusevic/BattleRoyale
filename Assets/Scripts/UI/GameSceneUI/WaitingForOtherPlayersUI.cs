using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        StateMachine.OnStateChanged += StateMachine_OnStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLocalPlayerReadyChanged -= GameManager_OnLocalPlayerReadyChanged;
        StateMachine.OnStateChanged -= StateMachine_OnStateChanged;
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e)
    {
        Show();
    }

    private void StateMachine_OnStateChanged(object sender, System.EventArgs e)
    {
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

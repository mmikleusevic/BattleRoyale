using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;

    private void Awake()
    {
        endTurnButton.onClick.AddListener(async () =>
        {
            Hide();
            await StateManager.Instance.EndState();
        });

        PlayerTurn.OnPlayerTurn += PlayerTurn_OnPlayerTurn;

        Hide();
    }

    private void OnDestroy()
    {
        PlayerTurn.OnPlayerTurn -= PlayerTurn_OnPlayerTurn;
        endTurnButton.onClick.RemoveAllListeners();
    }

    private void PlayerTurn_OnPlayerTurn(object sender, string[] e)
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

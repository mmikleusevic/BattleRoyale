using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreenUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private void Awake()
    {
        Won.OnWon += Won_OnWon;
        Lost.OnLost += Lost_OnLost;

        Hide();
    }

    private void OnDestroy()
    {
        Won.OnWon -= Won_OnWon;
        Lost.OnLost -= Lost_OnLost;
    }

    private void Won_OnWon(string obj)
    {
        SetText(obj);

        Show();
    }

    private void Lost_OnLost(string obj)
    {
        SetText(obj);

        Show();
    }

    private void SetText(string text)
    {
        gameOverText.text = text;
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

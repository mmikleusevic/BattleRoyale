using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button rulesButton;
    [SerializeField] private Button cardsButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene(Scene.LobbyScene);
        });

        rulesButton.onClick.AddListener(() =>
        {
            RulesUI.Instance.Show();
        });

        cardsButton.onClick.AddListener(() =>
        {
            CardsUI.Instance.Show();
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}

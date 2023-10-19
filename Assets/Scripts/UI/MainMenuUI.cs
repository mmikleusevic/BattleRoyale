using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button RulesButton;
    [SerializeField] private Button CardsButton;
    [SerializeField] private Button QuitButton;

    private void Awake()
    {
        PlayButton.onClick.AddListener(() =>
        {
            LevelManager.Instance.LoadScene(Scene.GameScene);
        });

        RulesButton.onClick.AddListener(() =>
        {
            RulesUI.Instance.Show();
        });

        CardsButton.onClick.AddListener(() =>
        {
            CardsUI.Instance.Show();
        });

        QuitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}

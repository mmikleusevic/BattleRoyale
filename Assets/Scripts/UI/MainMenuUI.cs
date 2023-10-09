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
        Application.targetFrameRate = 60;

        PlayButton.onClick.AddListener(() =>
        {

        });

        RulesButton.onClick.AddListener(() =>
        {
            RulesUI.Instance.Show();
        });

        CardsButton.onClick.AddListener(() =>
        {

        });

        QuitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}

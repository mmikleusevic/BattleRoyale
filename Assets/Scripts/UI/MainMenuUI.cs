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
        Application.targetFrameRate = 60;

        playButton.onClick.AddListener(() =>
        {
            
        });

        rulesButton.onClick.AddListener(() =>
        {
            RulesUI.Instance.Show();
        });

        cardsButton.onClick.AddListener(() =>
        {
            
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}

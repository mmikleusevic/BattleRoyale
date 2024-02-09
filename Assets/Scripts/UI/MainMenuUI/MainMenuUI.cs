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
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        });
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveAllListeners();
        rulesButton.onClick.RemoveAllListeners();
        cardsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI progressText;

    private void Awake()
    {
        Hide();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void LoadScene(Scene targetScene)
    {
        await LoadAsynchronously(targetScene);
    }

    private async Awaitable LoadAsynchronously(Scene targetScene)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());

        Show();

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            slider.value = progress;
            progressText.text = progress * 100f + "%";

            await Awaitable.NextFrameAsync();
        }

        Hide();
    }

    private void Show()
    {
        loadingScreen.SetActive(true);
    }

    private void Hide()
    {
        loadingScreen.SetActive(false);
    }
}

public enum Scene
{
    MainMenuScene,
    LobbyScene,
    GameScene
}

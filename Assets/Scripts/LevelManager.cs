using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private Slider Slider;
    [SerializeField] private TextMeshProUGUI ProgressText;

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

    public void LoadScene(Scene targetScene)
    {
        StartCoroutine(LoadAsynchronously(targetScene));
    }

    private IEnumerator LoadAsynchronously(Scene targetScene)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());

        Show();

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            Slider.value = progress;
            ProgressText.text = progress * 100f + "%";

            yield return null;
        }

        Hide();
    }

    private void Show()
    {
        LoadingScreen.SetActive(true);
    }

    private void Hide()
    {
        LoadingScreen.SetActive(false);
    }

    public enum Scene
    {
        MainMenuScene,
        LobbyScene,
        GameScene
    }
}

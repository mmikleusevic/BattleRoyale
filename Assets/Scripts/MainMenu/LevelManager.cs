using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Canvas canvas;

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

        canvas.sortingOrder = 1;

        Show();

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            slider.value = progress;
            progressText.text = progress * 100f + "%";

            yield return null;
        }

        Hide();

        canvas.sortingOrder = 0;
    }

    public void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
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
    CharacterScene,
    GameScene
}

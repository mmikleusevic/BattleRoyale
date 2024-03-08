using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicUI : MonoBehaviour
{
    [SerializeField] private Button musicButton;
    [SerializeField] private TextMeshProUGUI musicButtonText;

    private bool isEnabled;

    private void Awake()
    {     
        musicButton.onClick.AddListener(() =>
        {
            Toggle();
        });
    }

    private void Start()
    {
        isEnabled = SoundManager.Instance.GetIsMusicEnabled();

        SetButtonText();
    }

    private void OnDestroy()
    {
        musicButton.onClick.RemoveAllListeners();
    }

    public void Toggle()
    {
        SoundManager.Instance.ToggleMusic();
        isEnabled = !isEnabled;

        SetButtonText();
    }

    private void SetButtonText()
    {
        if (isEnabled)
        {
            musicButtonText.text = "MUSIC";
        }
        else
        {
            musicButtonText.text = "<s>MUSIC</s>";
        }
    }
}
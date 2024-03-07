using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUND = "PlayerPrefsSound";
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        bool.TryParse(PlayerPrefs.GetString(PLAYER_PREFS_SOUND), out bool result);
        musicSource.mute = result;
    }

    public bool GetIsMusicEnabled()
    {
        return !musicSource.mute;
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        PlayerPrefs.SetString(PLAYER_PREFS_SOUND, musicSource.mute.ToString());
    }
}

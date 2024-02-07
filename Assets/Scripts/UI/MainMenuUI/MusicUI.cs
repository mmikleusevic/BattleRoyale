using UnityEngine;

public class MusicUI : MonoBehaviour
{
    public void Toggle()
    {
       SoundManager.Instance.ToggleMusic();
    }
}
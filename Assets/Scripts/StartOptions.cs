using UnityEngine;

public class StartOptions : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}

using UnityEngine;

public class StartOptions : MonoBehaviour
{
    private void Awake()
    {
        //TODO check if ok
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}

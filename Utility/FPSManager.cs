using UnityEngine;

public static class FPSManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetFPS()
    {
        Application.targetFrameRate = 240;
        QualitySettings.vSyncCount = 0;
    }
}
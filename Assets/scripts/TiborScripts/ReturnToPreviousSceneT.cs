using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReturnToPreviousSceneT
{
    private const string PrevSceneKey = "RTPS_prevScene";
    private const string PosXKey = "RTPS_posX";
    private const string PosYKey = "RTPS_posY";
    private const string PosZKey = "RTPS_posZ";
    private const string RotYKey = "RTPS_rotY";
    private const string PendingRestoreKey = "RTPS_pendingRestore";

    // Store which scene we intend to restore INTO
    private const string TargetSceneKey = "RTPS_targetScene";

    //  FORCE RETURN SCENE (MemoryGame will ALWAYS go here)
    private const string ForcedReturnScene = "Scene35School1896";

    public static void SaveReturnPoint(Transform player)
    {
        if (player == null) return;

        //  Always return to Scene35School1896, no matter where we came from
        PlayerPrefs.SetString(PrevSceneKey, ForcedReturnScene);
        PlayerPrefs.SetString(TargetSceneKey, ForcedReturnScene);

        Vector3 p = player.position;
        PlayerPrefs.SetFloat(PosXKey, p.x);
        PlayerPrefs.SetFloat(PosYKey, p.y);
        PlayerPrefs.SetFloat(PosZKey, p.z);

        PlayerPrefs.SetFloat(RotYKey, player.eulerAngles.y);

        PlayerPrefs.SetInt(PendingRestoreKey, 1);
        PlayerPrefs.Save();
    }

    public static bool HasReturnPoint()
    {
        return PlayerPrefs.HasKey(PrevSceneKey);
    }

    public static void ReturnNow()
    {
        //  Always load Scene35School1896
        string sceneName = ForcedReturnScene;

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.FadeToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public static bool TryGetRestore(out Vector3 pos, out float rotY)
    {
        pos = Vector3.zero;
        rotY = 0f;

        if (PlayerPrefs.GetInt(PendingRestoreKey, 0) != 1)
            return false;

        pos = new Vector3(
            PlayerPrefs.GetFloat(PosXKey, 0f),
            PlayerPrefs.GetFloat(PosYKey, 0f),
            PlayerPrefs.GetFloat(PosZKey, 0f)
        );

        rotY = PlayerPrefs.GetFloat(RotYKey, 0f);
        return true;
    }

    public static string GetTargetSceneName()
    {
        //  Return forced target to match ReturnPointRestorerT checks
        return ForcedReturnScene;
    }

    public static void ClearPendingRestore()
    {
        PlayerPrefs.SetInt(PendingRestoreKey, 0);
        PlayerPrefs.Save();
    }
}
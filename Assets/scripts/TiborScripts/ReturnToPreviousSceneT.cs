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

    // NEW: store which scene we intend to restore INTO
    private const string TargetSceneKey = "RTPS_targetScene";

    public static void SaveReturnPoint(Transform player)
    {
        if (player == null) return;

        string currentScene = SceneManager.GetActiveScene().name;

        // Previous scene is where we want to return to
        PlayerPrefs.SetString(PrevSceneKey, currentScene);

        // NEW: target scene = the scene name we must be in before restoring
        PlayerPrefs.SetString(TargetSceneKey, currentScene);

        Vector3 p = player.position;
        PlayerPrefs.SetFloat(PosXKey, p.x);
        PlayerPrefs.SetFloat(PosYKey, p.y);
        PlayerPrefs.SetFloat(PosZKey, p.z);

        // Only Y rotation (typical third person)
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
        if (!HasReturnPoint()) return;

        string sceneName = PlayerPrefs.GetString(PrevSceneKey, "");
        if (string.IsNullOrEmpty(sceneName)) return;

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

    // NEW: lets the restorer verify scene match
    public static string GetTargetSceneName()
    {
        return PlayerPrefs.GetString(TargetSceneKey, "");
    }

    public static void ClearPendingRestore()
    {
        PlayerPrefs.SetInt(PendingRestoreKey, 0);
        PlayerPrefs.Save();
    }
}
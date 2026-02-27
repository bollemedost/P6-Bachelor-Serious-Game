using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnPointRestorerT : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public bool debugLogs = true;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RestoreAfterLoad(scene.name));
    }

    private IEnumerator RestoreAfterLoad(string sceneName)
    {
        // Wait 1 frame so other Awake/Start scripts can run first
        yield return null;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null)
        {
            if (debugLogs) Debug.LogWarning("[ReturnPointRestorerT] Player not found by tag.");
            yield break;
        }

        if (!ReturnToPreviousSceneT.TryGetRestore(out Vector3 pos, out float rotY))
        {
            if (debugLogs) Debug.Log("[ReturnPointRestorerT] No pending restore.");
            yield break;
        }

        Teleport(playerObj, pos, rotY);

        // Wait 1 more frame and apply again (beats “spawn at start” scripts)
        yield return null;
        Teleport(playerObj, pos, rotY);

        ReturnToPreviousSceneT.ClearPendingRestore();

        if (debugLogs)
            Debug.Log($"[ReturnPointRestorerT] Restored player to {pos} rotY {rotY} in '{sceneName}'.");
    }

    private void Teleport(GameObject playerObj, Vector3 pos, float rotY)
    {
        var cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObj.transform.position = pos;

        Vector3 e = playerObj.transform.eulerAngles;
        e.y = rotY;
        playerObj.transform.eulerAngles = e;

        if (cc != null) cc.enabled = true;
    }
}
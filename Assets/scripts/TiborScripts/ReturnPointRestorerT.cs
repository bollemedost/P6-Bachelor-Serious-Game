using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnPointRestorerT : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public bool debugLogs = true;

    [Header("Stability")]
    [Tooltip("How many frames to re-apply teleport to beat spawn/position scripts.")]
    public int applyFrames = 5;

    private static ReturnPointRestorerT _instance;

    private void Awake()
    {
        // Persist across scenes so you only need ONE instance in your project.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

        // If there's a pending restore, ONLY restore when we're in the intended target scene
        string targetScene = ReturnToPreviousSceneT.GetTargetSceneName();
        if (!string.IsNullOrEmpty(targetScene) && sceneName != targetScene)
        {
            // We're not returning to the saved scene -> clear pending so it can't mess with spawns
            if (debugLogs)
                Debug.Log($"[ReturnPointRestorerT] Pending restore exists but scene '{sceneName}' != target '{targetScene}'. Clearing pending restore.");
            ReturnToPreviousSceneT.ClearPendingRestore();
            yield break;
        }

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

        // Apply multiple frames to beat any "spawn at start" or controller scripts
        for (int i = 0; i < Mathf.Max(1, applyFrames); i++)
        {
            Teleport(playerObj, pos, rotY);
            yield return null;
        }

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
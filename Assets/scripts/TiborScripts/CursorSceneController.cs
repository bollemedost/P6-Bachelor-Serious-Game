using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorSceneController : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool unlockCursorInThisScene = true;

    private void Start()
    {
        if (unlockCursorInThisScene)
            UnlockCursor();
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
        if (unlockCursorInThisScene && scene.name == gameObject.scene.name)
            UnlockCursor();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
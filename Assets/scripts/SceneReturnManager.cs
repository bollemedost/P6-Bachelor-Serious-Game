using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReturnManager : MonoBehaviour
{
    public static SceneReturnManager Instance { get; private set; }

    private string previousScene;
    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerState(Transform player)
    {
        previousScene = SceneManager.GetActiveScene().name;
        savedPosition = player.position;
        savedRotation = player.rotation;
    }

    public void ReturnToPreviousScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(previousScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = savedPosition;
            player.transform.rotation = savedRotation;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
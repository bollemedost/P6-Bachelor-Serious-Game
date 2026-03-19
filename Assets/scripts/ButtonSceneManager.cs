using UnityEngine;

public class ButtonSceneManager : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        if (SceneFadeIn.instance != null)
        {
            SceneFadeIn.instance.FadeOutAndLoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneFadeIn instance not found. Loading without fade.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
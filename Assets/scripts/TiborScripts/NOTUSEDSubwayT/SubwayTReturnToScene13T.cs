using UnityEngine;
using UnityEngine.SceneManagement;

public static class SubwayTReturnToScene13T
{
    private const string SceneToLoad = "Scene13Home1915NOINTERACTION";

    public static void ReturnNow()
    {
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.FadeToScene(SceneToLoad);
        else
            SceneManager.LoadScene(SceneToLoad);
    }
}
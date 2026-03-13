using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneManager : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}

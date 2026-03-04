using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayTransition()
    {
        if (isTransitioning) return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        if (director != null)
        {
            director.Play();

            // Wait until timeline fully finishes
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
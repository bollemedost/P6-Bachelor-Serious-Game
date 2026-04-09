using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;       // Black overlay
    public float fadeDuration = 1f;           // Duration of fade
    public float startDelay = 0.1f;           // Delay before starting fade

    [Header("UI Canvases to Fade In")]
    public CanvasGroup[] uiCanvasGroups;      // Assign all player/UI canvases here
    public float uiFadeDuration = 0.5f;       // How long each UI fades in
    public float uiFadeDelay = 0.2f;          // Optional delay after scene fade before UI fade

    public static SceneFadeIn instance;

    private Coroutine currentFadeRoutine;
    private Coroutine currentLoadRoutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.gameObject.SetActive(true);

        if (uiCanvasGroups != null)
        {
            foreach (CanvasGroup cg in uiCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.gameObject.SetActive(true);
                }
            }
        }

        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        currentFadeRoutine = StartCoroutine(FadeInSceneAndUI());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeCanvasGroup == null)
            return;

        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.gameObject.SetActive(true);

        currentFadeRoutine = StartCoroutine(FadeInSceneAndUIForScene(uiCanvasGroups));
    }

    private IEnumerator FadeInSceneAndUIForScene(CanvasGroup[] sceneUI)
    {
        yield return new WaitForSecondsRealtime(startDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);

        TutorialUI tutorial = FindFirstObjectByType<TutorialUI>();
        if (tutorial != null)
        {
            try
            {
                tutorial.ShowTutorial();
            }
            catch (System.Exception e)
            {
                Debug.LogError("SceneFadeIn: TutorialUI.ShowTutorial() failed: " + e.Message);
            }
        }

        yield return new WaitForSecondsRealtime(uiFadeDelay);

        if (sceneUI != null)
        {
            foreach (CanvasGroup cg in sceneUI)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 0f;

                    elapsed = 0f;
                    while (elapsed < uiFadeDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        cg.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
                        yield return null;
                    }

                    cg.alpha = 1f;
                }
            }
        }

        currentFadeRoutine = null;
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        if (currentLoadRoutine != null)
            StopCoroutine(currentLoadRoutine);

        currentLoadRoutine = StartCoroutine(FadeOutThenLoad(sceneName));
    }

    private IEnumerator FadeOutThenLoad(string sceneName)
    {
        if (fadeCanvasGroup == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        yield return null;

        currentLoadRoutine = null;
    }

    private IEnumerator FadeInSceneAndUI()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(uiFadeDelay);

        if (uiCanvasGroups != null)
        {
            foreach (CanvasGroup cg in uiCanvasGroups)
            {
                if (cg != null)
                {
                    cg.gameObject.SetActive(true);
                    cg.alpha = 0f;

                    elapsed = 0f;
                    while (elapsed < uiFadeDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        cg.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
                        yield return null;
                    }

                    cg.alpha = 1f;
                }
            }
        }

        currentFadeRoutine = null;
    }
}
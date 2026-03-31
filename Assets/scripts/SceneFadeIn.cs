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

    private void Awake()
    {
        // Singleton pattern
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

    private void Start()
    {
        // Fade-in for the first scene
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.gameObject.SetActive(true);

            foreach (CanvasGroup cg in uiCanvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = 0f;
                    cg.gameObject.SetActive(true);
                }
            }

            StartCoroutine(FadeInSceneAndUI());
        }
    }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Only fade-in the UI canvases assigned in the inspector (if any)
    if (fadeCanvasGroup != null)
    {
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.gameObject.SetActive(true);

        // Start fade-in for the scene UI after the fade panel
        StartCoroutine(FadeInSceneAndUIForScene(uiCanvasGroups));
    }
}

// New coroutine for fading UI of a scene after the fade panel
private IEnumerator FadeInSceneAndUIForScene(CanvasGroup[] sceneUI)
{
    // Wait initial fade panel fade
    yield return new WaitForSeconds(startDelay);

    // Fade out black overlay
    float elapsed = 0f;
    while (elapsed < fadeDuration)
    {
        elapsed += Time.deltaTime;
        fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
        yield return null;
    }
    fadeCanvasGroup.alpha = 0f;
    fadeCanvasGroup.gameObject.SetActive(false);

    // 👇 ADD THIS
    FindFirstObjectByType<TutorialUI>()?.ShowTutorial();

    // Wait for UI fade delay
    yield return new WaitForSeconds(uiFadeDelay);

    // Fade in all UI canvases for this scene
    foreach (CanvasGroup cg in sceneUI)
    {
        if (cg != null)
        {
            elapsed = 0f;
            while (elapsed < uiFadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
                yield return null;
            }
            cg.alpha = 1f;
        }
    }
}

    // Call this to fade out and load a new scene
    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutThenLoad(sceneName));
    }

    private IEnumerator FadeOutThenLoad(string sceneName)
    {
        // Make sure fadeCanvasGroup is visible
        fadeCanvasGroup.gameObject.SetActive(true);

        // --- Fade out ---
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        // Load the next scene asynchronously to avoid hiccups
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
            yield return null;

        // Optional: small delay to ensure scene is rendered
        yield return null;
    }

    private IEnumerator FadeInSceneAndUI()
    {
        // Original fade-in logic
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);

        yield return new WaitForSeconds(uiFadeDelay);       

        foreach (CanvasGroup cg in uiCanvasGroups)
        {
            if (cg != null)
            {
                elapsed = 0f;
                while (elapsed < uiFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, elapsed / uiFadeDuration);
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }
    }
}
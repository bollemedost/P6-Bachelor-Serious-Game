using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Fade Settings")]
    public CanvasGroup fadePanelPrefab; // Assign a fullscreen black panel prefab here
    public float fadeDuration = 1f;     // Duration of fade in/out

    private CanvasGroup fadePanel;

    private void Awake()    
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Instantiate fadePanel prefab and make it a child of this singleton
            if (fadePanelPrefab != null)
            {
                fadePanel = Instantiate(fadePanelPrefab, transform);
                fadePanel.alpha = 0f;
                fadePanel.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("FadePanelPrefab not assigned!");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Call this to fade out and load a new scene
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade panel missing! Loading scene without fade.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // Ensure fadePanel is on top and visible
        fadePanel.transform.SetAsLastSibling();
        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(true);

        // --- Fade to black ---
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.alpha = EaseInOutQuad(t);
            yield return null;
        }
        fadePanel.alpha = 1f;

        // --- Load the new scene ---
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // Small frame delay to ensure scene is rendered
        yield return null;

        // --- Fade back in ---
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.alpha = 1f - EaseInOutQuad(t);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }

    // Easing function for smooth fade
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    /// <summary>
    /// Call this from UI buttons
    /// </summary>
    public void LoadSceneFromButton(string sceneName)
    {
        FadeToScene(sceneName);
    }

    /// <summary>
    /// Optional: ensure fadePanel stays on top after scene load
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel != null)
        {
            fadePanel.transform.SetAsLastSibling();
        }
    }
}
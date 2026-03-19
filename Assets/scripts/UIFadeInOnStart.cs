using UnityEngine;
using System.Collections;

public class UIFadeInOnStart : MonoBehaviour
{
    public CanvasGroup[] uiCanvasGroups;
    public float fadeDuration = 0.5f;
    public float delay = 0.2f;

    private void Start()
    {
        foreach (CanvasGroup cg in uiCanvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.gameObject.SetActive(true);
            }
        }

        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            foreach (CanvasGroup cg in uiCanvasGroups)
            {
                if (cg != null)
                    cg.alpha = t;
            }
            yield return null;
        }

        foreach (CanvasGroup cg in uiCanvasGroups)
        {
            if (cg != null)
                cg.alpha = 1f;
        }
    }
}
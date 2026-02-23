using UnityEngine;
using System.Collections;

public class UITomato : MonoBehaviour
{
    public float duration = 0.6f;
    public float arcHeight = 100f;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Throw(Vector2 targetAnchoredPos)
    {
        StartCoroutine(AnimateThrow(targetAnchoredPos));
    }

    IEnumerator AnimateThrow(Vector2 target)
    {
        Vector2 startPos = rectTransform.anchoredPosition;

        float time = 0;

        while (time < duration)
        {
            float t = time / duration;

            Vector2 pos = Vector2.Lerp(startPos, target, t);

            // Arc movement
            pos.y += arcHeight * Mathf.Sin(t * Mathf.PI);

            rectTransform.anchoredPosition = pos;

            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = target;

        Destroy(gameObject, 0.2f);
    }
}

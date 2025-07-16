
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraBlink : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.3f;

    void Start()
    {
        StartCoroutine(FadeOut());
    }

    public void Blink()
    {
        StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        yield return FadeIn();
        yield return new WaitForSeconds(0.1f); 
        yield return FadeOut();
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1);
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0);
    }

    void SetAlpha(float alpha)
    {
        var color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
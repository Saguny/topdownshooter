using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }


    [Header("Refs")] public CanvasGroup canvasGroup; // assign the CanvasGroup on this object
    [Header("Timing")] public float duration = 0.5f;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        DontDestroyOnLoad(gameObject);
    }


    public Coroutine FadeIn() => StartCoroutine(Fade(1f, 0f)); // from black to clear
    public Coroutine FadeOut() => StartCoroutine(Fade(0f, 1f)); // from clear to black


    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = true; // block clicks during transitions
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = to > 0.001f; // only block when black
    }


    public void FadeAndLoad(string sceneName)
    {
        StartCoroutine(FadeAndLoadRoutine(sceneName));
    }


    IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        yield return FadeOut();
        yield return SceneManager.LoadSceneAsync(sceneName);
        // wait a frame so new UI lays out, then fade in
        yield return null;
        yield return FadeIn();
    }
}
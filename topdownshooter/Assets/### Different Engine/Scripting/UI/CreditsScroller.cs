using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [Header("Refs")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public TMP_Text creditsText;

    [Header("Scrolling")]
    public float startDelay = 1f;
    public float scrollSpeed = 60f;
    public KeyCode fastKey = KeyCode.Space;

    [Header("Exit")]
    public KeyCode exitKey = KeyCode.Escape;
    public string menuSceneName = "MainMenu";

    bool running;
    bool exiting;
    float timer;

    void Start()
    {
        // fade from black when entering credits
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeIn();

        Begin();
    }

    public void Begin()
    {
        timer = 0f;
        running = true;
        exiting = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // start at the top (show first line first)
        scrollRect.normalizedPosition = new Vector2(0f, 1f);
    }

    void Update()
    {
        if (!running) return;

        // if esc pressed, start fade but do NOT stop scrolling
        if (Input.GetKeyDown(exitKey) && !exiting)
        {
            StartCoroutine(FadeAndExitAfterDelay());
        }

        // wait before starting
        if (timer < startDelay)
        {
            timer += Time.deltaTime;
            return;
        }

        float scrollable = content.rect.height - scrollRect.viewport.rect.height;
        if (scrollable <= 0f)
        {
            if (!exiting) StartCoroutine(FadeAndExitAfterDelay());
            return;
        }

        float speed = scrollSpeed * (Input.GetKey(fastKey) ? 3f : 1f);
        float dn = (speed * Time.deltaTime) / scrollable;

        var pos = scrollRect.normalizedPosition;
        pos.y = Mathf.Clamp01(pos.y - dn);
        scrollRect.normalizedPosition = pos;

        // when credits naturally finish, fade out
        if (pos.y <= 0.001f && !exiting)
        {
            StartCoroutine(FadeAndExitAfterDelay());
        }
    }

    IEnumerator FadeAndExitAfterDelay()
    {
        if (exiting) yield break;
        exiting = true;

        // trigger fade to black but keep scrolling visible
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(); // fade to black
            // once fully black, then switch
            yield return SceneManager.LoadSceneAsync(menuSceneName);
            yield return ScreenFader.Instance.FadeIn();  // fade back from black in new scene
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}

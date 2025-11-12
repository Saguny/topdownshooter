using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("scenes")]
    public string gameSceneName = "MainTestGame";
    public string creditsSceneName = "CreditsScene";

    bool isLoading;

    void Start()
    {
        // fade from black when menu appears
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeIn();
    }

    public void PlayGame()
    {
        if (isLoading) return;
        isLoading = true;

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeAndLoad(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneName);
    }

    public void OpenCredits()
    {
        if (isLoading) return;
        isLoading = true;

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeAndLoad(creditsSceneName);
        else
            SceneManager.LoadScene(creditsSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource bgmSource;

    public AudioClip[] playlist;
    public string[] playlistNames;

    [Header("Song Title UI (TMP)")]
    public TextMeshProUGUI songNameText; // will be filled at runtime

    [SerializeField]
    private string[] allowedScenes =
    {
        "MainMenu",
        "CreditsScene"
    };

    private int currentIndex = 0;
    private Coroutine switchRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // every time a new scene loads, try to find the TMP again
        RebindSongTextInScene();

        bool shouldPlay = false;
        foreach (string allowed in allowedScenes)
        {
            if (scene.name == allowed)
            {
                shouldPlay = true;
                break;
            }
        }

        if (shouldPlay)
        {
            if (playlist != null && playlist.Length > 0)
            {
                if (bgmSource.clip == null)
                {
                    currentIndex = Mathf.Clamp(currentIndex, 0, playlist.Length - 1);
                    bgmSource.clip = playlist[currentIndex];
                }

                UpdateSongNameUI();

                if (!bgmSource.isPlaying)
                    bgmSource.Play();
            }
            else
            {
                if (!bgmSource.isPlaying)
                    bgmSource.Play();
            }
        }
        else
        {
            if (bgmSource.isPlaying)
                bgmSource.Stop();
        }
    }

    private void RebindSongTextInScene()
    {
        // look for a TMP text with tag "SongNameText" in the current scene
        GameObject go = GameObject.FindWithTag("SongNameText");
        if (go != null)
        {
            songNameText = go.GetComponent<TextMeshProUGUI>();
            UpdateSongNameUI();
        }
        else
        {
            songNameText = null;
        }
    }

    public void NextTrack()
    {
        if (playlist == null || playlist.Length == 0 || bgmSource == null)
            return;

        if (switchRoutine != null)
            StopCoroutine(switchRoutine);

        switchRoutine = StartCoroutine(SwitchToNextTrackCoroutine());
    }

    private IEnumerator SwitchToNextTrackCoroutine()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();

        if (songNameText != null)
            songNameText.text = string.Empty;

        yield return new WaitForSeconds(0.5f);

        currentIndex++;
        if (currentIndex >= playlist.Length)
            currentIndex = 0;

        bgmSource.clip = playlist[currentIndex];
        bgmSource.Play();

        UpdateSongNameUI();
        switchRoutine = null;
    }

    private void UpdateSongNameUI()
    {
        if (songNameText == null)
            return;

        if (playlistNames != null &&
            playlistNames.Length > currentIndex &&
            playlistNames[currentIndex] != null)
        {
            songNameText.text = playlistNames[currentIndex];
        }
        else if (bgmSource.clip != null)
        {
            songNameText.text = bgmSource.clip.name;
        }
        else
        {
            songNameText.text = string.Empty;
        }
    }
}

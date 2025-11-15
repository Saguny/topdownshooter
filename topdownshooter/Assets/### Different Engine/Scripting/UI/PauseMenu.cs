using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [Header("audio mixer")]
    [SerializeField] AudioMixer mixer;         // assign your MasterMixer asset
    [SerializeField] string musicParam = "MusicVol";
    [SerializeField] string sfxParam = "SFXVol";

    [Header("other ui")]
    [SerializeField] private UpgradeMenuUI upgradeMenu; // block pause while this is open

    const string MusicKey = "vol_music";
    const string SfxKey = "vol_sfx";

    bool paused;

    void Start()
    {
        // auto-find upgrade menu if not wired in inspector
        if (upgradeMenu == null)
            upgradeMenu = FindObjectOfType<UpgradeMenuUI>();

        // 1) migrate any old saved zeros so we don't load “perma-mute”
        float musicSaved = PlayerPrefs.GetFloat(MusicKey, 100f);
        float sfxSaved = PlayerPrefs.GetFloat(SfxKey, 100f);
        if (musicSaved <= 0f) { musicSaved = 1f; PlayerPrefs.SetFloat(MusicKey, musicSaved); }
        if (sfxSaved <= 0f) { sfxSaved = 1f; PlayerPrefs.SetFloat(SfxKey, sfxSaved); }

        musicSlider.minValue = 0f; musicSlider.maxValue = 100f;
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 100f;

        musicSlider.SetValueWithoutNotify(musicSaved);
        sfxSlider.SetValueWithoutNotify(sfxSaved);

        ApplyMusic(musicSaved);
        ApplySfx(sfxSaved);

        musicSlider.onValueChanged.AddListener(ApplyMusic);
        sfxSlider.onValueChanged.AddListener(ApplySfx);

        pausePanel.SetActive(false);

        // keep mouse usable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // safety: ensure global listener isn’t muted
        AudioListener.pause = false;
        AudioListener.volume = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // if upgrade menu is open, ignore pause toggle completely
            if (upgradeMenu != null && upgradeMenu.IsOpen)
                return;

            TogglePause();
        }

        // DEV: press F9 to reset volumes to defaults if needed
        if (Input.GetKeyDown(KeyCode.F9))
            ResetVolumesToDefault();
    }

    public void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        pausePanel.SetActive(paused);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2) re-apply current values whenever state changes, in case mixer got suspended previously
        ReapplyMixer();
    }

    public void OnResume()
    {
        if (!paused) return;
        paused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ReapplyMixer();
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === volume logic ===
    void ApplyMusic(float v)
    {
        // 0 → tiny floor to avoid -80 dB hard mute
        float lin = Mathf.Max(v / 100f, 0.001f);
        mixer.SetFloat(musicParam, Mathf.Log10(lin) * 20f);
        PlayerPrefs.SetFloat(MusicKey, v);
    }

    void ApplySfx(float v)
    {
        float lin = Mathf.Max(v / 100f, 0.001f);
        mixer.SetFloat(sfxParam, Mathf.Log10(lin) * 20f);
        PlayerPrefs.SetFloat(SfxKey, v);
    }

    void ReapplyMixer()
    {
        ApplyMusic(musicSlider.value);
        ApplySfx(sfxSlider.value);
        // extra nudge: clear then set, in case a stale value latched
        mixer.ClearFloat(musicParam);
        mixer.ClearFloat(sfxParam);
        ApplyMusic(musicSlider.value);
        ApplySfx(sfxSlider.value);
    }

    void ResetVolumesToDefault()
    {
        musicSlider.value = 100f;
        sfxSlider.value = 100f;
        ReapplyMixer();
    }
}

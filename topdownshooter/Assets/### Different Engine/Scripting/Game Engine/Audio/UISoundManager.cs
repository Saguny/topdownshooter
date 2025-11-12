using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Default UI Clips")]
    public AudioClip defaultHoverSound;
    public AudioClip defaultClickSound;

    [Header("Audio")]
    public AudioSource uiAudioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // keep between scenes if you want
    }

    public void PlayHover(AudioClip overrideClip = null)
    {
        if (uiAudioSource == null) return;

        AudioClip clipToPlay = overrideClip != null ? overrideClip : defaultHoverSound;
        if (clipToPlay == null) return;

        uiAudioSource.PlayOneShot(clipToPlay);
    }

    public void PlayClick(AudioClip overrideClip = null)
    {
        if (uiAudioSource == null) return;

        AudioClip clipToPlay = overrideClip != null ? overrideClip : defaultClickSound;
        if (clipToPlay == null) return;

        uiAudioSource.PlayOneShot(clipToPlay);
    }
}

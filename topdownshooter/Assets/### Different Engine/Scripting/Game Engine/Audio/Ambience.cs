using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SeamlessAmbienceLoop : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Settings")]
    public float fadeDuration = 2f;     // crossfade between loops
    public float startFadeIn = 2f;      // fade in at scene start
    public bool playOnSceneStart = true;
    public bool persistentBetweenScenes = false;

    private AudioSource currentSource;
    private AudioSource nextSource;

    [SerializeField]
    private float targetVolume = 1f;    // the desired "final" volume after fade-in

    void Awake()
    {
        if (persistentBetweenScenes)
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (playOnSceneStart)
            StartLoop();
    }

    public void StartLoop()
    {
        if (sourceA == null || sourceB == null || sourceA.clip == null)
        {
            Debug.LogWarning("SeamlessAmbienceLoop is missing AudioSources or clips.");
            return;
        }

        currentSource = sourceA;
        nextSource = sourceB;

        // remember the volume the user set in the inspector (or via mixer)
        targetVolume = currentSource.volume;

        // start both at 0 for fade-in
        currentSource.volume = 0f;
        nextSource.volume = 0f;

        currentSource.Play();
        StartCoroutine(FadeInToTarget());
        StartCoroutine(LoopRoutine());
    }

    private IEnumerator FadeInToTarget()
    {
        float elapsed = 0f;
        while (elapsed < startFadeIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / startFadeIn;
            currentSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
        currentSource.volume = targetVolume;
    }

    private IEnumerator LoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSource.clip.length - fadeDuration);

            nextSource.time = 0f;
            nextSource.Play();
            StartCoroutine(Crossfade());

            yield return new WaitForSeconds(fadeDuration);

            var temp = currentSource;
            currentSource = nextSource;
            nextSource = temp;
        }
    }

    private IEnumerator Crossfade()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            currentSource.volume = Mathf.Lerp(targetVolume, 0f, t);
            nextSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        currentSource.Stop();
        nextSource.volume = targetVolume;
    }
}

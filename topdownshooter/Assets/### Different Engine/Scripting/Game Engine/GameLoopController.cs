using System.Collections;
using UnityEngine;

public class GameLoopController : MonoBehaviour
{
    [SerializeField] private float waveDuration = 180f;
    [SerializeField] private int baseKillsToClear = 25;
    [SerializeField] private float breakAfterWave = 2f;
    [SerializeField] private GameObject subjectiveDeathFx;

    private int waveIndex;
    private float elapsed;
    private int waveKills;
    private bool finalRush;
    private float totalRun;

    private void OnEnable() => GameEvents.OnEnemyKilled += OnEnemyKilled;
    private void OnDisable() => GameEvents.OnEnemyKilled -= OnEnemyKilled;
    private void Start() => StartCoroutine(Loop());

    private IEnumerator Loop()
    {
        while (true)
        {
            GameEvents.OnWaveStarted?.Invoke(waveIndex + 1);
            elapsed = 0f;
            waveKills = 0;
            finalRush = false;

            while (elapsed < waveDuration)
            {
                elapsed += Time.deltaTime;
                totalRun += Time.deltaTime;
                GameEvents.OnRunTimeChanged?.Invoke(totalRun);
                yield return null;
            }

            finalRush = true;
            waveKills = 0;
            float snap = (waveIndex + 1) * waveDuration;
            totalRun = snap;
            GameEvents.OnRunTimeChanged?.Invoke(totalRun);

            int quota = baseKillsToClear * (int)Mathf.Pow(2, waveIndex);
            GameEvents.OnFinalRushStarted?.Invoke(waveIndex + 1, quota);

            while (waveKills < quota)
                yield return null;

            GameEvents.OnPurgeEnemiesWithFx?.Invoke(subjectiveDeathFx);
            GameEvents.OnCollectAllGears?.Invoke();

            GameEvents.OnFinalRushEnded?.Invoke(waveIndex + 1);
            GameEvents.OnWaveCleared?.Invoke(waveIndex + 1);

            yield return new WaitForSeconds(breakAfterWave);
            waveIndex++;
        }
    }

    private void OnEnemyKilled(int _) { if (finalRush) waveKills++; }
}

using System.Collections;
using UnityEngine;

public class GameLoopController : MonoBehaviour
{
    [SerializeField] private float waveDuration = 180f;
    [SerializeField] private int baseKillsToClear = 25;
    [SerializeField] private float breakAfterWave = 2f;

    private int waveIndex = 0;
    private float elapsed;
    private int waveKills;
    private bool finalRush;

    private void OnEnable() => GameEvents.OnEnemyKilled += OnEnemyKilled;
    private void OnDisable() => GameEvents.OnEnemyKilled -= OnEnemyKilled;

    private void Start() => StartCoroutine(Loop());

    private IEnumerator Loop()
    {
        while (true)
        {
            GameEvents.OnWaveStarted?.Invoke(waveIndex + 1);
            elapsed = 0f; waveKills = 0; finalRush = false;

            while (elapsed < waveDuration)
            {
                elapsed += Time.deltaTime;
                GameEvents.OnRunTimeChanged?.Invoke(elapsed);
                yield return null;
            }

            finalRush = true;
            waveKills = 0;
            int quota = baseKillsToClear + waveIndex * 15;

            while (waveKills < quota)
                yield return null;

            GameEvents.OnWaveCleared?.Invoke(waveIndex + 1);
            yield return new WaitForSeconds(breakAfterWave);

            waveIndex++;
        }
    }

    private void OnEnemyKilled(int _)
    {
        if (finalRush) waveKills++;
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    private enum WaveState
    {
        Spawning,
        Active,
        Clearing,
        WaitingForNext
    }

    [Header("References")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _countdownText;

    [Header("Map Walls (exactly 4 BoxCollider2D: Left, Right, Bottom, Top in any order)")]
    [SerializeField] private BoxCollider2D[] _mapBounds = new BoxCollider2D[4];

    [SerializeField] private float _spawnInnerMargin = 0.25f;

    [Header("Wave Settings")]
    [SerializeField] private float _waveDuration = 180f;       // 3 minutes
    [SerializeField] private float _timeBetweenWaves = 5f;
    [SerializeField] private int _killsToClear = 25;
    [SerializeField] private float _minSpawnDelay = 0.5f;
    [SerializeField] private float _maxSpawnDelay = 5f;
    [SerializeField] private int _maxEnemiesOnField = 25;      // cap to avoid overflow
    [SerializeField] private float _spawnOffset = 2f;          // used if no mapBounds

    private float _elapsedTime = 0f;
    private float _nextCheckpoint = 180f;

    private int _waveNumber = 0;
    private int _totalKills = 0;
    private int _waveKills = 0;
    private float _waveTimer;
    private bool _spawning = false;
    private WaveState _state = WaveState.WaitingForNext;
    private float _spawnDelayModifier = 1f;

    private void OnEnable() => EnemyHealth.OnEnemyDied += HandleEnemyDeath;
    private void OnDisable() => EnemyHealth.OnEnemyDied -= HandleEnemyDeath;

    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_countdownText != null)
            _countdownText.alpha = 0f;

        // set up first wave correctly
        _waveNumber = 0;
        _elapsedTime = 0f;
        _nextCheckpoint = _waveDuration; // 180 seconds
        _state = WaveState.WaitingForNext;

        StartCoroutine(StartNextWave());
    }


    private void Update()
    {
        if (_state == WaveState.Active)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerUI();

            // only trigger when reaching or passing the 3-minute mark
            if (_elapsedTime >= _nextCheckpoint)
            {
                _state = WaveState.Clearing;
                _spawning = false;

                if (_timerText != null)
                    _timerText.text = $"Kill {_killsToClear} Enemies to clear";
            }
        }
    }


    private void HandleEnemyDeath()
    {
        _totalKills++;
        _waveKills++;
        UpdateUI();

        if (_state == WaveState.Clearing && _waveKills >= _killsToClear)
        {
            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(enemy);

            _waveNumber++;
            _waveKills = 0;
            _killsToClear += 25;
            _nextCheckpoint += _waveDuration;
            _state = WaveState.WaitingForNext;

            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        _state = WaveState.WaitingForNext;

        yield return new WaitForSeconds(_timeBetweenWaves);

        _waveNumber++; // increment before showing "Wave X"
        yield return StartCoroutine(ShowWaveCountdown());

        _state = WaveState.Active;

        // make sure we show time, not "Kill X Enemies"
        UpdateTimerUI();
        UpdateUI();

        _spawnDelayModifier = Mathf.Max(0.5f, 1f - _waveNumber * 0.05f);
        StartCoroutine(SpawnEnemiesContinuously());
    }


    private IEnumerator SpawnEnemiesContinuously()
    {
        _spawning = true;

        while (_state == WaveState.Active)
        {
            // pause if too many enemies alive
            if (GameObject.FindGameObjectsWithTag("Enemy").Length >= _maxEnemiesOnField)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // how far through the current 3-minute section we are (0 → 1)
            float sectionTime = _elapsedTime % _waveDuration;
            float progress = sectionTime / _waveDuration;

            float minDelay = _minSpawnDelay * _spawnDelayModifier;
            float maxDelay = _maxSpawnDelay * _spawnDelayModifier;

            // start slower, end faster as progress rises
            float dynamicDelay = Mathf.Lerp(maxDelay, minDelay, progress);
            float delay = Random.Range(dynamicDelay * 0.8f, dynamicDelay * 1.2f);


            SpawnEnemy(_enemyPrefab);
            yield return new WaitForSeconds(delay);
        }

        _spawning = false;
    }

    private IEnumerator ShowWaveCountdown()
    {
        if (_countdownText == null)
            yield break;

        _countdownText.text = $"Wave {_waveNumber}";
        float fadeIn = 0.5f, hold = 1.5f, fadeOut = 0.5f;

        for (float t = 0; t < fadeIn; t += Time.deltaTime)
        {
            _countdownText.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            _countdownText.transform.localScale = Vector3.one * (1f + 0.2f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        _countdownText.alpha = 1f;
        _countdownText.transform.localScale = Vector3.one;
        yield return new WaitForSeconds(hold);

        for (float t = 0; t < fadeOut; t += Time.deltaTime)
        {
            _countdownText.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }

        _countdownText.alpha = 0f;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Vector2 p = GetRandomOffscreenPosition();
        Vector3 spawnPos = new Vector3(p.x, p.y, 0f); // force Z=0
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        float progress = Mathf.Clamp01(1f - (_waveTimer / _waveDuration));
        float waveScale = 1f + (_waveNumber - 1) * 0.1f; // +10% per wave
        float timeScale = 1f + progress * 0.1f;          // +10% within wave
        if (enemy.TryGetComponent(out EnemyHealth h))
            h.SetHealthScale(waveScale * timeScale);
    }


    private Vector2 GetRandomSpawnPosition()
    {
        if (_mapBounds == null || _mapBounds.Length == 0)
            return Vector2.zero;

        // pick one of the colliders randomly
        var chosen = _mapBounds[Random.Range(0, _mapBounds.Length)];
        Bounds b = chosen.bounds;

        // pick a random point inside that collider’s bounds
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        // camera rect in world
        Vector3 bl = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float camMinX = bl.x, camMaxX = tr.x, camMinY = bl.y, camMaxY = tr.y;

        // playable inner area from walls
        Bounds play;
        bool hasPlay = TryGetPlayArea(out play);
        if (!hasPlay)
        {
            // fallback: old behavior if walls not set
            int sideF = Random.Range(0, 4);
            return sideF switch
            {
                0 => new Vector2(Random.Range(camMinX, camMaxX), camMaxY + _spawnOffset),
                1 => new Vector2(Random.Range(camMinX, camMaxX), camMinY - _spawnOffset),
                2 => new Vector2(camMinX - _spawnOffset, Random.Range(camMinY, camMaxY)),
                _ => new Vector2(camMaxX + _spawnOffset, Random.Range(camMinY, camMaxY)),
            };
        }

        // clamp the camera span to the playable rect so we never pick a coordinate outside play
        float xSpanMin = Mathf.Max(camMinX, play.min.x);
        float xSpanMax = Mathf.Min(camMaxX, play.max.x);
        float ySpanMin = Mathf.Max(camMinY, play.min.y);
        float ySpanMax = Mathf.Min(camMaxY, play.max.y);

        // choose a side; if that side would push us beyond play area, stick to the inner edge
        int side = Random.Range(0, 4);
        Vector2 spawn = Vector2.zero;

        switch (side)
        {
            case 0: // top: just above camera if possible, else at play's top edge
                spawn.x = (xSpanMin <= xSpanMax) ? Random.Range(xSpanMin, xSpanMax) : Mathf.Clamp((camMinX + camMaxX) * 0.5f, play.min.x, play.max.x);
                spawn.y = Mathf.Min(camMaxY + _spawnOffset, play.max.y);
                break;

            case 1: // bottom
                spawn.x = (xSpanMin <= xSpanMax) ? Random.Range(xSpanMin, xSpanMax) : Mathf.Clamp((camMinX + camMaxX) * 0.5f, play.min.x, play.max.x);
                spawn.y = Mathf.Max(camMinY - _spawnOffset, play.min.y);
                break;

            case 2: // left
                spawn.x = Mathf.Max(camMinX - _spawnOffset, play.min.x);
                spawn.y = (ySpanMin <= ySpanMax) ? Random.Range(ySpanMin, ySpanMax) : Mathf.Clamp((camMinY + camMaxY) * 0.5f, play.min.y, play.max.y);
                break;

            default: // right
                spawn.x = Mathf.Min(camMaxX + _spawnOffset, play.max.x);
                spawn.y = (ySpanMin <= ySpanMax) ? Random.Range(ySpanMin, ySpanMax) : Mathf.Clamp((camMinY + camMaxY) * 0.5f, play.min.y, play.max.y);
                break;
        }

        // final safety clamp: ensure strictly inside play area
        spawn.x = Mathf.Clamp(spawn.x, play.min.x, play.max.x);
        spawn.y = Mathf.Clamp(spawn.y, play.min.y, play.max.y);

        return spawn;
    }



    
    #region Play Area From 4 Walls
    private bool TryGetPlayArea(out Bounds inner)
    {
        inner = default;

        if (_mapBounds == null || _mapBounds.Length < 4)
            return false;

        // identify walls by extremal positions
        BoxCollider2D left = _mapBounds[0];
        BoxCollider2D right = _mapBounds[0];
        BoxCollider2D top = _mapBounds[0];
        BoxCollider2D bottom = _mapBounds[0];

        foreach (var c in _mapBounds)
        {
            if (c == null) continue;
            var b = c.bounds;
            if (b.center.x < left.bounds.center.x) left = c;
            if (b.center.x > right.bounds.center.x) right = c;
            if (b.center.y > top.bounds.center.y) top = c;
            if (b.center.y < bottom.bounds.center.y) bottom = c;
        }

        // inner playable area is between the inner faces of these walls
        float minX = left.bounds.max.x + _spawnInnerMargin;
        float maxX = right.bounds.min.x - _spawnInnerMargin;
        float minY = bottom.bounds.max.y + _spawnInnerMargin;
        float maxY = top.bounds.min.y - _spawnInnerMargin;

        if (minX >= maxX || minY >= maxY) return false;

        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(Mathf.Max(0.001f, maxX - minX),
                                       Mathf.Max(0.001f, maxY - minY),
                                       1f);
        inner = new Bounds(center, size);
        return true;
    }
    #endregion




    private void UpdateUI()
    {
        if (_waveText != null)
            _waveText.text = $"Wave {_waveNumber}\nTotal Kills: {_totalKills}";
    }

    private void UpdateTimerUI()
{
    if (_timerText != null)
    {
        int minutes = Mathf.FloorToInt(_elapsedTime / 60);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60);
        _timerText.text = $"{minutes:00}:{seconds:00}";
    }
}

}

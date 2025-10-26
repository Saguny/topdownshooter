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

    [Header("Map Walls (4 BoxCollider2D: Left, Right, Bottom, Top)")]
    [SerializeField] private BoxCollider2D[] _mapBounds = new BoxCollider2D[4];
    [SerializeField] private float _spawnInnerMargin = 0.25f;

    [Header("Wave Settings")]
    [SerializeField] private float _waveDuration = 180f; // 3 minutes
    [SerializeField] private float _timeBetweenWaves = 5f;
    [SerializeField] private int _killsToClear = 25;
    [SerializeField] private float _minSpawnDelay = 0.5f;
    [SerializeField] private float _maxSpawnDelay = 5f;
    [SerializeField] private int _maxEnemiesOnField = 25;
    [SerializeField] private float _spawnOffset = 2f;

    private float _elapsedTime = 0f;
    private float _nextCheckpoint = 180f;
    private int _waveNumber = 0;
    private int _totalKills = 0;
    private int _waveKills = 0;
    private bool _spawning = false;
    private WaveState _state = WaveState.WaitingForNext;

    // persistent scaling
    private float _enemyHealthScale = 1f;
    private float _spawnRateScale = 1f;
    private int _enemyCap;

    // UI colors
    private Color _normalColor = Color.white;
    private Color _pausedColor = new Color(1f, 0.3f, 0.3f);

    private void OnEnable() => EnemyHealth.OnEnemyDied += HandleEnemyDeath;
    private void OnDisable() => EnemyHealth.OnEnemyDied -= HandleEnemyDeath;

    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_countdownText != null)
            _countdownText.alpha = 0f;

        _waveNumber = 0;
        _elapsedTime = 0f;
        _nextCheckpoint = _waveDuration; // first wave checkpoint at 3min
        _enemyCap = _maxEnemiesOnField;
        _state = WaveState.WaitingForNext;

        StartCoroutine(StartNextWave());
    }

    private void Update()
    {
        if (_state == WaveState.Active)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerUI(_normalColor);

            // reached next checkpoint (3min, 6min, 9min...)
            if (_elapsedTime >= _nextCheckpoint)
            {
                _state = WaveState.Clearing;
                _spawning = false;
                UpdateTimerUI(_pausedColor);
            }
        }
        else
        {
            // paused states, timer doesn't increase
            UpdateTimerUI(_pausedColor);
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

            // persistent scaling
            _enemyHealthScale *= 1.1f;
            _spawnRateScale *= 1.05f;
            _enemyCap += 5;

            _waveNumber++;
            _waveKills = 0;
            _killsToClear += 25;

            // next checkpoint = waveNumber * waveDuration (cumulative)
            _nextCheckpoint = _waveNumber * _waveDuration;

            _state = WaveState.WaitingForNext;
            StartCoroutine(WaveCooldown());
        }
    }

    private IEnumerator WaveCooldown()
    {
        UpdateTimerUI(_pausedColor);
        PullAllGearsToPlayer();

        yield return new WaitForSeconds(_timeBetweenWaves);
        StartCoroutine(StartNextWave());
    }

    private IEnumerator StartNextWave()
    {
        _state = WaveState.WaitingForNext;
        yield return new WaitForSeconds(_timeBetweenWaves);

        _waveNumber++;
        yield return StartCoroutine(ShowWaveCountdown());

        _state = WaveState.Active;
        UpdateTimerUI(_normalColor);
        UpdateUI();

        StartCoroutine(SpawnEnemiesContinuously());
    }

    private IEnumerator SpawnEnemiesContinuously()
    {
        _spawning = true;

        while (_state == WaveState.Active)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length >= _enemyCap)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float sectionTime = _elapsedTime % _waveDuration;
            float progress = sectionTime / _waveDuration;

            float minDelay = _minSpawnDelay / _spawnRateScale;
            float maxDelay = _maxSpawnDelay / _spawnRateScale;

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
        Vector3 spawnPos = new Vector3(p.x, p.y, 0f);
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (enemy.TryGetComponent(out EnemyHealth h))
            h.SetHealthScale(_enemyHealthScale);
    }

    private void PullAllGearsToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        foreach (var gear in GameObject.FindGameObjectsWithTag("Gear"))
            StartCoroutine(MoveGearToPlayer(gear.transform, player.transform));
    }

    private IEnumerator MoveGearToPlayer(Transform gear, Transform player)
    {
        float duration = 0.6f;
        float t = 0f;
        Vector3 startPos = gear.position;

        while (t < duration && gear != null && player != null)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            gear.position = Vector3.Lerp(startPos, player.position, progress * progress);
            yield return null;
        }

        if (gear != null)
            Destroy(gear.gameObject);
    }

    #region Spawn Area
    private Vector2 GetRandomOffscreenPosition()
    {
        Vector3 bl = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float camMinX = bl.x, camMaxX = tr.x, camMinY = bl.y, camMaxY = tr.y;

        if (!TryGetPlayArea(out Bounds play))
        {
            int side = Random.Range(0, 4);
            return side switch
            {
                0 => new Vector2(Random.Range(camMinX, camMaxX), camMaxY + _spawnOffset),
                1 => new Vector2(Random.Range(camMinX, camMaxX), camMinY - _spawnOffset),
                2 => new Vector2(camMinX - _spawnOffset, Random.Range(camMinY, camMaxY)),
                _ => new Vector2(camMaxX + _spawnOffset, Random.Range(camMinY, camMaxY)),
            };
        }

        float xSpanMin = Mathf.Max(camMinX, play.min.x);
        float xSpanMax = Mathf.Min(camMaxX, play.max.x);
        float ySpanMin = Mathf.Max(camMinY, play.min.y);
        float ySpanMax = Mathf.Min(camMaxY, play.max.y);

        int s = Random.Range(0, 4);
        Vector2 spawn = Vector2.zero;

        switch (s)
        {
            case 0:
                spawn.x = Random.Range(xSpanMin, xSpanMax);
                spawn.y = Mathf.Min(camMaxY + _spawnOffset, play.max.y);
                break;
            case 1:
                spawn.x = Random.Range(xSpanMin, xSpanMax);
                spawn.y = Mathf.Max(camMinY - _spawnOffset, play.min.y);
                break;
            case 2:
                spawn.x = Mathf.Max(camMinX - _spawnOffset, play.min.x);
                spawn.y = Random.Range(ySpanMin, ySpanMax);
                break;
            default:
                spawn.x = Mathf.Min(camMaxX + _spawnOffset, play.max.x);
                spawn.y = Random.Range(ySpanMin, ySpanMax);
                break;
        }

        spawn.x = Mathf.Clamp(spawn.x, play.min.x, play.max.x);
        spawn.y = Mathf.Clamp(spawn.y, play.min.y, play.max.y);
        return spawn;
    }

    private bool TryGetPlayArea(out Bounds inner)
    {
        inner = default;
        if (_mapBounds == null || _mapBounds.Length < 4)
            return false;

        BoxCollider2D left = _mapBounds[0], right = _mapBounds[0], top = _mapBounds[0], bottom = _mapBounds[0];

        foreach (var c in _mapBounds)
        {
            if (c == null) continue;
            var b = c.bounds;
            if (b.center.x < left.bounds.center.x) left = c;
            if (b.center.x > right.bounds.center.x) right = c;
            if (b.center.y > top.bounds.center.y) top = c;
            if (b.center.y < bottom.bounds.center.y) bottom = c;
        }

        float minX = left.bounds.max.x + _spawnInnerMargin;
        float maxX = right.bounds.min.x - _spawnInnerMargin;
        float minY = bottom.bounds.max.y + _spawnInnerMargin;
        float maxY = top.bounds.min.y - _spawnInnerMargin;

        if (minX >= maxX || minY >= maxY) return false;

        Vector3 center = new((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new(Mathf.Max(0.001f, maxX - minX),
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

    private void UpdateTimerUI(Color color)
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_elapsedTime / 60);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60);
        _timerText.text = $"{minutes:00}:{seconds:00}";
        _timerText.color = color;
    }
}

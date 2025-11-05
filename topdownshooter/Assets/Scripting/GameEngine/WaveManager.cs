using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    private enum WaveState
    {
        Active,
        FinalRush,
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
    [SerializeField] private float _waveDuration = 180f;
    [SerializeField] private float _timeBetweenWaves = 3f;
    [SerializeField] private int _baseKillsToClear = 25;
    [SerializeField] private float _minSpawnDelay = 0.5f;
    [SerializeField] private float _maxSpawnDelay = 5f;
    [SerializeField] private int _maxEnemiesOnField = 25;
    [SerializeField] private float _spawnOffset = 2f;

    [Header("Final Rush Settings")]
    [SerializeField] private float _finalRushSpawnDelay = 0.18f;
    [SerializeField] private float _gearPickupDelay = 0.8f;
    [SerializeField] private float _postPickupDelay = 1.2f;

    // State
    private WaveState _state = WaveState.WaitingForNext;
    private bool _spawning = false;

    // Timers
    private float _elapsedTime = 0f;     // cumulative time (never reset)
    private float _nextCheckpoint = 0f;  // += _waveDuration each wave end

    // Wave & kills
    private int _waveNumber = 0;         // number of completed waves
    private int _totalKills = 0;
    private int _waveKills = 0;          // used only during FinalRush

    // Scaling across waves
    private float _enemyHealthScale = 1f;
    private float _enemySpeedScale = 1f;
    private float _enemyDamageScale = 1f;
    private float _spawnRateScale = 1f;
    private int _enemyCap;

    // UI colors
    private Color _normalColor = Color.white;
    private Color _pausedColor = new Color(1f, 0.3f, 0.3f);

    private void OnEnable() => EnemyHealth.OnEnemyDied += HandleEnemyDeath;
    private void OnDisable() => EnemyHealth.OnEnemyDied -= HandleEnemyDeath;

    private void Start()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_countdownText != null) _countdownText.alpha = 0f;

        _enemyCap = _maxEnemiesOnField;

        // first checkpoint is one wave duration
        _nextCheckpoint = _waveDuration;

        // start Wave 1
        StartCoroutine(BeginWaveRoutine());
    }

    private void Update()
    {
        if (_state == WaveState.Active)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerUI();

            // every 3 minutes enter FinalRush
            if (_elapsedTime >= _nextCheckpoint)
            {
                EnterFinalRush();
            }
        }
        else
        {
            // paused color while not actively spawning
            UpdateTimerUI(_pausedColor);
        }
    }

    // ===== Flow =====

    private IEnumerator BeginWaveRoutine()
    {
        _state = WaveState.WaitingForNext;

        // show upcoming wave number
        yield return StartCoroutine(ShowWaveCountdown(_waveNumber + 1));

        _state = WaveState.Active;
        UpdateUI();

        // start normal spawning
        StartCoroutine(SpawnEnemiesContinuously());
    }

    private void EnterFinalRush()
    {
        if (_state != WaveState.Active) return;

        _state = WaveState.FinalRush;

        // reset kills for the FinalRush objective
        _waveKills = 0;

        // stop normal spawning
        _spawning = false;

        // start FinalRush spawning
        StartCoroutine(SpawnFinalRush());
    }

    private IEnumerator EndWaveSequence()
    {
        // avoid double entry
        if (_state == WaveState.Clearing) yield break;

        _state = WaveState.Clearing;

        // wipe remaining enemies
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        // wait then pull all gears
        yield return new WaitForSeconds(_gearPickupDelay);
        PullAllGearsToPlayer();

        // short wait after pickup
        yield return new WaitForSeconds(_postPickupDelay);

        // apply scaling for next wave
        ApplyWaveScaling();

        // schedule next checkpoint (timer is cumulative)
        _nextCheckpoint += _waveDuration;

        // increment completed wave count
        _waveNumber += 1;

        // prep next wave
        _state = WaveState.WaitingForNext;

        yield return new WaitForSeconds(_timeBetweenWaves);

        // begin next wave
        StartCoroutine(BeginWaveRoutine());
    }

    // ===== Kill handling =====

    private void HandleEnemyDeath()
    {
        _totalKills++;

        if (_state == WaveState.FinalRush)
        {
            _waveKills++;

            if (_waveKills >= GetKillsToClear())
            {
                StartCoroutine(EndWaveSequence());
            }
        }

        UpdateUI();
    }

    // ===== Spawning =====

    private IEnumerator SpawnEnemiesContinuously()
    {
        _spawning = true;

        while (_state == WaveState.Active)
        {
            // pause if too many enemies alive
            if (GameObject.FindGameObjectsWithTag("Enemy").Length >= _maxEnemiesOnField)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }

            // progress within current wave window [0, _waveDuration]
            float sectionStart = _nextCheckpoint - _waveDuration;
            float sectionTime = Mathf.Clamp(_elapsedTime - sectionStart, 0f, _waveDuration);
            float progress = sectionTime / _waveDuration;

            float minDelay = _minSpawnDelay / _spawnRateScale;
            float maxDelay = _maxSpawnDelay / _spawnRateScale;
            float dynamicDelay = Mathf.Lerp(maxDelay, minDelay, progress);

            SpawnEnemy(_enemyPrefab);
            yield return new WaitForSeconds(Random.Range(dynamicDelay * 0.8f, dynamicDelay * 1.2f));
        }

        _spawning = false;
    }

    private IEnumerator SpawnFinalRush()
    {
        // keep the field filled up to cap, replenish quickly
        while (_state == WaveState.FinalRush)
        {
            int current = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (current < _enemyCap)
            {
                SpawnEnemy(_enemyPrefab);
            }

            // fast spawn during FinalRush (accelerated by spawn rate scale)
            float d = Mathf.Max(0.05f, _finalRushSpawnDelay / _spawnRateScale);
            yield return new WaitForSeconds(d);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Vector2 pos = GetRandomOffscreenPosition();
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        if (enemy.TryGetComponent(out EnemyHealth h))
            h.SetHealthScale(_enemyHealthScale);

        if (enemy.TryGetComponent(out EnemyMovement m))
            m.SetSpeedMultiplier(_enemySpeedScale);

        if (enemy.TryGetComponent(out EnemyDamage a))
            a.SetDamageMultiplier(_enemyDamageScale);
    }

    // ===== Scaling =====

    private void ApplyWaveScaling()
    {
        _enemyHealthScale *= 1.35f;
        _enemySpeedScale *= 1.20f;
        _enemyDamageScale *= 1.25f;
        _spawnRateScale *= 1.15f;
        _enemyCap += 10;
    }

    private int GetKillsToClear()
    {
        // example progression: 25, 40, 55, 70, ...
        return _baseKillsToClear + (_waveNumber * 15);
    }

    // ===== Gear pickup =====

    private void PullAllGearsToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        var gears = GameObject.FindGameObjectsWithTag("Gear");
        foreach (var g in gears)
        {
            if (g != null)
                StartCoroutine(MoveGearToPlayer(g.transform, player));
        }
    }


    private IEnumerator MoveGearToPlayer(Transform gear, Transform player)
    {
        float duration = 0.6f;
        float t = 0f;
        Vector3 start = gear.position;

        while (t < duration && gear != null && player != null)
        {
            t += Time.deltaTime;
            float p = t / duration;
            gear.position = Vector3.Lerp(start, player.position, p * p);
            yield return null;
        }

        if (gear != null) Destroy(gear.gameObject);
    }

    // ===== UI =====

    private IEnumerator ShowWaveCountdown(int nextWaveNumber)
    {
        if (_countdownText == null) yield break;

        _countdownText.text = $"Wave {nextWaveNumber}";
        float fadeIn = 0.4f, hold = 1.0f, fadeOut = 0.4f;

        for (float t = 0; t < fadeIn; t += Time.deltaTime)
        {
            _countdownText.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            yield return null;
        }

        yield return new WaitForSeconds(hold);

        for (float t = 0; t < fadeOut; t += Time.deltaTime)
        {
            _countdownText.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }

        _countdownText.alpha = 0f;
    }

    private void UpdateUI()
    {
        if (_waveText != null)
            _waveText.text = $"Wave {_waveNumber + 1}\nTotal Kills: {_totalKills}";
    }

    private void UpdateTimerUI() => UpdateTimerUI(_normalColor);

    private void UpdateTimerUI(Color color)
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";
        _timerText.color = color;
    }

    // ===== Spawn positions =====

    private Vector2 GetRandomOffscreenPosition()
    {
        Vector3 bl = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float camMinX = bl.x, camMaxX = tr.x, camMinY = bl.y, camMaxY = tr.y;

        // try to confine to playable area defined by walls
        if (TryGetPlayArea(out var play))
        {
            float xSpanMin = Mathf.Max(camMinX, play.min.x);
            float xSpanMax = Mathf.Min(camMaxX, play.max.x);
            float ySpanMin = Mathf.Max(camMinY, play.min.y);
            float ySpanMax = Mathf.Min(camMaxY, play.max.y);

            int side = Random.Range(0, 4);
            Vector2 spawn = Vector2.zero;

            switch (side)
            {
                case 0: // top
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

            spawn.x = Mathf.Clamp(spawn.x, play.min.x, play.max.x);
            spawn.y = Mathf.Clamp(spawn.y, play.min.y, play.max.y);
            return spawn;
        }

        // fallback: spawn just outside camera bounds
        int sideF = Random.Range(0, 4);
        return sideF switch
        {
            0 => new Vector2(Random.Range(camMinX, camMaxX), camMaxY + _spawnOffset),
            1 => new Vector2(Random.Range(camMinX, camMaxX), camMinY - _spawnOffset),
            2 => new Vector2(camMinX - _spawnOffset, Random.Range(camMinY, camMaxY)),
            _ => new Vector2(camMaxX + _spawnOffset, Random.Range(camMinY, camMaxY)),
        };
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

        // inner playable area between inner faces of walls
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
}

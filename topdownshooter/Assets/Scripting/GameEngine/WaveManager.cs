using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    private enum WaveState
    {
        Active,         // 通常スポーン中
        FinalRush,      // 3分到達後の討伐フェーズ
        Clearing,       // 残党処理＆ギア回収
        WaitingForNext  // 次ウェーブ待ち
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    [SerializeField] private float _waveDuration = 180f;       // 3 minutes
    [SerializeField] private float _timeBetweenWaves = 5f;
    [SerializeField] private int _killsToClear = 25;
=======
    [SerializeField] private float _waveDuration = 180f;        // 各ウェーブの時間
    [SerializeField] private float _timeBetweenWaves = 3f;      // カウントダウン前の待機
    [SerializeField] private int _baseKillsToClear = 25;        // FinalRush 目標キルの基準
>>>>>>> Stashed changes
=======
    [SerializeField] private float _waveDuration = 180f;        // 各ウェーブの時間
    [SerializeField] private float _timeBetweenWaves = 3f;      // カウントダウン前の待機
    [SerializeField] private int _baseKillsToClear = 25;        // FinalRush 目標キルの基準
>>>>>>> Stashed changes
    [SerializeField] private float _minSpawnDelay = 0.5f;
    [SerializeField] private float _maxSpawnDelay = 5f;
    [SerializeField] private int _maxEnemiesOnField = 25;      // cap to avoid overflow
    [SerializeField] private float _spawnOffset = 2f;          // used if no mapBounds

<<<<<<< Updated upstream
<<<<<<< Updated upstream
    private float _elapsedTime = 0f;
    private float _nextCheckpoint = 180f;

    private int _waveNumber = 0;
    private int _totalKills = 0;
    private int _waveKills = 0;
    private float _waveTimer;
    private bool _spawning = false;
    private WaveState _state = WaveState.WaitingForNext;
    private float _spawnDelayModifier = 1f;
=======
=======
>>>>>>> Stashed changes
    [Header("Final Rush Settings")]
    [SerializeField] private float _finalRushSpawnDelay = 0.18f; // FinalRush の基本スポーン間隔
    [SerializeField] private float _gearPickupDelay = 0.8f;      // 残党処理→ギア吸引までの待ち
    [SerializeField] private float _postPickupDelay = 1.2f;      // ギア吸引後→次ウェーブ開始前の待ち

    // 状態
    private WaveState _state = WaveState.WaitingForNext;
    private bool _spawning = false;

    // タイマー
    private float _elapsedTime = 0f;      // 累積（リセットしない）
    private float _nextCheckpoint = 0f;   // 次の停止時刻（+= waveDuration）

    // ウェーブ・キル
    private int _waveNumber = 0;          // 現在までに完了したウェーブ数
    private int _totalKills = 0;
    private int _waveKills = 0;           // FinalRush 専用のカウント

    // 永続スケーリング
    private float _enemyHealthScale = 1f;
    private float _enemySpeedScale = 1f;
    private float _enemyDamageScale = 1f;
    private float _spawnRateScale = 1f;
    private int _enemyCap;

    // UI 色
    private Color _normalColor = Color.white;
    private Color _pausedColor = new Color(1f, 0.3f, 0.3f);
>>>>>>> Stashed changes

    private void OnEnable() => EnemyHealth.OnEnemyDied += HandleEnemyDeath;
    private void OnDisable() => EnemyHealth.OnEnemyDied -= HandleEnemyDeath;

    private void Start()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_countdownText != null) _countdownText.alpha = 0f;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        if (_countdownText != null)
            _countdownText.alpha = 0f;

        // set up first wave correctly
        _waveNumber = 0;
        _elapsedTime = 0f;
        _nextCheckpoint = _waveDuration; // 180 seconds
        _state = WaveState.WaitingForNext;
=======
        _enemyCap = _maxEnemiesOnField;
>>>>>>> Stashed changes
=======
        _enemyCap = _maxEnemiesOnField;
>>>>>>> Stashed changes

        // 最初のチェックポイントは 180 秒
        _nextCheckpoint = _waveDuration;

        // Wave 1 を開始
        StartCoroutine(BeginWaveRoutine());
    }


    private void Update()
    {
        if (_state == WaveState.Active)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerUI();

<<<<<<< Updated upstream
<<<<<<< Updated upstream
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
=======
            // 3分到達ごとに FinalRush へ
            if (_elapsedTime >= _nextCheckpoint)
            {
=======
            // 3分到達ごとに FinalRush へ
            if (_elapsedTime >= _nextCheckpoint)
            {
>>>>>>> Stashed changes
                EnterFinalRush();
            }
        }
        else
        {
            // 停止色
            UpdateTimerUI(_pausedColor);
        }
    }

    // ===== フロー遷移 =====

    private IEnumerator BeginWaveRoutine()
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    {
        _state = WaveState.WaitingForNext;

        // 表示は「これから始まるウェーブ番号」
        yield return StartCoroutine(ShowWaveCountdown(_waveNumber + 1));

        _state = WaveState.Active;
        UpdateUI();

<<<<<<< Updated upstream
<<<<<<< Updated upstream
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
=======
        // 通常スポーン開始
        StartCoroutine(SpawnEnemiesContinuously());
    }

=======
        // 通常スポーン開始
        StartCoroutine(SpawnEnemiesContinuously());
    }

>>>>>>> Stashed changes
    private void EnterFinalRush()
    {
        if (_state != WaveState.Active) return;

        _state = WaveState.FinalRush;

        // ここでキル数をゼロリセット。これがなかったせいで一撃で達成になっていた
        _waveKills = 0;

        // 通常スポーン停止
        _spawning = false;

        // FinalRush スポーン開始
        StartCoroutine(SpawnFinalRush());
    }

    private IEnumerator EndWaveSequence()
    {
        // 二重呼び出し防止
        if (_state == WaveState.Clearing) yield break;

        _state = WaveState.Clearing;

        // 残党を全滅
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        // 少し待ってからギア吸引
        yield return new WaitForSeconds(_gearPickupDelay);
        PullAllGearsToPlayer();

        // 吸引後の待機
        yield return new WaitForSeconds(_postPickupDelay);

        // 次ウェーブに向けたスケーリングをここで適用
        ApplyWaveScaling();

        // 次の 3 分チェックポイントを積み増し（タイマーは累積のまま）
        _nextCheckpoint += _waveDuration;

        // ウェーブ番号はここで一回だけ増やす
        _waveNumber += 1;

        // 次ウェーブの準備待ち
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        _state = WaveState.WaitingForNext;

        yield return new WaitForSeconds(_timeBetweenWaves);

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        _waveNumber++; // increment before showing "Wave X"
        yield return StartCoroutine(ShowWaveCountdown());

        _state = WaveState.Active;

        // make sure we show time, not "Kill X Enemies"
        UpdateTimerUI();
        UpdateUI();

        _spawnDelayModifier = Mathf.Max(0.5f, 1f - _waveNumber * 0.05f);
        StartCoroutine(SpawnEnemiesContinuously());
    }

=======
        // 次ウェーブ開始
        StartCoroutine(BeginWaveRoutine());
    }

=======
        // 次ウェーブ開始
        StartCoroutine(BeginWaveRoutine());
    }

>>>>>>> Stashed changes
    // ===== キル処理 =====

    private void HandleEnemyDeath()
    {
        _totalKills++;

        // FinalRush 中のみ達成判定に使う
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

    // ===== スポーン =====
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
<<<<<<< Updated upstream
            // how far through the current 3-minute section we are (0 → 1)
            float sectionTime = _elapsedTime % _waveDuration;
=======
            // ウェーブ内進行度で遅延を補間（序盤は遅く、終盤は速く）
            float sectionTime = Mathf.Clamp(_elapsedTime - (_nextCheckpoint - _waveDuration), 0f, _waveDuration);
>>>>>>> Stashed changes
            float progress = sectionTime / _waveDuration;

            float minDelay = _minSpawnDelay * _spawnDelayModifier;
            float maxDelay = _maxSpawnDelay * _spawnDelayModifier;

            // start slower, end faster as progress rises
=======
            // ウェーブ内進行度で遅延を補間（序盤は遅く、終盤は速く）
            float sectionTime = Mathf.Clamp(_elapsedTime - (_nextCheckpoint - _waveDuration), 0f, _waveDuration);
            float progress = sectionTime / _waveDuration;

            float minDelay = _minSpawnDelay / _spawnRateScale;
            float maxDelay = _maxSpawnDelay / _spawnRateScale;
<<<<<<< Updated upstream
>>>>>>> Stashed changes
            float dynamicDelay = Mathf.Lerp(maxDelay, minDelay, progress);

=======
            float dynamicDelay = Mathf.Lerp(maxDelay, minDelay, progress);
>>>>>>> Stashed changes

            SpawnEnemy(_enemyPrefab);
            yield return new WaitForSeconds(Random.Range(dynamicDelay * 0.8f, dynamicDelay * 1.2f));
        }

        _spawning = false;
    }

    private IEnumerator SpawnFinalRush()
    {
        // FinalRush 中は常にキャップまで詰めて、高速で補充
        while (_state == WaveState.FinalRush)
<<<<<<< Updated upstream
        {
            int current = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (current < _enemyCap)
            {
                SpawnEnemy(_enemyPrefab);
            }

            // FinalRush はスポーンが速い（スケールで更に速く）
            float d = Mathf.Max(0.05f, _finalRushSpawnDelay / _spawnRateScale);
            yield return new WaitForSeconds(d);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
<<<<<<< Updated upstream
        Vector2 p = GetRandomOffscreenPosition();
        Vector3 spawnPos = new Vector3(p.x, p.y, 0f); // force Z=0
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
=======
        Vector2 pos = GetRandomOffscreenPosition();
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
>>>>>>> Stashed changes

        float progress = Mathf.Clamp01(1f - (_waveTimer / _waveDuration));
        float waveScale = 1f + (_waveNumber - 1) * 0.1f; // +10% per wave
        float timeScale = 1f + progress * 0.1f;          // +10% within wave
        if (enemy.TryGetComponent(out EnemyHealth h))
<<<<<<< Updated upstream
            h.SetHealthScale(waveScale * timeScale);
    }


    private Vector2 GetRandomSpawnPosition()
=======
            h.SetHealthScale(_enemyHealthScale);   // 体力

        if (enemy.TryGetComponent(out EnemyMovement m))
            m.SetSpeedMultiplier(_enemySpeedScale); // 移動速度

        if (enemy.TryGetComponent(out EnemyDamage a))
            a.SetDamageMultiplier(_enemyDamageScale); // ダメージ
    }

    // ===== スケーリング =====

    private void ApplyWaveScaling()
    {
        // 要望通り「大幅」に強化
        _enemyHealthScale *= 1.35f;
        _enemySpeedScale *= 1.20f;
        _enemyDamageScale *= 1.25f;
        _spawnRateScale *= 1.15f;
        _enemyCap += 10;
    }

    private int GetKillsToClear()
    {
        // 波ごとに必要キル増加（例：25, 40, 55, 70, ...）
        // 好みに応じて係数は調整可
        return _baseKillsToClear + (_waveNumber * 15);
    }

    // ===== ギア吸引 =====

    private void PullAllGearsToPlayer()
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
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

    // ===== 表示/UI =====

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
=======
        {
            int current = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (current < _enemyCap)
            {
                SpawnEnemy(_enemyPrefab);
            }

            // FinalRush はスポーンが速い（スケールで更に速く）
            float d = Mathf.Max(0.05f, _finalRushSpawnDelay / _spawnRateScale);
            yield return new WaitForSeconds(d);
        }
>>>>>>> Stashed changes
    }

    private void UpdateUI()
    {
<<<<<<< Updated upstream
        if (_waveText != null)
            _waveText.text = $"Wave {_waveNumber + 1}\nTotal Kills: {_totalKills}";
    }

    private void UpdateTimerUI(Color color)
=======
        Vector2 pos = GetRandomOffscreenPosition();
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        if (enemy.TryGetComponent(out EnemyHealth h))
            h.SetHealthScale(_enemyHealthScale);   // 体力

        if (enemy.TryGetComponent(out EnemyMovement m))
            m.SetSpeedMultiplier(_enemySpeedScale); // 移動速度

        if (enemy.TryGetComponent(out EnemyDamage a))
            a.SetDamageMultiplier(_enemyDamageScale); // ダメージ
    }

    // ===== スケーリング =====

    private void ApplyWaveScaling()
    {
        // 要望通り「大幅」に強化
        _enemyHealthScale *= 1.35f;
        _enemySpeedScale *= 1.20f;
        _enemyDamageScale *= 1.25f;
        _spawnRateScale *= 1.15f;
        _enemyCap += 10;
    }

    private int GetKillsToClear()
    {
        // 波ごとに必要キル増加（例：25, 40, 55, 70, ...）
        // 好みに応じて係数は調整可
        return _baseKillsToClear + (_waveNumber * 15);
    }

    // ===== ギア吸引 =====

    private void PullAllGearsToPlayer()
>>>>>>> Stashed changes
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";
        _timerText.color = color;
    }

<<<<<<< Updated upstream
    // ===== スポーン位置 =====

>>>>>>> Stashed changes
=======
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

    // ===== 表示/UI =====

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

    private void UpdateTimerUI(Color color)
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60f);
        _timerText.text = $"{minutes:00}:{seconds:00}";
        _timerText.color = color;
    }

    // ===== スポーン位置 =====

>>>>>>> Stashed changes
    private Vector2 GetRandomOffscreenPosition()
    {
        // camera rect in world
        Vector3 bl = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float camMinX = bl.x, camMaxX = tr.x, camMinY = bl.y, camMaxY = tr.y;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
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
=======
        int side = Random.Range(0, 4);
        return side switch
        {
=======
        int side = Random.Range(0, 4);
        return side switch
        {
>>>>>>> Stashed changes
            0 => new Vector2(Random.Range(camMinX, camMaxX), camMaxY + _spawnOffset),
            1 => new Vector2(Random.Range(camMinX, camMaxX), camMinY - _spawnOffset),
            2 => new Vector2(camMinX - _spawnOffset, Random.Range(camMinY, camMaxY)),
            _ => new Vector2(camMaxX + _spawnOffset, Random.Range(camMinY, camMaxY)),
        };
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    }
}

}

using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private TextMeshProUGUI _waveText; // UI text for wave info

    [Header("Wave Settings")]
    [SerializeField] private float _timeBetweenWaves = 3f;
    [SerializeField] private float _spawnOffset = 2f;

    private int _waveNumber = 0;
    private int _enemiesAlive = 0;
    private int _totalKills = 0;
    private bool _spawning = false;

    private void OnEnable()
    {
        EnemyHealth.OnEnemyDied += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyDied -= HandleEnemyDeath;
    }

    private void Start()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        StartCoroutine(SpawnNextWave());
    }

    private void HandleEnemyDeath()
    {
        _totalKills++;
        _enemiesAlive--;
        UpdateUI();

        if (_enemiesAlive <= 0 && !_spawning)
        {
            StartCoroutine(SpawnNextWave());
        }
    }

    private IEnumerator SpawnNextWave()
    {
        _spawning = true;
        _waveNumber++;

        UpdateUI();

        yield return new WaitForSeconds(_timeBetweenWaves);

        if (_waveNumber % 5 == 0)
        {
            SpawnEnemy(_bossPrefab);
            _enemiesAlive = 1;
            Debug.Log($"Wave {_waveNumber}: Boss spawned!");
        }
        else
        {
            int enemyCount = 3 + _waveNumber;
            _enemiesAlive = enemyCount;

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy(_enemyPrefab);
                yield return new WaitForSeconds(1f);
            }
        }

        UpdateUI();
        _spawning = false;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        Vector2 spawnPos = GetRandomOffscreenPosition();
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetRandomOffscreenPosition()
    {
        int side = Random.Range(0, 4);
        Vector3 spawnWorldPos = Vector3.zero;

        Vector3 screenBottomLeft = _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, _mainCamera.nearClipPlane));
        Vector3 screenTopRight = _mainCamera.ViewportToWorldPoint(new Vector3(1, 1, _mainCamera.nearClipPlane));

        float xMin = screenBottomLeft.x;
        float xMax = screenTopRight.x;
        float yMin = screenBottomLeft.y;
        float yMax = screenTopRight.y;

        switch (side)
        {
            case 0: spawnWorldPos = new Vector3(Random.Range(xMin, xMax), yMax + _spawnOffset, 0); break;
            case 1: spawnWorldPos = new Vector3(Random.Range(xMin, xMax), yMin - _spawnOffset, 0); break;
            case 2: spawnWorldPos = new Vector3(xMin - _spawnOffset, Random.Range(yMin, yMax), 0); break;
            case 3: spawnWorldPos = new Vector3(xMax + _spawnOffset, Random.Range(yMin, yMax), 0); break;
        }

        return spawnWorldPos;
    }

    private void UpdateUI()
    {
        if (_waveText != null)
            _waveText.text = $"Wave {_waveNumber}\nKills: {_totalKills}";
    }
}

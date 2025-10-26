using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public GameObject bulletPrefab;

    [Header("Shooting Settings")]
    [SerializeField] public int bulletCount = 1;
    [SerializeField] public float fireRate = 0.25f;
    [SerializeField] private float spawnOffset = 0.1f; // extra margin outside collider

    private float _nextFireTime = 0f;
    private Collider2D _playerCollider;

    private void Awake()
    {
        _playerCollider = GetComponent<Collider2D>();
        if (_playerCollider == null)
            Debug.LogError("AutoShooter requires a Collider2D on the player!");
    }

    private void Update()
    {
        if (Time.time >= _nextFireTime)
        {
            ShootAtNearestEnemy();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private void ShootAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector2 direction = (nearestEnemy.transform.position - transform.position).normalized;

        // calculate spawn outside collider using bounds
        Vector2 spawnPos = (Vector2)transform.position +
                           direction * (_playerCollider.bounds.extents.magnitude + spawnOffset);

        // loop for multi-bullet upgrade
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            bullet.GetComponent<Bullet>().Shoot(direction);

            // rotate bullet visually
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            // check if enemy is visible in camera
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(enemy.transform.position);
            bool onScreen = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                            viewportPos.y >= 0 && viewportPos.y <= 1 &&
                            viewportPos.z > 0; // in front of camera

            if (!onScreen)
                continue; // skip enemies off-screen

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}

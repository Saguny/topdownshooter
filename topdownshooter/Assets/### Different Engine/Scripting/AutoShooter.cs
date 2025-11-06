using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] private AutoAimService aimer;

    [Header("Base Stats")]
    [SerializeField] public float baseFireRate = 3f;   // shots per second
    [SerializeField] public float baseBulletSpeed = 12f;

    private float cooldown;
    private StatContext stats;
    private Collider2D playerCollider;

    private void Awake()
    {
        stats = GetComponent<StatContext>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        float fireRate = baseFireRate * (stats ? stats.fireRateMul : 1f);
        if (fireRate <= 0f) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        var target = aimer != null ? aimer.FindTarget(transform.position) : null;
        if (target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;

        int extra = stats ? stats.bulletCountAdd : 0;
        int shots = Mathf.Max(1, 1 + extra);

        for (int i = 0; i < shots; i++)
            SpawnBullet(dir);

        cooldown = 1f / fireRate;
    }

    private void SpawnBullet(Vector2 dir)
    {
        var go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // speed with stat multiplier
        float speed = baseBulletSpeed * (stats ? stats.bulletSpeedMul : 1f);

        var bullet = go.GetComponent<Projectile>();
        if (bullet != null)
            bullet.Fire(dir, speed);

        // avoid hitting the player
        var bCol = go.GetComponent<Collider2D>();
        if (bCol != null && playerCollider != null)
            Physics2D.IgnoreCollision(bCol, playerCollider, true);
    }
}

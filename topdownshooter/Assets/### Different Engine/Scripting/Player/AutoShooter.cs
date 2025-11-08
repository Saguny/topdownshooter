using UnityEngine;
using System.Collections.Generic;

public class AutoShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] private AutoAimService aimer;

    [Header("Base Stats")]
    [SerializeField] public float baseFireRate = 3f;
    [SerializeField] public float baseBulletSpeed = 12f;
    [SerializeField] public float baseBulletDamage = 10f;

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

        int extra = stats ? stats.bulletCountAdd : 0;
        int shots = Mathf.Max(1, 1 + extra);

        List<Transform> targets = null;
        if (aimer != null) targets = aimer.FindTargets(transform.position, shots);
        if (targets == null || targets.Count == 0) return;

        Vector2 forward = (targets[0].position - transform.position).normalized;

        for (int i = 0; i < shots; i++)
        {
            Vector2 dir = (i < targets.Count && targets[i] != null)
                ? (targets[i].position - transform.position).normalized
                : forward;

            SpawnBullet(dir);
        }

        cooldown = 1f / fireRate;
    }

    private void SpawnBullet(Vector2 dir)
    {
        var go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        float speed = baseBulletSpeed * (stats ? stats.bulletSpeedMul : 1f);
        float damage = baseBulletDamage * (stats ? stats.bulletDamageMul : 1f);

        var bullet = go.GetComponent<Projectile>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);
            bullet.Fire(dir, speed);
        }

        var bCol = go.GetComponent<Collider2D>();
        if (bCol != null && playerCollider != null) Physics2D.IgnoreCollision(bCol, playerCollider, true);
    }
}

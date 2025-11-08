using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifeTime = 4f;

    [Header("Damage")]
    public float damage = 20f;
    public float radius = 2f; // AOE radius

    [Header("VFX")]
    public GameObject impactEffectPrefab;
    public float impactEffectDuration = 0.5f;

    private Transform target;

    public void Setup(float damageAmount, float radius)
    {
        this.damage = damageAmount;
        this.radius = radius;

        // bei start den nächstgelegenen gegner suchen
        target = FindNearestEnemy();

        // wenn einer da ist → auf ihn ausrichten
        if (target != null)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            transform.up = dir;
        }

        // immer nach 4 sekunden zerstören
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (target == null)
        {
            // kein ziel → geradeaus fliegen
            transform.position += transform.up * speed * Time.deltaTime;
            return;
        }

        // richtung zum ziel berechnen
        Vector3 dir = (target.position - transform.position);
        float distanceThisFrame = speed * Time.deltaTime;

        // falls target gestorben oder despawned → reset
        if (target == null)
            return;

        // bewegen
        transform.position += dir.normalized * distanceThisFrame;
        transform.up = dir.normalized;

        // falls nahe genug → explodieren
        if (dir.magnitude <= distanceThisFrame)
            Explode();
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e.transform;
            }
        }

        return nearest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            Explode();
    }

    private void Explode()
    {
        // schaden an alle gegner im radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
                enemy.TakeDamage(damage);
        }

        // VFX erstellen
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.localScale = Vector3.one * (radius * 2f);
            Destroy(fx, impactEffectDuration);
        }

        Destroy(gameObject);
    }
}

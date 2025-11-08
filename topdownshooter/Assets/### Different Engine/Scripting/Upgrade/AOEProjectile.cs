using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 20f;
    public float radius = 2f; // AOE Radius
    public GameObject impactEffectPrefab;
    public float impactEffectDuration = 0.5f;

    private Transform target;

    public void Setup(Transform target, float damageAmount, float radius)
    {
        this.target = target;
        this.damage = damageAmount;
        this.radius = radius;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        float moveStep = speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) <= moveStep)
            Explode();
        else
            transform.position += dir * moveStep;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            Explode();
    }

    private void Explode()
    {
        // Schaden auf alle Gegner im Radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
                enemy.TakeDamage(damage);
        }

        // VFX erstellen und automatisch skalieren
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

            // Annahme: Sprite hat ursprünglich Radius = 1
            float scale = radius * 2f; // *2 weil OverlapCircle Radius = halber Durchmesser
            fx.transform.localScale = new Vector3(scale, scale, 1f);

            Destroy(fx, impactEffectDuration);
        }

        Destroy(gameObject);
    }
}

using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    [Header("AOE Settings")]
    public float speed = 10f;
    public float damage = 20f;
    public GameObject impactEffectPrefab; // der weiße halbtransparente Kreis
    public float impactEffectDuration = 0.5f; // wie lange der Kreis sichtbar bleibt

    private Transform target;

    public void Setup(Transform target, float damageAmount)
    {
        this.target = target;
        this.damage = damageAmount;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); // Kein Ziel mehr -> löschen
            return;
        }

        // Richtung zum Ziel
        Vector3 dir = (target.position - transform.position).normalized;
        float moveStep = speed * Time.deltaTime;

        // Prüfen, ob wir das Ziel erreichen
        if (Vector3.Distance(transform.position, target.position) <= moveStep)
        {
            Explode();
        }
        else
        {
            transform.position += dir * moveStep;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Schadenslogik: hier alle Gegner in Radius treffen
        float radius = 2f; // AOE-Radius, kann über Upgrade veränderbar sein
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyHealth enemy))
            {
                enemy.TakeDamage(damage);
            }
        }

        // VFX erzeugen
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, impactEffectDuration);
        }

        Destroy(gameObject); // Projektil weg
    }

    private void OnDrawGizmosSelected()
    {
        // Optional: Radius in Scene sichtbar machen
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private Rigidbody2D rb;
    private bool fired;
    private Vector2 dir;
    private float spd;

    public void SetDamage(float v) { damage = v; }

    public void Fire(Vector2 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        spd = speed;
        fired = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = dir * spd;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void OnEnable()
    {
        if (fired) rb.linearVelocity = dir * spd;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        var eh = c.collider.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        var eh = c.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 5;
    [SerializeField] private int pierce = 0; // 0 = no pierce

    private Rigidbody2D rb;
    private Vector2 dir;
    private float speed;
    private int pierced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Fire(Vector2 direction, float setSpeed)
    {
        dir = direction.normalized;
        speed = setSpeed;
        pierced = 0;

        rb.linearVelocity = dir * speed;
        CancelInvoke();
        Invoke(nameof(Despawn), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (other.TryGetComponent(out EnemyHealth eh))
            eh.TakeDamage(damage);

        if (pierce <= 0 || ++pierced > pierce)
            Despawn();
    }

    private void Despawn()
    {
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject); // swap to pooling later if desired
    }
}

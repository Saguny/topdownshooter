using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifetime = 2f;
    [SerializeField] private float _damage = 10f;

    private Vector2 _direction;
    private Rigidbody2D _rb;

    public float Speed { get => _speed; set => _speed = value; }

    public void Shoot(Vector2 direction) => _direction = direction.normalized;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Destroy(gameObject, _lifetime);
    }

    private void FixedUpdate()
    {
        if (_rb != null)
            _rb.linearVelocity = _direction * _speed;
        else
            transform.Translate(_direction * _speed * Time.fixedDeltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(_damage);

            Destroy(gameObject);
        }
    }
}

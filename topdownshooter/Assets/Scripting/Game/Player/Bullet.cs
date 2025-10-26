using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifetime = 2f;
    [SerializeField] private float _damage = 10f;

    private Vector2 _direction;

    // expose speed for upgrades
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public void Shoot(Vector2 direction)
    {
        _direction = direction.normalized;
    }

    private void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(_damage);
        }

        if (collision.CompareTag("Player"))

        Destroy(gameObject);
    }
}

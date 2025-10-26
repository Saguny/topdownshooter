using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float _contactDamage = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log("Player takes damage!");
                playerHealth.TakeDamage(_contactDamage);
            }
        }
    }

}

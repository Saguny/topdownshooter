using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float _damage = 5f;
    private float _baseDamage;

    private void Awake()
    {
<<<<<<< Updated upstream
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(_contactDamage);
        }
    }
=======
        _baseDamage = _damage;
    }

    public void SetDamageMultiplier(float mult)
    {
        _damage = _baseDamage * mult;
    }
>>>>>>> Stashed changes
}

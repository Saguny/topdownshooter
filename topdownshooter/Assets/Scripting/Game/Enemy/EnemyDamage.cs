using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float _damage = 5f;
    private float _baseDamage;

    private void Awake()
    {
        _baseDamage = _damage;
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _damage = _baseDamage * multiplier;
    }

    public float GetDamage() => _damage;
}

using UnityEngine;

public class AOEAttack : MonoBehaviour
{
    [Header("AOE Attack Settings")]
    public GameObject projectilePrefab;
    public float attackInterval = 5f;
    public float attackRange = 8f;
    public float damage = 20f;

    [Header("Audio")]
    public AudioClip fireSound;
    [Range(0f, 1f)] public float fireVolume = 0.8f;

    private AudioSource _audio;
    private float _timer;
    private bool _active = false;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            FireProjectile();
        }
    }

    public void Activate()
    {
        _active = true;
        _timer = 0f;
    }

    public void Deactivate()
    {
        _active = false;
    }

    public void Upgrade(float intervalMultiplier, float damageMultiplier)
    {
        attackInterval = Mathf.Max(0.5f, attackInterval * intervalMultiplier);
        damage *= damageMultiplier;
    }

    private void FireProjectile()
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        if (proj.TryGetComponent(out AOEProjectile aoe))
        {
            aoe.Setup(damage, attackRange);
        }

        if (fireSound != null)
            _audio.PlayOneShot(fireSound, fireVolume);
    }
}

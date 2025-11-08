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
            FireAtNearestEnemy();
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

    // 👉 Wird beim Upgrade aufgerufen
    public void Upgrade(float intervalMultiplier, float damageMultiplier)
    {
        attackInterval = Mathf.Max(0.5f, attackInterval * intervalMultiplier);
        damage *= damageMultiplier;
    }

    private void FireAtNearestEnemy()
    {
        GameObject nearest = FindNearestEnemy();
        if (nearest == null) return;

        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (proj.TryGetComponent(out AOEProjectile aoe))
            aoe.Setup(nearest.transform, damage);

        if (fireSound != null)
            _audio.PlayOneShot(fireSound, fireVolume);
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                nearest = e;
                minDist = dist;
            }
        }

        return nearest;
    }
}

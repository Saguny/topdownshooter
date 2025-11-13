using UnityEngine;

public class AOEAttack : MonoBehaviour
{
    [Header("AOE Attack Settings")]
    public GameObject projectilePrefab;
    public float attackInterval = 5f;
    public float attackRange = 8f;
    public float damage = 20f;
    public int projectileCount = 1;

    [Header("Audio")]
    public AudioClip fireSound;
    [Range(0f, 1f)] public float fireVolume = 0.8f;

    [Header("Meteor Targeting")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 viewportXRange = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 viewportYRange = new Vector2(0.2f, 0.9f);
    [SerializeField] private float spawnYOffset = 2f;
    [SerializeField] private float meteorTiltFromVerticalDeg = 16.786f;

    private AudioSource _audio;
    private float _timer;
    private bool _active = false;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            FireProjectiles();
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

    public void Upgrade(float intervalMultiplier, float damageMultiplier, float rangeMultiplier, int extraProjectiles)
    {
        attackInterval = Mathf.Max(0.5f, attackInterval * intervalMultiplier);
        damage *= damageMultiplier;
        attackRange *= rangeMultiplier;
        projectileCount = Mathf.Max(1, projectileCount + extraProjectiles);
    }

    private void FireProjectiles()
    {
        if (projectilePrefab == null || targetCamera == null) return;

        for (int i = 0; i < projectileCount; i++)
        {
            float vx = Random.Range(viewportXRange.x, viewportXRange.y);
            float vy = Random.Range(viewportYRange.x, viewportYRange.y);

            Vector3 impactPos = targetCamera.ViewportToWorldPoint(
                new Vector3(vx, vy, Mathf.Abs(targetCamera.transform.position.z))
            );
            impactPos.z = 0f;

            float worldAngleDeg = -90f + meteorTiltFromVerticalDeg;
            float worldAngleRad = worldAngleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(worldAngleRad), Mathf.Sin(worldAngleRad)).normalized;

            float topY = targetCamera.transform.position.y + targetCamera.orthographicSize;
            float spawnY = topY + spawnYOffset;

            float t = (spawnY - impactPos.y) / (-dir.y);
            Vector3 spawnPos = impactPos - (Vector3)(dir * t);
            spawnPos.z = 0f;

            GameObject proj = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.AngleAxis(worldAngleDeg, Vector3.forward)
            );

            if (proj.TryGetComponent(out AOEProjectile aoe))
            {
                aoe.Setup(damage, attackRange, impactPos, dir);
            }
        }

        if (fireSound != null)
            _audio.PlayOneShot(fireSound, fireVolume);
    }
}

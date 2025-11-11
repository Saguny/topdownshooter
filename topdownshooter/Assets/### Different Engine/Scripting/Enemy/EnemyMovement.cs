using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAwareness))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 3f;
    private float _baseSpeed;

    [Header("Boss Avoidance")]
    [SerializeField] private float _bossAvoidRadius = 2.5f;
    [SerializeField, Range(0f, 3f)] private float _bossAvoidWeight = 1.5f;

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
    private Animator _animator;
    private Vector3 _originalScale;

    // cached boss reference
    private Transform _bossTransform;

    private readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _animator = GetComponent<Animator>();
        if (_animator) _animator.enabled = true;

        _originalScale = transform.localScale;
        _baseSpeed = _speed;

        // find the boss (object with BossMarker component)
        BossMarker bossMarker = FindObjectOfType<BossMarker>();
        if (bossMarker != null)
        {
            _bossTransform = bossMarker.transform;
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = _playerAwareness.AwareOfPlayer
            ? _playerAwareness.DirectionToPlayer
            : Vector2.zero;

        // apply boss avoidance if needed
        direction = ApplyBossAvoidance(direction);

        Move(direction);
        FlipSprite(direction);

        bool isMoving = _rigidbody.linearVelocity.sqrMagnitude > 0.01f;
        if (_animator && _animator.isActiveAndEnabled)
            _animator.SetBool(HashIsRunning, isMoving);
    }

    private Vector2 ApplyBossAvoidance(Vector2 toPlayerDir)
    {
        if (_bossTransform == null)
            return toPlayerDir;

        if (toPlayerDir.sqrMagnitude < 0.0001f)
            return toPlayerDir;

        Vector2 enemyPos = _rigidbody.position;
        Vector2 bossPos = _bossTransform.position;
        Vector2 toBoss = bossPos - enemyPos;

        float distToBoss = toBoss.magnitude;
        if (distToBoss > _bossAvoidRadius || distToBoss <= Mathf.Epsilon)
            return toPlayerDir;

        // base direction towards player
        Vector2 playerDirNorm = toPlayerDir.normalized;

        // direction around boss (perpendicular to the vector towards boss)
        Vector2 aroundDir = Vector2.Perpendicular(toBoss).normalized;

        // choose the side that is more aligned with going to the player
        if (Vector2.Dot(aroundDir, playerDirNorm) < 0f)
            aroundDir = -aroundDir;

        // how strong the avoidance is (stronger when closer)
        float t = 1f - Mathf.Clamp01(distToBoss / _bossAvoidRadius);

        // blend between "go to player" and "go around boss"
        Vector2 blended =
            playerDirNorm * (1f - t) +
            aroundDir * (t * _bossAvoidWeight);

        if (blended.sqrMagnitude < 0.0001f)
            return playerDirNorm;

        return blended.normalized;
    }

    private void Move(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        _rigidbody.linearVelocity = direction.normalized * _speed;
    }

    private void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
        else if (direction.x < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
    }

    public void SetSpeedMultiplier(float mult)
    {
        _speed = _baseSpeed * mult;
    }
}

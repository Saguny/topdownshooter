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

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _avoidanceRadius = 0.5f;
    [SerializeField] private float _avoidanceDistance = 2f;
    [SerializeField, Range(0f, 3f)] private float _avoidanceWeight = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _directionSmoothFactor = 0.2f;

    [Header("Visuals")]
    [SerializeField] private bool _spriteFacesLeftByDefault = true;
    // toggle this: true if your art faces left, false if it faces right

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
    private Animator _animator;
    private SpriteRenderer _sr;
    private Vector3 _originalScale;

    private Transform _bossTransform;
    private Vector2 _smoothedDirection = Vector2.zero;
    private float _avoidanceSide = 0f;

    private readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _animator = GetComponent<Animator>();
        if (_animator != null) _animator.enabled = true;

        _sr = GetComponentInChildren<SpriteRenderer>();
        _originalScale = transform.localScale;
        _baseSpeed = _speed;

        BossMarker bossMarker = FindObjectOfType<BossMarker>();
        if (bossMarker != null)
            _bossTransform = bossMarker.transform;

        // initial flip fix
        if (_sr != null)
            _sr.flipX = _spriteFacesLeftByDefault;
    }

    private void FixedUpdate()
    {
        Vector2 direction = _playerAwareness != null && _playerAwareness.AwareOfPlayer
            ? _playerAwareness.DirectionToPlayer
            : Vector2.zero;

        direction = ApplyBossAvoidance(direction);
        direction = ApplyObstacleAvoidance(direction);
        UpdateSmoothedDirection(direction);

        Move(_smoothedDirection);
        FlipSprite(_smoothedDirection);

        bool isMoving = _rigidbody.linearVelocity.sqrMagnitude > 0.01f;
        if (_animator != null && _animator.isActiveAndEnabled)
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

        Vector2 playerDirNorm = toPlayerDir.normalized;
        Vector2 aroundDir = Vector2.Perpendicular(toBoss).normalized;

        if (Vector2.Dot(aroundDir, playerDirNorm) < 0f)
            aroundDir = -aroundDir;

        float t = 1f - Mathf.Clamp01(distToBoss / _bossAvoidRadius);

        Vector2 blended =
            playerDirNorm * (1f - t) +
            aroundDir * (t * _bossAvoidWeight);

        if (blended.sqrMagnitude < 0.0001f)
            return playerDirNorm;

        return blended.normalized;
    }

    private Vector2 ApplyObstacleAvoidance(Vector2 desiredDir)
    {
        if (desiredDir.sqrMagnitude < 0.0001f)
        {
            _avoidanceSide = 0f;
            return desiredDir;
        }

        Vector2 desiredNorm = desiredDir.normalized;
        Vector2 origin = _rigidbody.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            _avoidanceRadius,
            desiredNorm,
            _avoidanceDistance,
            _obstacleMask
        );

        if (!hit.collider)
        {
            _avoidanceSide = 0f;
            return desiredNorm;
        }

        float t = 1f - Mathf.Clamp01(hit.distance / _avoidanceDistance);
        Vector2 normal = hit.normal.normalized;
        Vector2 around = Vector2.Perpendicular(normal).normalized;

        if (Mathf.Approximately(_avoidanceSide, 0f))
        {
            float sideDot = Vector2.Dot(around, desiredNorm);
            _avoidanceSide = Mathf.Sign(sideDot);

            if (Mathf.Approximately(_avoidanceSide, 0f))
                _avoidanceSide = 1f;
        }

        around *= _avoidanceSide;

        Vector2 blended =
            desiredNorm * (1f - t) +
            around * (t * _avoidanceWeight);

        if (blended.sqrMagnitude < 0.0001f)
            return desiredNorm;

        return blended.normalized;
    }

    private void UpdateSmoothedDirection(Vector2 targetDirection)
    {
        if (targetDirection.sqrMagnitude > 0.0001f)
        {
            Vector2 targetNorm = targetDirection.normalized;

            if (_smoothedDirection.sqrMagnitude < 0.0001f)
            {
                _smoothedDirection = targetNorm;
            }
            else
            {
                _smoothedDirection = Vector2.Lerp(
                    _smoothedDirection,
                    targetNorm,
                    _directionSmoothFactor
                );
            }
        }
        else
        {
            _smoothedDirection = Vector2.zero;
        }
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
        if (_sr == null) return;
        if (Mathf.Abs(direction.x) < 0.01f) return;

        bool movingRight = direction.x > 0f;

        // if sprite art faces left by default, invert logic
        _sr.flipX = _spriteFacesLeftByDefault ? movingRight : !movingRight;
    }

    public void SetSpeedMultiplier(float mult)
    {
        _speed = _baseSpeed * mult;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAwareness))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    private float _baseSpeed;

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
    private Animator _animator;
    private Vector3 _originalScale;

    private readonly int HashIsRunning = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _animator = GetComponent<Animator>();
        if (_animator) _animator.enabled = true;

        _originalScale = transform.localScale;
        _baseSpeed = _speed;
    }

    private void FixedUpdate()
    {
        Vector2 direction = _playerAwareness.AwareOfPlayer ? _playerAwareness.DirectionToPlayer : Vector2.zero;

        Move(direction);
        FlipSprite(direction);

        bool isMoving = _rigidbody.linearVelocity.sqrMagnitude > 0.01f;
        if (_animator && _animator.isActiveAndEnabled) _animator.SetBool(HashIsRunning, isMoving);
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

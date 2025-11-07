using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAwareness))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    private float _baseSpeed; // 追加

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
    private Animator _animator;
    private Vector3 _originalScale;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _animator = GetComponent<Animator>();
        _originalScale = transform.localScale;

        _baseSpeed = _speed; // ここで保存
    }

    private void FixedUpdate()
    {
        Vector2 direction = _playerAwareness.AwareOfPlayer
            ? _playerAwareness.DirectionToPlayer
            : Vector2.zero;

        Move(direction);
        FlipSprite(direction);

        bool isMoving = _rigidbody.linearVelocity.sqrMagnitude > 0.01f;
        _animator.SetBool("IsRunning", isMoving);
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
        {
            transform.localScale = new Vector3(
                Mathf.Abs(_originalScale.x),
                _originalScale.y,
                _originalScale.z
            );
        }
        else if (direction.x < -0.01f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(_originalScale.x),
                _originalScale.y,
                _originalScale.z
            );
        }
    }

    // 追加：毎回「基準×倍率」で決定（累積しない）
    public void SetSpeedMultiplier(float mult)
    {
        _speed = _baseSpeed * mult;
    }
}

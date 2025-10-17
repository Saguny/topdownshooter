using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
    private Vector2 _targetDirection;

    private Vector3 _originalScale;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _originalScale = transform.localScale; 
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        FlipSprite();
        SetVelocity();
    }

    private void UpdateTargetDirection()
    {
        if (_playerAwareness.AwareOfPlayer)
        {
            _targetDirection = _playerAwareness.DirectionToPlayer;
        }
        else
        {
            _targetDirection = Vector2.zero;
        }
    }

    private void FlipSprite()
    {
        if (_targetDirection.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z); // face right
        else if (_targetDirection.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z); // face left
    }

    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }
        else
        {
            // move towards player
            _rigidbody.linearVelocity = _targetDirection.normalized * _speed;
        }
    }
}

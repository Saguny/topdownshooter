using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator _animator;

    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;
    private Vector2 _smoothedMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private Vector3 _originalScale;
    private Vector3 _originalFirePointLocalPos;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _originalScale = transform.localScale;

        if (firePoint != null)
            _originalFirePointLocalPos = firePoint.localPosition;
    }

    [System.Obsolete]
    private void FixedUpdate()
    {
        SetPlayerVelocity();
        FlipSprite();
    }

    private void SetPlayerVelocity()
    {
        _smoothedMovementInput = Vector2.SmoothDamp(
            _smoothedMovementInput,
            _movementInput,
            ref _movementInputSmoothVelocity,
            0.1f);

        _rigidbody.linearVelocity = _smoothedMovementInput * _speed;
    }

    private void FlipSprite()
    {
        if (_movementInput.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            if (firePoint != null)
                firePoint.localPosition = _originalFirePointLocalPos; // reset to original
        }
        else if (_movementInput.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            if (firePoint != null)
            {
                Vector3 flipped = _originalFirePointLocalPos;
                flipped.x *= -1;
                firePoint.localPosition = flipped;
            }
        }
    }

    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();

        bool isMoving = _movementInput.sqrMagnitude > 0.01f;

        _animator.SetBool("IsRunning", isMoving);
    }
}

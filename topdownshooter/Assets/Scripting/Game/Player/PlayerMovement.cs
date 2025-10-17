using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;
    private Vector2 _smoothedMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private Vector3 _originalScale;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _originalScale = transform.localScale;
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
        if(_movementInput.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
        }
        if (_movementInput.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }
}

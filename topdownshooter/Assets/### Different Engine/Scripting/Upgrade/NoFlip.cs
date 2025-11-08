using UnityEngine;

public class NoFlip : MonoBehaviour
{
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        // keep scale absolute (so it never flips)
        Vector3 parentScale = transform.parent != null ? transform.parent.localScale : Vector3.one;
        transform.localScale = new Vector3(
            Mathf.Abs(_originalScale.x) * Mathf.Sign(parentScale.x) < 0 ? -_originalScale.x : _originalScale.x,
            _originalScale.y,
            _originalScale.z
        );

        // simpler version: always force positive X
        transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
    }
}

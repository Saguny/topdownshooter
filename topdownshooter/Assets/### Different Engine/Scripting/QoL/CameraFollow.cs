using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private float _cameraZ = -10f; // fixed z-distance from player
    [SerializeField] private Vector2 _offset = Vector2.zero; // optional x/y offset

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPos = new Vector3(
            _target.position.x + _offset.x,
            _target.position.y + _offset.y,
            _cameraZ // keep this constant!
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, _smoothSpeed * Time.deltaTime);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.25f;

    private float _nextFireTime = 0f;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnFire(InputValue value)
    {
        if (Time.time < _nextFireTime) return;

        Shoot();
        _nextFireTime = Time.time + fireRate;
    }

    private void Shoot()
    {
        // Mausposition in Weltkoordinaten
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        // Richtung vom FirePoint zur Maus
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        // Kugel erzeugen
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetDirection(direction);

        // Kugel in Flugrichtung drehen (optional, nur optisch)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}

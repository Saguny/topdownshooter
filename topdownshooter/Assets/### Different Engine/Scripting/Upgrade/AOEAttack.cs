using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class AOEAttack : MonoBehaviour
{
    [Header("AOE Attack Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private int projectileCount = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip projectileSound;
    [Range(0f, 1f)][SerializeField] private float projectileVolume = 0.8f;
    [SerializeField] private AudioClip splashSound;
    [Range(0f, 1f)][SerializeField] private float splashVolume = 0.8f;

    [Header("Meteor Targeting")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 viewportXRange = new Vector2(0.1f, 0.9f);
    [SerializeField] private Vector2 viewportYRange = new Vector2(0.2f, 0.9f);
    [SerializeField] private float spawnYOffset = 2f;
    [SerializeField] private float meteorTiltFromVerticalDeg = 16.786f;

    private AudioSource _audio;
    private float _timer;
    private bool _active;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (!_active) return;

        _timer += Time.deltaTime;
        if (_timer >= attackInterval)
        {
            _timer = 0f;
            FireProjectiles();
        }
    }

    public void Activate()
    {
        _active = true;
        _timer = 0f;
    }

    public void Deactivate()
    {
        _active = false;
    }

    public void Upgrade(float intervalMultiplier, float damageMultiplier, float rangeMultiplier, int extraProjectiles)
    {
        attackInterval = Mathf.Max(0.5f, attackInterval * intervalMultiplier);
        damage *= damageMultiplier;
        attackRange *= rangeMultiplier;
        projectileCount = Mathf.Max(1, projectileCount + extraProjectiles);
    }


    private void FireProjectiles()
    {
        if (projectilePrefab == null || targetCamera == null) return;

        for (int i = 0; i < projectileCount; i++)
        {
            float vx = UnityEngine.Random.Range(viewportXRange.x, viewportXRange.y);
            float vy = UnityEngine.Random.Range(viewportYRange.x, viewportYRange.y);

            Vector3 impactPos = targetCamera.ViewportToWorldPoint(
                new Vector3(vx, vy, Mathf.Abs(targetCamera.transform.position.z))
            );
            impactPos.z = 0f;

            float worldAngleDeg = -90f + meteorTiltFromVerticalDeg;
            float worldAngleRad = worldAngleDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(worldAngleRad), Mathf.Sin(worldAngleRad)).normalized;

            float topY = targetCamera.transform.position.y + targetCamera.orthographicSize;
            float spawnY = topY + spawnYOffset;
            float t = (spawnY - impactPos.y) / (-dir.y);

            Vector3 spawnPos = impactPos - (Vector3)(dir * t);
            spawnPos.z = 0f;

            GameObject proj = Instantiate(
                projectilePrefab,
                spawnPos,
                Quaternion.AngleAxis(worldAngleDeg, Vector3.forward)
            );

            if (proj.TryGetComponent(out AOEProjectile aoe))
            {
                aoe.Setup(damage, attackRange, impactPos, dir, splashSound, splashVolume);
            }
        }

        if (projectileSound != null)
            _audio.PlayOneShot(projectileSound, projectileVolume);
    }
}
